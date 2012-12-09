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
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using ServiceBusMQ.Manager;

namespace ServiceBusMQ {
  public class SbmqSystem {

    IMessageManager _mgr;
    CommandHistoryManager _history;
    UIStateConfig _uiState = new UIStateConfig();

    private SbmqSystem() {
    }

    private void Init() {
      AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

      Config = SystemConfig.Load();

      _mgr = MessageBusFactory.Create(Config.MessageBus, Config.MessageBusQueueType);
      _mgr.ErrorOccured += MessageMgr_ErrorOccured;
      _mgr.ItemsChanged += _mgr_ItemsChanged;

      _mgr.Init(Config.ServerName, Config.WatchCommandQueues, Config.WatchEventQueues, Config.WatchMessageQueues, Config.WatchErrorQueues,
                                Config.CommandDefinition);

      _history = new CommandHistoryManager();
    }

    static SbmqSystem _instance;
    public static SbmqSystem Create() {
      _instance = new SbmqSystem();
      _instance.Init();

      return _instance;
    }


    Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
      string asmName = args.Name.Split(',')[0];

      if( Config != null ) {
        foreach( var path in Config.CommandsAssemblyPaths ) {
          var fileName = string.Format("{0}\\{1}.dll", path, asmName);

          try {

            if( File.Exists(fileName) ) {
              return Assembly.LoadFrom(fileName);
            }

          } catch { }
        }
      }

      var fn = string.Format("{0}\\{1}.dll", Assembly.GetExecutingAssembly().Location, asmName);
      if( File.Exists(fn) ) {
        return Assembly.LoadFrom(fn);
      }


      throw new ApplicationException("Failed resolving assembly, " + args.Name);
    }


    void _mgr_ItemsChanged(object sender, EventArgs e) {
      _itemsChanged.Invoke(sender, e);
      //Dispatcher.CurrentDispatcher.BeginInvoke( _itemsChanged );
    }

    private void MessageMgr_ErrorOccured(object sender, ErrorArgs e) {

      MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

      if( e.Fatal )
        Application.Current.Shutdown();

    }


    public IMessageManager Manager { get { return _mgr; } }
    public SystemConfig1 Config { get; private set; }
    public CommandHistoryManager SavedCommands { get { return _history; } }
    public UIStateConfig UIState { get { return _uiState; } }

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


    protected EventHandler _itemsChanged;

    public event EventHandler ItemsChanged {
      [MethodImpl(MethodImplOptions.Synchronized)]
      add {
        _itemsChanged = (EventHandler)Delegate.Combine(_itemsChanged, value);
      }
      [MethodImpl(MethodImplOptions.Synchronized)]
      remove {
        _itemsChanged = (EventHandler)Delegate.Remove(_itemsChanged, value);
      }
    }

    //public event EventHandler<EventArgs> ItemsChanged;
    //protected void OnItemsChanged() {
    //  if( ItemsChanged != null )
    //    ItemsChanged(this, EventArgs.Empty);
    //}


  }

}
