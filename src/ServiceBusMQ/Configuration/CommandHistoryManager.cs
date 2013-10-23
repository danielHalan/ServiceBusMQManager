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
using KellermanSoftware.CompareNetObjects;
using Newtonsoft.Json;
using ServiceBusMQ.Configuration;

namespace ServiceBusMQ {


  [Serializable]
  public class CommandHistoryManager {

    string _itemsFolder;
    string _itemsFile;
    string _itemsFileV2;

    //AppDomain _appDomain;
    List<SavedCommandItem3> _items;
    SystemConfig3 _config;

    public IEnumerable<SavedCommandItem3> Items {
      get {
        if( _items == null )
          Load();

        return _items;
      }
    }

    public CommandHistoryManager(SystemConfig3 config) {
      _config = config;

      _itemsFolder = SbmqSystem.AppDataPath + @"\savedCommands\";
      _itemsFileV2 = SbmqSystem.AppDataPath + "savedCommands.dat";
      _itemsFile = SbmqSystem.AppDataPath + "savedCommands3.dat";

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

      if( File.Exists(_itemsFile) ) {

        Load3();

      } else if( File.Exists(_itemsFileV2) ) {
        Load2();

      } else Load1(); // v1

    }

    private void Load3() {
      var file = JsonFile.Read<SavedCommandItems3>(_itemsFile);

      _items = new List<SavedCommandItem3>(file.Items);

      SavedCommandItem3[] removed = _items.Where(i => !File.Exists(i.FileName)).ToArray();
      removed.ForEach(i => _items.Remove(i));
    }

    private void Load2() {
      var file = JsonFile.Read<SavedCommandItems>(_itemsFileV2);

      _items = file.Items.Select( i => new SavedCommandItem3(i.Id, i.DisplayName, i.FileName, i.Pinned, i.LastSent) ).ToList();

      SavedCommandItem3[] removed = _items.Where(i => !File.Exists(i.FileName)).ToArray();
      removed.ForEach(i => _items.Remove(i));
    }


    private void Load1() {
      _items = new List<SavedCommandItem3>();

      foreach( var file in Directory.GetFiles(_itemsFolder, "*.cmd") ) {
        try {
          var cmd = JsonFile.Read<SavedCommand>(file);
          var cmd3 = new SavedCommand3() { 
            ConnectionStrings = new Dictionary<string, string> { { "server", cmd.Server } },
            ServiceBus = cmd.ServiceBus, 
            Queue = cmd.Queue, 
            Transport = cmd.Transport, 
            Command = cmd.Command };

          var item = new SavedCommandItem3(cmd.DisplayName, file, false, cmd.LastSent);

          item.SetCommand(cmd3);

          _items.Add(item);
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

      SavedCommandItems3 file = new SavedCommandItems3();
      List<SavedCommandItem3> fileItems = new List<SavedCommandItem3>();

      foreach( SavedCommandItem3 item in _items.OrderByDescending(c => c.LastSent).Take(50) ) {
        if( !item.FileName.IsValid() )
          item.FileName = GetAvailableFileName();

        fileItems.Add(item);

        if( item.IsNew )
          JsonFile.Write(item.FileName, item.SentCommand);
      }

      file.Items = fileItems.ToArray();

      JsonFile.Write(_itemsFile, file);
    }

    private string GetAvailableFileName() {
      string fileName;

      int i = 0;
      do {
        fileName = string.Format("{0}{1}.cmd", _itemsFolder, ++i);
      } while( File.Exists(fileName) );

      return fileName;
    }

    public void RenameCommand(Guid id, string newDisplayName) {

      var co = new CompareObjects();

      var item = _items.SingleOrDefault( i => i.Id == id );

      if( item != null ) {

        item.DisplayName = newDisplayName;

        Save();
      }


    }

    public SavedCommandItem3 AddCommand(object command, string serviceBus, string transport, Dictionary<string, string> connectionStrings, string queue) {
      SavedCommandItem3 item = null;

      var co = new CompareObjects();

      foreach( var c in _items ) {

        if( co.Compare(c.SentCommand, command) ) {
          item = c; // TODO: When we show what ServiceBus/Server/Queue the command has been sent to, 
          // then also compare those values
          break;
        }
      }

      if( item == null ) {
        item = new SavedCommandItem3(command.GetType().GetDisplayName(command).CutEnd(70), null, false, DateTime.Now);

        SavedCommand3 cmd = new SavedCommand3();
        cmd.Command = command;

        cmd.ServiceBus = serviceBus;
        cmd.Transport = transport;
        cmd.ConnectionStrings = connectionStrings;
        cmd.Queue = queue;

        item.SetCommand(cmd);

        _items.Insert(0, item);
      }

      item.LastSent = DateTime.Now;

      Save();

      return item;
    }

    public void Remove(SavedCommandItem3 item) {

      if( item.FileName.IsValid() )
        File.Delete(item.FileName);

      _items.Remove(item);

    }
  }
}
