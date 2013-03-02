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
using Newtonsoft.Json;
using ServiceBusMQ.Configuration;

namespace ServiceBusMQ {

  [Serializable]
  public class SavedCommandItems {
    public int Version { get; set; }
    public SavedCommandItem[] Items { get; set; }
  }

  [Serializable]
  public class SavedCommandItem {
    public Guid Id { get; set; }
    public string DisplayName { get; set; }
    public string FileName { get; set; }
    public bool Pinned { get; set; }
    public DateTime LastSent { get; set; }


    [JsonConstructor]
    public SavedCommandItem() {
      IsNew = false;
    }

    public SavedCommandItem(string displayName, string fileName, bool pinned, DateTime lastSent) { 
      Id = Guid.NewGuid();

      DisplayName = displayName;
      FileName = fileName;
      Pinned = pinned;
      LastSent = lastSent;

      IsNew = true;
    }

    SavedCommand2 _sentCommand = null;
    
    [JsonIgnore]
    public SavedCommand2 SentCommand {
      get {
        if( _sentCommand == null )
          _sentCommand = JsonFile.Read<SavedCommand2>(FileName);
        
        return _sentCommand;
      }
    }

    public void SetCommand(SavedCommand2 cmd) {
      _sentCommand = cmd;

      IsNew = true;
    }

    [JsonIgnore]
    public bool IsNew { get; private set; }
  }


  [Serializable]
  public class CommandHistoryManager {

    string _itemsFolder;
    string _itemsFile;

    //AppDomain _appDomain;
    List<SavedCommandItem> _items;
    SystemConfig2 _config;

    public IEnumerable<SavedCommandItem> Items {
      get {
        if( _items == null )
          Load();

        return _items;
      }
    }

    public CommandHistoryManager(SystemConfig2 config) {
      _config = config;

      _itemsFolder = SbmqSystem.AppDataPath + @"\savedCommands\";
      _itemsFile = SbmqSystem.AppDataPath + "savedCommands.dat";

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

        var file = JsonFile.Read<SavedCommandItems>(_itemsFile);

        _items = new List<SavedCommandItem>(file.Items);
        RemoveDeletedItems();

      } else Load1(); // v1

    }

    private void RemoveDeletedItems() {
      SavedCommandItem[] removed = _items.Where( i => !File.Exists(i.FileName) ).ToArray();

      removed.ForEach( i => _items.Remove(i) );
    }

    private void Load1() {
      _items = new List<SavedCommandItem>();

      foreach( var file in Directory.GetFiles(_itemsFolder, "*.cmd") ) {
        try {
          var cmd = JsonFile.Read<SavedCommand>(file);
          var cmd2 = new SavedCommand2() { 
            Server = cmd.Server,
            ServiceBus = cmd.ServiceBus, 
            Queue = cmd.Queue, 
            Transport = cmd.Transport, 
            Command = cmd.Command };

          var item = new SavedCommandItem(cmd.DisplayName, file, false, cmd.LastSent);

          item.SetCommand(cmd2);

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

      SavedCommandItems file = new SavedCommandItems() { Version = 2 };
      List<SavedCommandItem> fileItems = new List<SavedCommandItem>();

      foreach( SavedCommandItem item in _items.OrderByDescending(c => c.LastSent).Take(50) ) {
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

    public SavedCommandItem AddCommand(object command, string serviceBus, string transport, string server, string queue) {
      SavedCommandItem item = null;

      var co = new CompareObjects();

      foreach( var c in _items ) {

        if( co.Compare(c.SentCommand, command) ) {
          item = c; // TODO: When we show what ServiceBus/Server/Queue the command has been sent to, 
          // then also compare those values
          break;
        }
      }

      if( item == null ) {
        item = new SavedCommandItem(command.GetType().GetDisplayName(command).CutEnd(70), null, false, DateTime.Now);

        SavedCommand2 cmd = new SavedCommand2();
        cmd.Command = command;

        cmd.ServiceBus = serviceBus;
        cmd.Transport = transport;
        cmd.Server = server;
        cmd.Queue = queue;

        item.SetCommand(cmd);

        _items.Insert(0, item);
      }

      item.LastSent = DateTime.Now;

      Save();

      return item;
    }

    public void Remove(SavedCommandItem item) {

      if( item.FileName.IsValid() )
        File.Delete(item.FileName);

      _items.Remove(item);

    }
  }
}
