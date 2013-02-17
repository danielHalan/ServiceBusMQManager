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
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using ServiceBusMQ.Configuration;
using ServiceBusMQ.Manager;
using ServiceBusMQ.Model;
using ServiceBusMQ.ViewModel;

namespace ServiceBusMQ {

  public class SbmqSystem {


    bool _isServiceBusStarted = false;

    IServiceBusManager _mgr;
    CommandHistoryManager _history;
    static UIStateConfig _uiState = new UIStateConfig();


    List<QueueItemViewModel> _items = new List<QueueItemViewModel>();

    public IServiceBusManager Manager { get { return _mgr; } }
    public SystemConfig2 Config { get; private set; }
    public CommandHistoryManager SavedCommands { get { return _history; } }
    public static UIStateConfig UIState { get { return _uiState; } }

    public List<QueueItemViewModel> Items { get { return _items; } }

    public bool CanSendCommand { get; private set; }
    public bool CanViewSubscriptions { get; private set; }
    
    private SbmqSystem() {
    }

    private void Initialize() {
      AppDomain.CurrentDomain.AssemblyResolve += SbmqmDomain_AssemblyResolve;
      MonitorQueueType = new bool[4];

      Config = SystemConfig.Load();

      Config.StartCount += 1;

      _mgr = ServiceBusFactory.CreateManager(Config.MessageBus, Config.MessageBusQueueType);
      _mgr.ErrorOccured += SbMgr_ErrorOccured;
      _mgr.ItemsChanged += SbMgr_ItemsChanged;

      _mgr.Initialize(Config.MonitorServer, Config.MonitorQueues.Select( mq => new Queue(mq.Name, mq.Type, mq.Color) ).ToArray());

      CanSendCommand = ( _mgr as ISendCommand ) != null;
      CanViewSubscriptions = ( _mgr as IViewSubscriptions ) != null; 

      _history = new CommandHistoryManager(Config);
    }

    private static SbmqSystem _instance;
    public static SbmqSystem Create() {
      _instance = new SbmqSystem();
      _instance.Initialize();

      return _instance;
    }


    public IServiceBusDiscovery GetDiscoveryService() {
      return ServiceBusFactory.CreateDiscovery(Config.MessageBus, Config.MessageBusQueueType);
    }


    protected volatile object _itemsLock = new object();
    public void UpdateUnprocessedQueueItemList() {

      if( !MonitorQueueType.Any( mq => mq ) || _mgr.MonitorQueues.Length == 0 )
        return;

      List<QueueItem> items = new List<QueueItem>();

      // TODO: Solve why we can not iterate thru Remote MQ, 
      // both GetMessageEnumerator2() and GetAllMessages() should be available for
      // Remote computer and direct format name, but returns zero (0) messages in some cases
      //if( !Tools.IsLocalHost(_serverName) )
      //  return;

      IEnumerable<QueueItem> currentItems = _items.AsEnumerable<QueueItem>();
      foreach( QueueType t in Enum.GetValues(typeof(QueueType)) )
        items.AddRange(_mgr.GetUnprocessedMessages(t, currentItems));

      // Newest first
      if( items.Count > 1 )
        items.Sort((a, b) => b.ArrivedTime.CompareTo(a.ArrivedTime));

      bool changed = false;
      lock( _itemsLock ) {

        // Add new items
        foreach( var itm in items ) {
          var existingItem = _items.SingleOrDefault(i => i.Id == itm.Id);

          if( existingItem == null ) {

            _items.Insert(0, new QueueItemViewModel(itm));
            changed = true;

          } else if( existingItem.Processed ) {

            _items.Remove(existingItem);
            itm.Processed = false;

            _items.Insert(0, new QueueItemViewModel(itm));
            changed = true;
          }

        }

        // Mark removed as deleted messages
        foreach( var itm in _items )
          if( !items.Any(i2 => i2.Id == itm.Id) ) {

            if( !itm.Processed ) {
              itm.Processed = true;
              changed = true;
            }
          }

      }

      if( changed )
        OnItemsChanged();

    }

    public void GetProcessedQueueItems(TimeSpan timeSpan) {
      if( _mgr.MonitorQueues.Length == 0 )
        return;

      List<QueueItem> items = new List<QueueItem>();

      // TODO: Solve why we can not iterate thru Remote MQ, 
      // both GetMessageEnumerator2() and GetAllMessages() should be available for
      // Remote computer and direct format name, but returns zero (0) messages always
      //if( !Tools.IsLocalHost(_serverName) )
      //  return;
      DateTime since = DateTime.Now - timeSpan;

      foreach( QueueType t in Enum.GetValues(typeof(QueueType)) )
        if( MonitorQueueType[(int)t] )
          items.AddRange(_mgr.GetProcessedMessages(t, since, _items.AsEnumerable<QueueItem>()));

      bool changed = false;
      lock( _itemsLock ) {

        // Add new items
        foreach( var itm in items )
          if( !_items.Any(i => i.Id == itm.Id) ) {

            _items.Insert(0, new QueueItemViewModel(itm));
            changed = true;
          }

        // Mark removed as deleted messages
        foreach( var itm in _items )
          if( !items.Any(i2 => i2.Id == itm.Id) ) {

            if( !itm.Processed ) {
              itm.Processed = true;
              changed = true;
            }
          }

      }

      if( changed ) {
        _items.Sort((a, b) => b.ArrivedTime.CompareTo(a.ArrivedTime));

        OnItemsChanged();
      }
    }

    public void ClearProcessedItems() {
      foreach( var itm in _items.Where(i => i.Processed).ToArray() )
        _items.Remove(itm);
    }


    private void SbMgr_ErrorOccured(object sender, ErrorArgs e) {

      MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

      if( e.Fatal )
        Application.Current.Shutdown();

    }
    private void SbMgr_ItemsChanged(object sender, EventArgs e) {
      _itemsChanged.Invoke(sender, e);
      //Dispatcher.CurrentDispatcher.BeginInvoke( _itemsChanged );
    }

    protected void OnItemsChanged() {
      if( _itemsChanged != null )
        _itemsChanged(this, EventArgs.Empty);
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


    public Type[] GetAvailableCommands() {
      var sc = _mgr as ISendCommand;
      if( sc != null )
        return sc.GetAvailableCommands(Config.CommandsAssemblyPaths, Config.CommandDefinition);
      else return new Type[0];
    }
    public Type[] GetAvailableCommands(string[] _asmPath, CommandDefinition cmdDef) {
      var sc = _mgr as ISendCommand;
      if( sc != null )
        return sc.GetAvailableCommands(_asmPath, cmdDef);
      else return new Type[0];
    }

    
    public MessageSubscription[] GetMessageSubscriptions(string serverName) {
      var sc = _mgr as IViewSubscriptions;
      if( sc != null )
        return sc.GetMessageSubscriptions(serverName);
      else return new MessageSubscription[0];
    }


    public void SendCommand(string destinationServer, string destinationQueue, object message) {
      var sc = _mgr as ISendCommand;
      if( sc != null ) {
        
        if( !_isServiceBusStarted ) {
          sc.SetupServiceBus(Config.CommandsAssemblyPaths, Config.CommandDefinition);
          _isServiceBusStarted = true;
        }

        sc.SendCommand(destinationServer, destinationQueue, message);
      }
    }



    private Assembly SbmqmDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
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


    bool[] MonitorQueueType { get; set; }

    public bool MonitorCommands {
      get { return (bool)MonitorQueueType[(int)QueueType.Command]; }
      set { MonitorQueueType[(int)QueueType.Command] = value; UpdateItems(QueueType.Command, value); }
    }

    public bool MonitorEvents {
      get { return (bool)MonitorQueueType[(int)QueueType.Event]; }
      set { MonitorQueueType[(int)QueueType.Event] = value; UpdateItems(QueueType.Event, value); }
    }

    public bool MonitorMessages {
      get { return (bool)MonitorQueueType[(int)QueueType.Message]; }
      set { MonitorQueueType[(int)QueueType.Message] = value; UpdateItems(QueueType.Message, value); }
    }

    public bool MonitorErrors {
      get { return (bool)MonitorQueueType[(int)QueueType.Error]; ; }
      set { MonitorQueueType[(int)QueueType.Error] = value; UpdateItems(QueueType.Error, value); }
    }


    private void UpdateItems(QueueType type, bool value) {

      if( !value )
        foreach( var itm in _items.Where(i => i.Queue.Type == type).ToArray() )
          _items.Remove(itm);
    }



  }

}
