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
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

using ServiceBusMQ;
using ServiceBusMQ.Model;

namespace ServiceBusMQManager.MessageBus.NServiceBus {

  public class NServiceBus_MSMQ_Manager : NServiceBusManagerBase {
    

    public NServiceBus_MSMQ_Manager() {
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

      } catch(Exception e) {
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

            if( itm == null ) {
              itm = new QueueItem();
              itm.DisplayName = msg.Label;
              itm.QueueDisplayName = qName.CutBeginning(44);
              itm.QueueName = qName;
              itm.QueueType = type;
              itm.Label = msg.Label;
              itm.Id = msg.Id;
              itm.ArrivedTime = msg.ArrivedTime;
              itm.Content = ReadMessageStream(msg.BodyStream);
            }

            if( !IsIgnoredQueueItem(itm) ) {

              itm.MessageNames = GetMessageNames(itm.Content, true);
              itm.DisplayName = MergeStringArray(GetMessageNames(itm.Content, false)).Default(itm.Label);

              r.Add(itm);
            }
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

            if( itm == null ) {
              itm = new QueueItem();
              itm.DisplayName = msg.Label;
              itm.QueueDisplayName = qName.CutBeginning(44);
              itm.QueueName = qName;
              itm.QueueType = type;
              itm.Label = msg.Label;
              itm.Id = msg.Id;
              itm.ArrivedTime = msg.ArrivedTime;
              itm.Content = ReadMessageStream(msg.BodyStream);
            }

            if( !IsIgnoredQueueItem(itm) ) {

              itm.MessageNames = GetMessageNames(itm.Content, true);
              itm.DisplayName = MergeStringArray(GetMessageNames(itm.Content, false)).Default(itm.Label);

              r.Add(itm);
            }
          }
        } catch( Exception e ) {
          OnError("Error occured when processing queue item", e, true);
        } 

      }

      return r;
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
