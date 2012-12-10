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
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServiceBusMQ;
using ServiceBusMQ.Model;

namespace ServiceBusMQManager.MessageBus.NServiceBus {

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

      var queueTypes = Enum.GetValues(typeof(QueueType));
      List<MessageQueue> list = null;

      foreach( QueueType qt in queueTypes ) {

        list = GetQueueListByType(qt);

        foreach( var q in list ) {
          var t = new Thread(new ParameterizedThreadStart(PeekMessages));
          t.Name = "PEEK-MSMQ-" + q.QueueName;
          t.Start(new PeekThreadParam() { Queue = q, QueueType = qt });
        }
      }

    }

    public void PeekMessages(object prm) {
      PeekThreadParam p = prm as PeekThreadParam;
      string qName = p.Queue.QueueName.Replace("private$\\", "");
      int sameCount = 0;
      string lastId = string.Empty;

      bool _isPeeking = false;

      p.Queue.MessageReadPropertyFilter.ArrivedTime = true;
      p.Queue.MessageReadPropertyFilter.Label = true;
      p.Queue.MessageReadPropertyFilter.Body = true;


      p.Queue.PeekCompleted += (source, asyncResult) => {
        if( IsMonitoring(p.QueueType) ) {
          Message msg = p.Queue.EndPeek(asyncResult.AsyncResult);

          if( msg.Id == lastId )
            sameCount++;

          else TryAddItem(msg, qName, p.QueueType);

          if( lastId != msg.Id )
            lastId = msg.Id;

          if( sameCount == 10 ) { // Nobody picking up the message
            sameCount = 0;
            Thread.Sleep(500);
          }

          p.Queue.BeginPeek();
        
        } else _isPeeking = false;
      };


      while( !_disposed ) {

        while( !IsMonitoring(p.QueueType) ) {
          Thread.Sleep(1000);

          if( _disposed )
            return;
        }

        if( !_isPeeking ) {
          p.Queue.BeginPeek();
          _isPeeking = true;
        }

        Thread.Sleep(1000);
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
      if( !queueName.StartsWith("private$\\") )
        queueName = "private$\\" + queueName;

      var queue = MessageQueue.GetPrivateQueuesByMachine(server).Where(q => q.QueueName == queueName).FirstOrDefault();

      return queue != null ? queue.CanRead : false;
    }



    protected override void LoadQueues() {
      try {
        _eventQueues.Clear();
        _cmdQueues.Clear();
        _msgQueues.Clear();
        _errorQueues.Clear();

        var queues = MessageQueue.GetPrivateQueuesByMachine(_serverName);

        foreach( var q in queues ) {
          string qName = q.QueueName.Replace("private$\\", "");

          if( _watchEventQueues.Any(w1 => qName.StartsWith(w1)) )
            _eventQueues.Add(q);

          else if( _watchCommandQueues.Any(w2 => qName.StartsWith(w2)) )
            _cmdQueues.Add(q);

          else if( _watchMessageQueues.Any(w3 => qName.StartsWith(w3)) )
            _msgQueues.Add(q);

          else if( _watchErrorQueues.Any(w4 => qName.StartsWith(w4)) )
            _errorQueues.Add(q);


        }

      } catch( Exception e ) {
        OnError("Error occured when loading queues", e, true);
      }

    }

    private List<QueueItem> LoadSubscriptionQueues(IList<MessageQueue> queues, QueueType type, IList<QueueItem> currentItems) {
      if( queues.Count == 0 )
        return EMPTY_LIST;

      List<QueueItem> r = new List<QueueItem>();

      foreach( var q in queues ) {
        string qName = q.QueueName.Replace("private$\\", "");

        if( !IsSubscriptionQueue(qName) )
          continue;

        q.MessageReadPropertyFilter.ArrivedTime = true;
        q.MessageReadPropertyFilter.Label = true;
        q.MessageReadPropertyFilter.Body = true;

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





    protected override IEnumerable<QueueItem> DoFetchQueueItems(IList<MessageQueue> queues, QueueType type, IList<QueueItem> currentItems) {
      if( queues.Count == 0 )
        return EMPTY_LIST;

      List<QueueItem> r = new List<QueueItem>();

      foreach( var q in queues ) {
        string qName = q.QueueName.Replace("private$\\", "");

        if( IsIgnoredQueue(qName) )
          continue;

        q.MessageReadPropertyFilter.ArrivedTime = true;
        q.MessageReadPropertyFilter.Label = true;
        q.MessageReadPropertyFilter.Body = true;

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

      return qs.Single(i => i.QueueName.EndsWith(itm.QueueName));
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

      foreach( var q in MessageQueue.GetPrivateQueuesByMachine(server).
                                            Where(q => q.QueueName.EndsWith(".subscriptions")) ) {

        q.MessageReadPropertyFilter.Label = true;
        q.MessageReadPropertyFilter.Body = true;

        try {
          var publisher = q.QueueName.Replace(".subscriptions", string.Empty).Replace("private$\\", string.Empty);

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

      _errorQueues.Where(q => q.QueueName == name).Single().Purge();

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
