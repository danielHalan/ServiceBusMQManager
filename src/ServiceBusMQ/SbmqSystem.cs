#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    SbmqSystem.cs
  Created: 2012-11-27

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using ServiceBusMQ.Manager;

namespace ServiceBusMQ {
  public class SbmqSystem {

    IMessageManager _mgr;
    CommandHistoryManager _history;

    public SbmqSystem() {
    }

    public void Init() {
      AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
      
      Config = new SystemConfig();
      Config.Load();

      _mgr = MessageBusFactory.Create(Config.MessageBus, Config.MessageBusQueueType);
      _mgr.ErrorOccured += MessageMgr_ErrorOccured;
      _mgr.ItemsChanged += _mgr_ItemsChanged;
      
      _mgr.Init(Config.ServerName, Config.WatchCommandQueues, Config.WatchEventQueues, Config.WatchMessageQueues, Config.WatchErrorQueues);

      _history = new CommandHistoryManager();

    }


    Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
      string asmName = args.Name.Split(',')[0];

      foreach( var path in Config.CommandsAssemblyPaths ) {
        var fileName = string.Format("{0}\\{1}.dll", path, asmName);

        try {

          if( File.Exists(fileName) ) {
            return Assembly.LoadFrom(fileName);
          }

        } catch { }

      }

      throw new ApplicationException("Failed resolving assembly, " + args.Name);
    }


    void _mgr_ItemsChanged(object sender, EventArgs e) {
      OnItemsChanged();
    }

    private void MessageMgr_ErrorOccured(object sender, ErrorArgs e) {

      MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

      if( e.Fatal )
        Application.Current.Shutdown();

    }


    public IMessageManager Manager { get { return _mgr; } }
    public SystemConfig Config { get; private set; }
    public CommandHistoryManager HistoryManager { get { return _history; } }

    static string _appDataPath = null;
    public static string AppDataPath {
      get {
        if( _appDataPath == null ) {
          _appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SBMQM\";

          if( !Directory.Exists(_appDataPath) )
            Directory.CreateDirectory(_appDataPath);
        }

        return _appDataPath;
      }

    }


    public event EventHandler<EventArgs> ItemsChanged;
    protected void OnItemsChanged() {
      if( ItemsChanged != null )
        ItemsChanged(this, EventArgs.Empty);
    }


  }

}
