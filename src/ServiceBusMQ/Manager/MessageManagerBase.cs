#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    MessageManagerBase.cs
  Created: 2012-09-23

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ServiceBusMQ.Model;
using ServiceBusMQ.ViewModel;

namespace ServiceBusMQ.Manager {

  //public delegate void Error

  public abstract class MessageManagerBase : IMessageManager {

    protected List<QueueItem> EMPTY_LIST = new List<QueueItem>();

    protected volatile object _itemsLock = new object();
    protected volatile List<QueueItemViewModel> _items = new List<QueueItemViewModel>();

    public List<QueueItemViewModel> Items { get { return _items; } }

    protected string _serverName;

    protected Queue[] _monitorQueues;
    public Queue[] MonitorQueues { get { return _monitorQueues; } }

    protected CommandDefinition _commandDef;

    public bool MonitorCommands {
      get { return (bool)MonitorQueueTypes[QueueType.Command]; }
      set { MonitorQueueTypes[QueueType.Command] = value; UpdateItems(QueueType.Command, value); }
    }

    public bool MonitorEvents {
      get { return (bool)MonitorQueueTypes[QueueType.Event]; }
      set { MonitorQueueTypes[QueueType.Event] = value; UpdateItems(QueueType.Event, value); }
    }

    public bool MonitorMessages {
      get { return (bool)MonitorQueueTypes[QueueType.Message]; }
      set { MonitorQueueTypes[QueueType.Message] = value; UpdateItems(QueueType.Message, value); }
    }

    public bool MonitorErrors {
      get { return (bool)MonitorQueueTypes[QueueType.Error]; ; }
      set { MonitorQueueTypes[QueueType.Error] = value; UpdateItems(QueueType.Error, value); }
    }

    public System.Collections.Hashtable MonitorQueueTypes { get; private set; }


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

    protected void OnItemsChanged() {
      if( _itemsChanged != null )
        _itemsChanged(this, EventArgs.Empty);
    }


    public virtual void Init(string serverName, Queue[] monitorQueues, CommandDefinition commandDef) {

      _serverName = serverName;
      _monitorQueues = monitorQueues;

      MonitorQueueTypes = InitMonitorQueueTypes();

      _commandDef = commandDef;

      LoadQueues();
    }

    private System.Collections.Hashtable InitMonitorQueueTypes() {
      var h = new System.Collections.Hashtable();
      h.Add(QueueType.Command, true);
      h.Add(QueueType.Event, true);
      h.Add(QueueType.Message, false);
      h.Add(QueueType.Error, true);

      return h;
    }
    public virtual void Dispose() { }


    private void UpdateItems(QueueType type, bool value) {

      if( !value )
        foreach( var itm in _items.Where(i => i.Queue.Type == type).ToArray() )
          _items.Remove(itm);
    }

    protected abstract void LoadQueues();

    public abstract string[] GetAllAvailableQueueNames(string server);
    public abstract bool CanAccessQueue(string server, string queueName);


    IEnumerable<QueueItem> ProcessQueue(QueueType type) {

      if( IsMonitoring(type) ) {
        var fetched = FetchQueueItems(type, _items);

        return fetched;

      } else {

        return EMPTY_LIST;
      }
    }

    protected bool IsMonitoring(QueueType type) {
      switch( type ) {
        case QueueType.Command: return MonitorCommands;
        case QueueType.Event: return MonitorEvents;
        case QueueType.Message: return MonitorMessages;
        case QueueType.Error: return MonitorErrors;
      }

      return false;
    }

    protected abstract IEnumerable<QueueItem> GetProcessedQueueItems(QueueType type, DateTime since, IEnumerable<QueueItem> currentItems);


    public void LoadProcessedQueueItems(TimeSpan timeSpan) {
      if( _monitorQueues.Length == 0 )
        return;

      List<QueueItem> items = new List<QueueItem>();

      // TODO: Solve why we can not iterate thru Remote MQ, 
      // both GetMessageEnumerator2() and GetAllMessages() should be available for
      // Remote computer and direct format name, but returns zero (0) messages always
      //if( !Tools.IsLocalHost(_serverName) )
      //  return;
      DateTime since = DateTime.Now - timeSpan;

      foreach( QueueType t in Enum.GetValues(typeof(QueueType)) )
        if( IsMonitoring(t) )
          items.AddRange(GetProcessedQueueItems(t, since, _items.AsEnumerable<QueueItem>()));

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


    public void RefreshQueueItems() {

      if( _monitorQueues.Length == 0 )
        return;

      List<QueueItem> items = new List<QueueItem>();

      // TODO: Solve why we can not iterate thru Remote MQ, 
      // both GetMessageEnumerator2() and GetAllMessages() should be available for
      // Remote computer and direct format name, but returns zero (0) messages in some cases
      //if( !Tools.IsLocalHost(_serverName) )
      //  return;

      foreach( QueueType t in Enum.GetValues(typeof(QueueType)) )
        items.AddRange(ProcessQueue(t));

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

    public void ClearProcessedItems() {
      foreach( var itm in _items.Where(i => i.Processed).ToArray() )
        _items.Remove(itm);
    }


    protected abstract IEnumerable<QueueItem> FetchQueueItems(QueueType type, IEnumerable<QueueItem> currentItems);

    public abstract string GetMessageContent(QueueItem itm);

    public abstract MessageContentFormat MessageContentFormat { get; }


    public abstract void PurgeMessage(QueueItem itm);
    public abstract void PurgeAllMessages();

    public abstract void PurgeErrorMessages(string queueName);
    public abstract void PurgeErrorAllMessages();


    public abstract void MoveErrorItemToOriginQueue(QueueItem itm);
    public abstract void MoveAllErrorItemsToOriginQueue(string errorQueue);

    public abstract string BusName { get; }
    public abstract string BusQueueType { get; }

    public event EventHandler<ErrorArgs> ErrorOccured;

    protected void OnError(string message, string stackTrace, bool fatal) {
      if( ErrorOccured != null )
        ErrorOccured(this, new ErrorArgs(message, stackTrace, fatal));
    }
    protected void OnError(string message, Exception e, bool fatal) {
      OnError(string.Format("{0}\n\r {1} ({2})", message, e.Message, e.GetType().Name), e.StackTrace, fatal);
    }

    public abstract Type[] GetAvailableCommands(string[] asmPaths);
    public abstract Type[] GetAvailableCommands(string[] asmPaths, CommandDefinition commandDef);
    public abstract void SetupServiceBus(string[] asmPaths);
    public abstract void SendCommand(string destinationServer, string destinationQueue, object message);


    public abstract MessageSubscription[] GetMessageSubscriptions(string server);

  }

}
