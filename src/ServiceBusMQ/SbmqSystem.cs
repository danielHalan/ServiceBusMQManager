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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ServiceBusMQ.Configuration;
using ServiceBusMQ.Manager;
using ServiceBusMQ.Model;
using ServiceBusMQ.ViewModel;

namespace ServiceBusMQ {

  public enum ItemChangeOrigin { Queue, Filter }
  public class ItemsChangedEventArgs : EventArgs {

    public ItemChangeOrigin Origin { get; set; }

  }

  public class SbmqSystem {

    public static readonly int MAX_ITEMS_PER_QUEUE = 500;

    bool _isServiceBusStarted = false;

    IServiceBusManager _mgr;
    CommandHistoryManager _history;
    static UIStateConfig _uiState = new UIStateConfig();
    private SbmqmMonitorState _monitorState;


    List<QueueItemViewModel> _items = new List<QueueItemViewModel>();

    public IServiceBusManager Manager { get { return _mgr; } }
    public SystemConfig2 Config { get; private set; }
    public CommandHistoryManager SavedCommands { get { return _history; } }
    public static UIStateConfig UIState { get { return _uiState; } }

    string _filter = null;

    public IEnumerable<QueueItemViewModel> Items {
      get {
        return _items.Where(i => _filter == null ||
          i.DisplayName.Contains(_filter.Split(' ')));
      }
    }

    public bool CanSendCommand { get; private set; }
    public bool CanViewSubscriptions { get; private set; }

    public static ApplicationInfo AppInfo { get; set; }

    private SbmqSystem() {
      _UnprocessedItemsCount = new uint[4];
    }


    private void Initialize() {
      AppDomain.CurrentDomain.AssemblyResolve += SbmqmDomain_AssemblyResolve;
      _monitorState = new SbmqmMonitorState();

      Config = SystemConfig.Load();

      Config.StartCount += 1;

      _mgr = ServiceBusFactory.CreateManager(Config.MessageBus, Config.MessageBusQueueType);
      _mgr.ErrorOccured += System_ErrorOccured;
      _mgr.ItemsChanged += System_ItemsChanged;

      _mgr.Initialize(Config.MonitorServer, Config.MonitorQueues.Select(mq => new Queue(mq.Name, mq.Type, mq.Color)).ToArray(), _monitorState);

      CanSendCommand = ( _mgr as ISendCommand ) != null;
      CanViewSubscriptions = ( _mgr as IViewSubscriptions ) != null;

      _history = new CommandHistoryManager(Config);

      AppInfo = new ApplicationInfo(Config.Id, Assembly.GetEntryAssembly());
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


    volatile ThreadState _currentMonitor = null;
    //bool _monitoring = false;
    public void StartMonitoring() {

      //_monitoring = true;
      _currentMonitor = new ThreadState();
      var t = new Thread(ExecMonitor);
      t.Name = "Queue Monitoring";
      t.Start(_currentMonitor);

    }
    public void StopMonitoring() {
      //_monitoring = false;
      _currentMonitor.Executing = false;
      _currentMonitor = null;
    }

    internal class ThreadState {
      public bool Executing { get; set; }

      public bool Stopped { get; set; }

      internal ThreadState() {
        Executing = true;
      }
    }

    public void ExecMonitor(object prm) {
      var state = prm as ThreadState;

      try {
        while( state.Executing ) {

          if( RefreshUnprocessedQueueItemList() )
            OnItemsChanged(ItemChangeOrigin.Queue);

          Thread.Sleep(Config.MonitorInterval);
        }

      } finally {
        state.Stopped = true;
      }
    }


    uint[] _UnprocessedItemsCount { get; set; }


    public uint GetUnprocessedItemsCount(QueueType qt) {
      return _UnprocessedItemsCount[(int)qt];
    }


    public bool RefreshUnprocessedQueueItemList() {

      if( !_monitorState.MonitorQueueType.Any(mq => mq) || _mgr.MonitorQueues.Length == 0 )
        return false;

      List<QueueItem> items = new List<QueueItem>();

      // TODO: Solve why we can not iterate thru Remote MQ, 
      // both GetMessageEnumerator2() and GetAllMessages() should be available for
      // Remote computer and direct format name, but returns zero (0) messages in some cases
      //if( !Tools.IsLocalHost(_serverName) )
      //  return;
      bool changedItemsCount = false;

      IEnumerable<QueueItem> currentItems = _items.AsEnumerable<QueueItem>();
      foreach( QueueType t in Enum.GetValues(typeof(QueueType)) ) {
        int typeIndex = (int)t;
        if( _monitorState.MonitorQueueType[typeIndex] ) {
          var r = _mgr.GetUnprocessedMessages(t, currentItems);
          items.AddRange(r.Items);

          if( _UnprocessedItemsCount[typeIndex] != r.Count )
            changedItemsCount = true;

          _UnprocessedItemsCount[typeIndex] = r.Count;
        }
      }

      // Oldest first
      if( items.Count > 1 )
        items.Sort((a, b) => a.ArrivedTime.CompareTo(b.ArrivedTime));

      bool changed = false;
      lock( _itemsLock ) {

        // Add new items
        foreach( var itm in items ) {
          var existingItem = _items.SingleOrDefault(i => i.Id == itm.Id);

          if( existingItem == null ) {

            _items.Insert(0, new QueueItemViewModel(itm));

            if( !changed )
              changed = true;

          } else if( existingItem.Processed ) {

            // It has been retried, move to top
            _items.Remove(existingItem);
            existingItem.Processed = false;

            _items.Insert(0, existingItem);

            if( !changed )
              changed = true;
          }

        }

        // Mark removed as deleted messages
        foreach( var itm in _items )
          if( !items.Any(i2 => i2.Id == itm.Id) ) {

            if( !itm.Processed ) {
              itm.Processed = true;

              if( !changed )
                changed = true;
            }
          }

      }

      return changed || changedItemsCount;
    }
    public void RetrieveProcessedQueueItems(TimeSpan timeSpan) {
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
        if( _monitorState.MonitorQueueType[(int)t] ) {
          var r = _mgr.GetProcessedMessages(t, since, _items.AsEnumerable<QueueItem>());
          items.AddRange(r.Items);
        }

      bool changed = false;
      lock( _itemsLock ) {

        // Add new items
        foreach( var itm in items )
          if( !_items.Any(i => i.Id == itm.Id) ) {

            _items.Add(new QueueItemViewModel(itm));

            if( !changed )
              changed = true;
          }

      }

      if( changed ) {
        _items.Sort((a, b) => b.ArrivedTime.CompareTo(a.ArrivedTime));

        OnItemsChanged(ItemChangeOrigin.Queue);
      }
    }

    public void ClearProcessedItems() {
      foreach( var itm in _items.Where(i => i.Processed).ToArray() )
        _items.Remove(itm);
    }


    private void System_ErrorOccured(object sender, ErrorArgs e) {

      OnError(e);

      if( e.Fatal )
        Application.Current.Shutdown();
    }
    private void System_ItemsChanged(object sender, EventArgs e) {
      OnItemsChanged(ItemChangeOrigin.Queue);
      //_itemsChanged.Invoke(sender, e);
    }

    protected void OnItemsChanged(ItemChangeOrigin origin) {
      if( _itemsChanged != null )
        _itemsChanged(this, new ItemsChangedEventArgs() { Origin = origin });
    }

    protected EventHandler<ItemsChangedEventArgs> _itemsChanged;
    public event EventHandler<ItemsChangedEventArgs> ItemsChanged {
      [MethodImpl(MethodImplOptions.Synchronized)]
      add {
        _itemsChanged = (EventHandler<ItemsChangedEventArgs>)Delegate.Combine(_itemsChanged, value);
      }
      [MethodImpl(MethodImplOptions.Synchronized)]
      remove {
        _itemsChanged = (EventHandler<ItemsChangedEventArgs>)Delegate.Remove(_itemsChanged, value);
      }
    }


    public Type[] GetAvailableCommands(bool suppressErrors = false) {
      var sc = _mgr as ISendCommand;
      if( sc != null )
        return sc.GetAvailableCommands(Config.CommandsAssemblyPaths, Config.CommandDefinition, suppressErrors);
      else return new Type[0];
    }
    public Type[] GetAvailableCommands(string[] _asmPath, CommandDefinition cmdDef, bool suppressErrors = false) {
      var sc = _mgr as ISendCommand;
      if( sc != null )
        return sc.GetAvailableCommands(_asmPath, cmdDef, suppressErrors);
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

      if( !args.Name.StartsWith("mscorlib.XmlSerializers") )
        throw new ApplicationException("Failed resolving assembly, " + args.Name);

      return null;
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


    public bool MonitorCommands {
      get { return (bool)_monitorState.MonitorQueueType[(int)QueueType.Command]; }
      set { MonitorStateChanged(QueueType.Command, value); }
    }
    public bool MonitorEvents {
      get { return (bool)_monitorState.MonitorQueueType[(int)QueueType.Event]; }
      set { MonitorStateChanged(QueueType.Event, value); }
    }
    public bool MonitorMessages {
      get { return (bool)_monitorState.MonitorQueueType[(int)QueueType.Message]; }
      set { MonitorStateChanged(QueueType.Message, value); }
    }
    public bool MonitorErrors {
      get { return (bool)_monitorState.MonitorQueueType[(int)QueueType.Error]; ; }
      set { MonitorStateChanged(QueueType.Error, value); }
    }


    private void MonitorStateChanged(QueueType type, bool value) {
      _monitorState.MonitorQueueType[(int)type] = value;

      if( !value )
        foreach( var itm in _items.Where(i => i.Queue.Type == type).ToArray() )
          _items.Remove(itm);
    }

    public event EventHandler<ErrorArgs> ErrorOccured;

    protected void OnError(string message, Exception exception = null, bool fatal = false) {
      if( ErrorOccured != null )
        ErrorOccured(this, new ErrorArgs(message, exception, fatal));
    }
    protected void OnError(ErrorArgs arg) {
      if( ErrorOccured != null )
        ErrorOccured(this, arg);
    }



    public void FilterItems(string str) {

      if( str.IsValid() ) {
        _filter = str;
        OnItemsChanged(ItemChangeOrigin.Filter);

      } else ClearFilter();

    }

    public void ClearFilter() {
      _filter = null;
      OnItemsChanged(ItemChangeOrigin.Filter);
    }




    public void PurgeAllMessages() {

      Task.Factory.StartNew((data) => {
        ThreadState s = data as ThreadState;

        StopMonitoring();
        while( !s.Stopped )
          Thread.Sleep(100);

        try {
          _mgr.PurgeAllMessages();

        } finally {
          StartMonitoring();
        }

      }, _currentMonitor);


    }

    public void PurgeErrorAllMessages() {

      Task.Factory.StartNew((data) => {
        ThreadState s = data as ThreadState;

        StopMonitoring();
        while( !s.Stopped )
          Thread.Sleep(100);

        try {
          _mgr.PurgeErrorAllMessages();

        } finally {
          StartMonitoring();
        }

      }, _currentMonitor);

    }

    public void PurgeMessage(QueueItem itm) {
      _mgr.PurgeMessage(itm);
    }
  }

}
