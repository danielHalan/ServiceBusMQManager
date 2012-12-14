#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    NServiceBusMSMQManager.cs
  Created: 2012-09-23

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using NServiceBus.Utils;
using ServiceBusMQ;
using ServiceBusMQ.Model;

namespace ServiceBusMQ.NServiceBus {

  //[PermissionSetAttribute(SecurityAction.LinkDemand, Name = "FullTrust")]
  public class NServiceBus_MSMQ_Manager : NServiceBusManagerBase {

    class PeekThreadParam {
      public QueueType QueueType { get; set; }
      public MessageQueue Queue { get; set; }
    }

    bool _disposed = false;

    public NServiceBus_MSMQ_Manager() {
    }

    public override void Init(string serverName, string[] commandQueues, string[] eventQueues, string[] messageQueues, string[] errorQueues, CommandDefinition commandDef) {
      base.Init(serverName, commandQueues, eventQueues, messageQueues, errorQueues, commandDef);

      StartPeekThreads();
    }

    public override void Dispose() {
      base.Dispose();

      _disposed = true;
    }

    void StartPeekThreads() {
      foreach( QueueType qt in Enum.GetValues(typeof(QueueType)) ) {

        if( qt != QueueType.Error ) {
          foreach( var q in GetQueueListByType(qt) ) {
            var t = new Thread(new ParameterizedThreadStart(PeekMessages));
            if( q.CanRead ) {
              t.Name = "peek-msmq-" + q.GetDisplayName();
              t.Start(new PeekThreadParam() { Queue = q, QueueType = qt });
            }
          }


        }
      }
    }

    public void PeekMessages(object prm) {
      PeekThreadParam p = prm as PeekThreadParam;
      string qName = p.Queue.GetDisplayName();
      uint sameCount = 0;
      string lastId = string.Empty;

      bool _isPeeking = false;

      SetupMessageReadPropertyFilters(p.Queue, p.QueueType);

      p.Queue.PeekCompleted += (source, asyncResult) => {
        if( IsMonitoring(p.QueueType) ) {
          Message msg = p.Queue.EndPeek(asyncResult.AsyncResult);

          if( msg.Id == lastId )
            sameCount++;

          else {
            sameCount = 0;
            TryAddItem(msg, qName, p.QueueType);
          }

          if( lastId != msg.Id )
            lastId = msg.Id;

        } 
        _isPeeking = false;
      };

      while( !_disposed ) {

        while( !IsMonitoring(p.QueueType) ) {
          Thread.Sleep(1000);

          if( _disposed )
            return;
        }


        if( !_isPeeking ) {

          if( sameCount > 0 ) {
            if( sameCount / 10.0F == 1.0F ) 
              Thread.Sleep(100);

            else if( sameCount / 100.0F == 1.0F ) 
              Thread.Sleep(200);

            else if( sameCount % 300 == 0 ) 
              Thread.Sleep(500);
          }
          p.Queue.BeginPeek();
          _isPeeking = true;
        }

        Thread.Sleep(100);
      }


    }

    private bool TryAddItem(Message msg, string qName, QueueType queueType) {

      lock( _itemsLock ) {

        if( !_items.Any(i => i.Id == msg.Id) ) {

          var itm = CreateQueueItem(qName, msg, queueType);

          AddQueueItem(_items, itm);

          OnItemsChanged();

          return true;

        } else return false;
      }

    }

    public override string[] GetAllAvailableQueueNames(string server) {
      return MessageQueue.GetPrivateQueuesByMachine(server).Where(q => !IsIgnoredQueue(q.QueueName)).
          Select(q => q.QueueName.Replace("private$\\", "")).ToArray();
    }
    public override bool CanAccessQueue(string server, string queueName) {
      var queue = CreateMessageQueue(server, queueName, QueueAccessMode.ReceiveAndAdmin);

      return queue != null ? queue.CanRead : false;
    }

    
    private MessageQueue CreateMessageQueue(string serverName, string queueName, QueueAccessMode accessMode) {
      if( !queueName.StartsWith("private$\\") )
        queueName = "private$\\" + queueName;

      queueName = string.Format("FormatName:DIRECT=OS:{0}\\{1}", !Tools.IsLocalHost(serverName) ? serverName : ".", queueName);

      return new MessageQueue(queueName, false, true, accessMode);
    }



    protected override void LoadQueues() {
      try {
        _eventQueues.Clear();
        _cmdQueues.Clear();
        _msgQueues.Clear();
        _errorQueues.Clear();

        foreach( var name in _watchEventQueues ) 
          _eventQueues.Add(CreateMessageQueue(_serverName, name, QueueAccessMode.ReceiveAndAdmin));

        foreach( var name in _watchCommandQueues )
          _cmdQueues.Add(CreateMessageQueue(_serverName, name, QueueAccessMode.ReceiveAndAdmin));

        foreach( var name in _watchMessageQueues )
          _msgQueues.Add(CreateMessageQueue(_serverName, name, QueueAccessMode.ReceiveAndAdmin));

        foreach( var name in _watchErrorQueues )
          _errorQueues.Add(CreateMessageQueue(_serverName, name, QueueAccessMode.ReceiveAndAdmin));

      } catch( Exception e ) {
        OnError("Error occured when loading queues", e, true);
      }

    }


    private List<QueueItem> LoadSubscriptionQueues(IList<MessageQueue> queues, QueueType type, IList<QueueItem> currentItems) {
      if( queues.Count == 0 )
        return EMPTY_LIST;

      List<QueueItem> r = new List<QueueItem>();

      foreach( var q in queues ) {
        string qName = q.GetDisplayName();

        if( !IsSubscriptionQueue(qName) )
          continue;

        SetupMessageReadPropertyFilters(q, type);

        try {
          foreach( var msg in q.GetAllMessages() ) {

            QueueItem itm = currentItems.SingleOrDefault(i => i.Id == msg.Id);

            if( itm == null )
              itm = CreateQueueItem(qName, msg, type);

            AddQueueItem(r, itm);
          }
        } catch( Exception e ) {
          OnError("Error occured when processing subscription queue item", e, true);
        }
      }

      return r;
    }

    private void SetupMessageReadPropertyFilters(MessageQueue q, QueueType type) {
      
      q.MessageReadPropertyFilter.ArrivedTime = true;
      q.MessageReadPropertyFilter.Label = true;
      q.MessageReadPropertyFilter.Body = true;
      
      if( type == QueueType.Error )
        q.MessageReadPropertyFilter.Extension = true;
    }

    protected override IEnumerable<QueueItem> DoFetchQueueItems(IList<MessageQueue> queues, QueueType type, IList<QueueItem> currentItems) {
      if( queues.Count == 0 )
        return EMPTY_LIST;

      List<QueueItem> r = new List<QueueItem>();

      foreach( var q in queues ) {
        string qName = q.GetDisplayName();

        if( IsIgnoredQueue(qName) || !q.CanRead )
          continue;

        SetupMessageReadPropertyFilters(q, type);

        try {
          foreach( var msg in q.GetAllMessages() ) {

            QueueItem itm = currentItems.SingleOrDefault(i => i.Id == msg.Id);

            if( itm == null )
              itm = CreateQueueItem(qName, msg, type);

            AddQueueItem(r, itm);
          }
        } catch( Exception e ) {
          OnError("Error occured when processing queue " + qName + ", " + e.Message, e, false);
        }

      }

      return r;
    }


    private void AddQueueItem(List<QueueItem> r, QueueItem itm) {

      if( !IsIgnoredQueueItem(itm) ) {

        itm.MessageNames = GetMessageNames(itm.Content, true);
        itm.DisplayName = MergeStringArray(GetMessageNames(itm.Content, false)).Default(itm.Label);

        r.Add(itm);
      }
    }

    private static readonly XmlSerializer headerSerializer = new XmlSerializer(typeof(List<HeaderInfo>));

    private QueueItem CreateQueueItem(string queueName, Message msg, QueueType type) {
      var itm = new QueueItem();
      itm.DisplayName = msg.Label;
      itm.QueueDisplayName = queueName.CutBeginning(46);
      itm.QueueName = queueName;
      itm.QueueType = type;
      itm.Label = msg.Label;
      itm.Id = msg.Id;
      itm.ArrivedTime = msg.ArrivedTime;
      itm.Content = ReadMessageStream(msg.BodyStream);
      
      if( type == QueueType.Error ) { // Check for error msg 

        itm.Headers = new Dictionary<string, string>();
        if( msg.Extension.Length > 0 ) {
          var stream = new MemoryStream(msg.Extension);
          var o = headerSerializer.Deserialize(stream);

          foreach( var pair in o as List<HeaderInfo> )
            if( pair.Key != null )
              itm.Headers.Add(pair.Key, pair.Value);
        }

        itm.Error = new QueueItemError();
        try { 
          itm.Error.Message = itm.Headers.SingleOrDefault( k => k.Key == "NServiceBus.ExceptionInfo.Message" ).Value;
          itm.Error.Retries = Convert.ToInt32(itm.Headers.SingleOrDefault(k => k.Key == "NServiceBus.Retries").Value);
          //itm.Error.TimeOfFailure = Convert.ToDateTime(itm.Headers.SingleOrDefault(k => k.Key == "NServiceBus.TimeOfFailure").Value);
        } catch {
          itm.Error = null;
        }
      }

      return itm;
    }

    private bool IsSubscriptionQueue(string queueName) {
      return ( queueName.EndsWith("subscriptions") );
    }


    private MessageQueue GetMessageQueue(QueueItem itm) {
      List<MessageQueue> qs = null;

      switch( itm.QueueType ) {
        case QueueType.Command: qs = _cmdQueues; break;
        case QueueType.Event: qs = _eventQueues; break;
        case QueueType.Message: qs = _msgQueues; break;
        case QueueType.Error: qs = _errorQueues; break;
      }

      return qs.Single(i => i.FormatName.EndsWith(itm.QueueName));
    }

    public override string LoadMessageContent(QueueItem itm) {
      if( itm.Content == null ) {

        MessageQueue mq = GetMessageQueue(itm);

        mq.MessageReadPropertyFilter.ArrivedTime = false;
        mq.MessageReadPropertyFilter.Body = true;
        itm.Content = !itm.Deleted ? ReadMessageStream(mq.PeekById(itm.Id).BodyStream) : "DELETED";
      }

      return itm.Content;
    }


    public override MessageSubscription[] GetMessageSubscriptions(string server) {

      List<MessageSubscription> r = new List<MessageSubscription>();

      foreach( var queueName in MessageQueue.GetPrivateQueuesByMachine(server).
                                            Where(q => q.QueueName.EndsWith(".subscriptions")).Select( q => q.QueueName ) ) {

        MessageQueue q = CreateMessageQueue(server, queueName, QueueAccessMode.ReceiveAndAdmin);

        q.MessageReadPropertyFilter.Label = true;
        q.MessageReadPropertyFilter.Body = true;

        try {
          var publisher = q.GetDisplayName().Replace(".subscriptions", string.Empty);

          foreach( var msg in q.GetAllMessages() ) {

            var itm = new MessageSubscription();
            itm.FullName = GetSubscriptionType(ReadMessageStream(msg.BodyStream));
            itm.Name = ParseClassName(itm.FullName);
            itm.Subscriber = msg.Label;
            itm.Publisher = publisher;

            r.Add(itm);
          }
        } catch( Exception e ) {
          OnError("Error occured when processing queue item", e, true);
        }

      }

      return r.ToArray();
    }

    private string ParseClassName(string asmName) {

      if( asmName.IsValid() ) {

        int iEnd = asmName.IndexOf(',');
        int iStart = asmName.LastIndexOf('.', iEnd);

        if( iEnd > -1 && iStart > -1 ) {
          iStart++;
          return asmName.Substring(iStart, iEnd - iStart);
        }

      }

      return asmName;
    }

    public override void PurgeErrorMessages(string queueName) {
      string name = "private$\\" + queueName;

      _errorQueues.Where(q => q.GetDisplayName() == name).Single().Purge();

      OnItemsChanged();
    }
    public override void PurgeErrorAllMessages() {
      _errorQueues.ForEach(q => q.Purge());

      OnItemsChanged();
    }

    public override void PurgeMessage(QueueItem itm) {
      var q = GetMessageQueue(itm);
      q.ReceiveById(itm.Id);

      itm.Deleted = true;

      OnItemsChanged();
    }
    public override void PurgeAllMessages() {
      _cmdQueues.ForEach(q => q.Purge());
      _eventQueues.ForEach(q => q.Purge());
      _msgQueues.ForEach(q => q.Purge());

      OnItemsChanged();
    }

    public override string BusName { get { return "NServiceBus"; } }
    public override string BusQueueType { get { return "MSMQ"; } }



  }
}
