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
using System.Text;
using System.Threading.Tasks;
using ServiceBusMQ.Model;

namespace ServiceBusMQ.Manager {

  //public delegate void Error

  public abstract class MessageManagerBase : IMessageManager {

    protected List<QueueItem> EMPTY_LIST = new List<QueueItem>();

    protected List<QueueItem> _items = new List<QueueItem>();
    public List<QueueItem> Items { get { return _items; } }    

    protected string _serverName;

    protected string[] _watchEventQueues;
    protected string[] _watchCommandQueues;
    protected string[] _watchMessageQueues;
    protected string[] _watchErrorQueues;


    public string[] EventQueues { get { return _watchEventQueues; } }
    public string[] CommandQueues { get { return _watchCommandQueues; } }
    public string[] MessageQueues { get { return _watchMessageQueues; } }
    public string[] ErrorQueues { get { return _watchErrorQueues; } }


    bool _monitorCommands, _monitorEvents, _monitorMessages, _monitorErrors;

    public bool MonitorCommands { get { return _monitorCommands; } set { _monitorCommands = value; UpdateItems(QueueType.Command, value); } }
    public bool MonitorEvents { get { return _monitorEvents; } set { _monitorEvents = value; UpdateItems(QueueType.Event, value); } }
    public bool MonitorMessages { get { return _monitorMessages; } set { _monitorMessages = value; UpdateItems(QueueType.Message, value); } }
    public bool MonitorErrors { get { return _monitorErrors; } set { _monitorErrors = value; UpdateItems(QueueType.Error, value); } }


    public event EventHandler<EventArgs> ItemsChanged;
    protected void OnItemsChanged() {
      if( ItemsChanged != null )
        ItemsChanged(this, EventArgs.Empty);
    }


    public virtual void Init(string serverName, string[] watchCommandQueues, string[] watchEventQueues, string[] watchMessageQueues, string[] watchErrorQueues) {

      _serverName = serverName;
      _watchCommandQueues = watchCommandQueues;
      _watchEventQueues = watchEventQueues;
      _watchMessageQueues = watchMessageQueues;
      _watchErrorQueues = watchErrorQueues;

      LoadQueues();
    }


    private void UpdateItems(QueueType type, bool value) {

      if( !value ) 
        foreach( var itm in _items.Where(i => i.QueueType == type).ToArray() )
          _items.Remove(itm);
    }

    protected abstract void LoadQueues();


    IEnumerable<QueueItem> ProcessQueue(QueueType type) {
      //int iType = (int)type;

      if( IsMonitoring(type) ) {
        var fetched = FetchQueueItems(type, _items);

        //string count = string.Format("({0})", fetched.Count());
        //if( ( btn.Content as string ).Contains(count) )
        //  btn.Content = string.Concat(BUTTON_LABELS[iType], SPACE_SEPARATOR, count);

        return fetched;

      } else {
        //btn.Content = BUTTON_LABELS[iType];

        return EMPTY_LIST;
      }
    }

    private bool IsMonitoring(QueueType type) {
      switch(type) {
        case QueueType.Command: return MonitorCommands;
        case QueueType.Event: return MonitorEvents;
        case QueueType.Message: return MonitorMessages;
        case QueueType.Error: return MonitorErrors;
      }

      return false;
    }


    public void RefreshQueueItems() {

      if( _watchEventQueues.Length == 0 && _watchCommandQueues.Length == 0 && _watchMessageQueues.Length == 0 )
        OnError("No queues has been configured. \n\nPlease add the queues you want to monitor in ServiceBusMQManager.exe.config, and try again.", true);

      List<QueueItem> items = new List<QueueItem>();

      foreach( QueueType t in Enum.GetValues(typeof(QueueType)) )
        items.AddRange(ProcessQueue(t));


      // Add new items
      bool changed = false;
      foreach( var itm in items )
        if( !_items.Any(i => i.Id == itm.Id) ) {
          _items.Insert(0, itm);
          changed = true;
        }

      // Mark removed as deleted messages
      foreach( var itm in _items )
        if( !items.Any(i2 => i2.Id == itm.Id && !itm.Deleted) ) {
          itm.Deleted = true;
          changed = true;
        }


      if( changed ) 
        OnItemsChanged();


    }
    
    public void ClearDeletedItems() {
      foreach( var itm in _items.Where(i => i.Deleted).ToArray() )
        _items.Remove(itm);
    }


    protected abstract IEnumerable<QueueItem> FetchQueueItems(QueueType type, IList<QueueItem> currentItems);

    public abstract string LoadMessageContent(QueueItem itm);


    public abstract bool IsIgnoredQueue(string queueName);
    public abstract bool IsIgnoredQueueItem(QueueItem itm);


    public abstract void PurgeMessage(QueueItem itm);
    public abstract void PurgeAllMessages();

    public abstract void PurgeErrorMessages(string queueName);
    public abstract void PurgeErrorAllMessages();


    public abstract void MoveErrorItemToOriginQueue(QueueItem itm);
    public abstract void MoveAllErrorItemsToOriginQueue(string errorQueue);

    public abstract string BusName { get; }
    public abstract string BusQueueType { get; }

    public event EventHandler<ErrorArgs> ErrorOccured;

    protected void OnError(string message, bool fatal) {
      if( ErrorOccured != null )
        ErrorOccured(this, new ErrorArgs(message, fatal));
    }

  }
}
