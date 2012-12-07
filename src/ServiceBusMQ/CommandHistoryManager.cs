#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    CommandHistoryManager.cs
  Created: 2012-12-04

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KellermanSoftware.CompareNetObjects;

namespace ServiceBusMQ {
  public class CommandHistoryManager {


    string _itemsFolder;

    List<SavedCommand> _items = new List<SavedCommand>();

    public IEnumerable<SavedCommand> Items { get { return _items.OrderByDescending( i => i.LastSent ); } }

    public CommandHistoryManager() {

      _itemsFolder = SbmqSystem.AppDataPath + @"\savedCommands\";

      if( !Directory.Exists(_itemsFolder) )
        Directory.CreateDirectory(_itemsFolder);

      Load();
    }

    private void Load() {

      foreach( var file in Directory.GetFiles(_itemsFolder, "*.cmd") ) {
        try {
          _items.Add( JsonFile.Read<SavedCommand>(file) );
        } catch { }
      }

    }

    public void Save() {

      foreach( var cmd in _items ) {
        if( !cmd.FileName.IsValid() )
          cmd.FileName = GetAvailableFileName();

        JsonFile.Write(cmd.FileName, cmd);
      }

    }

    private string GetAvailableFileName() {
      string fileName;
      
      int i = 0;
      do {
        fileName = string.Format("{0}{1}.cmd", _itemsFolder, ++i);
      } while( File.Exists( fileName ) );
      
      return string.Format("{0}.cmd", i);
    }

    public void RenameCommand(string displayName, object command) {

      var co = new CompareObjects();

      foreach( var c in _items ) {

        if( co.Compare(c.Command, command) ) {
          c.DisplayName = displayName;

          Save();
          break;
        }
      }

    }

    public SavedCommand AddCommand(object command, string serviceBus, string transport, string server, string queue) {
      SavedCommand cmd = null;

      var co = new CompareObjects();

      foreach( var c in _items ) {
      
        if( co.Compare(c.Command, command) ) {
          cmd = c; // TODO: when we show what SB/Server/Q the command has been sent to, then also compare those values
          break;
        }
      }

      if( cmd == null ) {
        cmd = new SavedCommand();      
        
        cmd.DisplayName = command.GetType().GetDisplayName(command).CutEnd(70);
        cmd.Command = command;
      
        cmd.ServiceBus = serviceBus;
        cmd.Transport = transport;
        cmd.Server = server;
        cmd.Queue = queue;

        _items.Add(cmd);
      }

      cmd.LastSent = DateTime.Now;

      Save();

      return cmd;
    }

    public void Remove(SavedCommand item) {
      _items.Remove(item);
      
      Save();
    }
  }
}
