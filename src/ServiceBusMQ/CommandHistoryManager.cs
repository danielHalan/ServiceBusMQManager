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
using System.Reflection;
using System.Text;
using KellermanSoftware.CompareNetObjects;

namespace ServiceBusMQ {

  [Serializable]
  public class CommandHistoryManager {


    string _itemsFolder;

    //AppDomain _appDomain;
    List<SavedCommand> _items;
    SystemConfig1 _config;

    public IEnumerable<SavedCommand> Items { 
      get {
        if( _items == null )
          Load();
        
        return _items;
      } 
    }

    public CommandHistoryManager(SystemConfig1 config) {
      _config = config;

      _itemsFolder = SbmqSystem.AppDataPath + @"\savedCommands\";

      if( !Directory.Exists(_itemsFolder) )
        Directory.CreateDirectory(_itemsFolder);

    }

    private void Load() {
      /*
      _appDomain = AppDomain.CreateDomain("CommandHistoryManager",
                      new System.Security.Policy.Evidence(AppDomain.CurrentDomain.Evidence),
                      AppDomain.CurrentDomain.BaseDirectory,
                      null, true);
      _appDomain.SetData("CommandsAssemblyPaths", _config.CommandsAssemblyPaths);
      _appDomain.AssemblyResolve += AppDomain_AssemblyResolve;
      
       var binder = new AppDomainBinder(_appDomain);
      */

      _items = new List<SavedCommand>();
      

      foreach( var file in Directory.GetFiles(_itemsFolder, "*.cmd") ) {
        try {
          var cmd = JsonFile.Read<SavedCommand>(file);
          cmd.FileName = file;

          _items.Add(cmd);
        } catch { }
      }

      _items = _items.OrderByDescending(i => i.LastSent).ToList(); 
    }

    /*
    static Assembly AppDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
      string asmName = args.Name.Split(',')[0];
      var appDomain = AppDomain.CurrentDomain;
      var commandsAssemblyPaths = (string[])appDomain.GetData("CommandsAssemblyPaths");

      if( commandsAssemblyPaths != null ) {
        foreach( var path in commandsAssemblyPaths ) {
          var fileName = string.Format("{0}\\{1}.dll", path, asmName);

          try {

            if( File.Exists(fileName) ) {

              return appDomain.Load(new AssemblyName() { CodeBase = fileName });
              
              //return Assembly.LoadFrom(fileName);
            }

          } catch { }
        }
      }

      var fn = string.Format("{0}\\{1}.dll", Assembly.GetExecutingAssembly().Location, asmName);
      if( File.Exists(fn) ) {
        return appDomain.Load(new AssemblyName() { CodeBase = fn });
        //return Assembly.LoadFrom(fn);
      }


      throw new ApplicationException("Failed resolving assembly, " + args.Name);
    }
    */


    public void Unload() {
      _items.Clear();
      _items = null;
      
      // Would be nice to implement, releasing all command DLLs
      //AppDomain.Unload(_appDomain);
    }

    public void Save() {

      foreach( var cmd in _items.OrderByDescending(c => c.LastSent).Take(50) ) {
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
      } while( File.Exists(fileName) );

      return fileName;
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
          cmd = c; // TODO: When we show what ServiceBus/Server/Queue the command has been sent to, 
          // then also compare those values
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

        _items.Insert(0, cmd);
      }

      cmd.LastSent = DateTime.Now;

      Save();

      return cmd;
    }

    public void Remove(SavedCommand item) {

      if( item.FileName.IsValid() )
        File.Delete(item.FileName);

      _items.Remove(item);

    }
  }
}
