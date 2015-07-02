#region File Information
/********************************************************************
  Project: ServiceBusMQ.NServiceBus4
  File:    ErrorManager.cs
  Created: 2013-10-11

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion



namespace ServiceBusMQ.Adapter.NServiceBus4.Azure.SB22 {

  using System;
  using System.Linq;
  using System.Collections.Generic;
  using Microsoft.ServiceBus.Messaging;
  using ServiceBusMQ.Model;
  using NLog;

  public class ErrorManager {

    protected Logger _log = LogManager.GetCurrentClassLogger();
    public const string KEY_FailedQueue = "NServiceBus.FailedQ";

    public bool ClusteredQueue { get; set; }

    public string ConnectionString { get; set; }

    public ErrorManager(string connectionString) {
      ConnectionString = connectionString;
    }

    public virtual QueueClient GetInputQueue(string queueName, ReceiveMode mode) {
      return QueueClient.CreateFromConnectionString(ConnectionString, queueName, mode);
    }
    public virtual QueueClient GetInputQueue(string queueName) {
      return QueueClient.CreateFromConnectionString(ConnectionString, queueName);
    }

    public void ReturnAll(string fromQueueName, uint queueCount) {
      var queue = GetInputQueue(fromQueueName, ReceiveMode.ReceiveAndDelete);


      int i = 0;
      IEnumerable<BrokeredMessage> msgs;
      while( ( msgs = queue.ReceiveBatch(0xFFFF) ).Any() ) {
        _log.Trace("About to move {0} messages to origin queue", msgs.Count());
        
        foreach( var msg in msgs ) {

          try {
            string originQueueName = GetOriginQueue(msg);

            if( originQueueName.IsValid() ) {
              var originQueue = GetInputQueue(originQueueName);
              originQueue.Send(msg);
            }

          } catch( Exception ex ) {
            _log.Trace(ex.Message);
          }

          i++;
        }

        if( i >= queueCount )
          break;
      };

    }


    public void ReturnMessageToSourceQueue(string fromQueueName, ServiceBusMQ.Model.QueueItem itm) {
      var queue = GetInputQueue(fromQueueName);

      ReturnMessageToSourceQueue(queue, itm);
    }

    /// <summary>
    ///     May throw a timeout exception if a message with the given id cannot be found.
    /// </summary>
    /// <param name="seqNumber"></param>
    public void ReturnMessageToSourceQueue(QueueClient queue, ServiceBusMQ.Model.QueueItem itm) {
      try {

        var msg = FindMessage(queue, itm); //queue.Receive((long)itm.MessageQueueItemId);

        string originQueueName = GetOriginQueue(msg);
        if( originQueueName.IsValid() ) {
          var q = GetInputQueue(originQueueName);
          q.Send(msg.Clone());

          msg.Complete();
        } else _log.Trace("No valid origin Queue for Message, " + itm.Id);

      } catch( Exception ex ) {
        _log.Trace(ex.ToString());
      }
    }

    internal void ReturnMessageToSourceQueue(QueueItem itm) {
    }

    private BrokeredMessage FindMessage(QueueClient client, QueueItem msg) {

      var items = client.PeekBatch(50);
      while( items != null && items.Any() ) {
        foreach( var itm in items ) {
          if( itm.MessageId == msg.Id )
            return itm;
        }

        items = client.PeekBatch(50);
      }

      return null;
    }

    private void TryFindMessage(ServiceBusMQ.Model.QueueItem itm) {

      //if( ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout ) {

      //  foreach( var m in queue.GetAllMessages() ) {
      //    var tm = MsmqUtilities.Convert(m);

      //    string originalId = null;

      //    if( tm.Headers.ContainsKey(Headers.MessageId) ) {
      //      originalId = tm.Headers[Headers.MessageId];
      //    }

      //    if( string.IsNullOrEmpty(originalId) && tm.Headers.ContainsKey(Headers.CorrelationId) ) {
      //      originalId = tm.Headers[Headers.CorrelationId];
      //    }

      //    if( string.IsNullOrEmpty(originalId) || seqNumber != originalId ) {
      //      continue;
      //    }

      //    Console.WriteLine("Found message - going to return to queue.");

      //    using( var q = GetInputQueue(itm.Headers[FaultsHeaderKeys.FailedQ]) ) {
      //      q.Send(m);
      //    }

      //    queue.ReceiveByLookupId(MessageLookupAction.Current, m.LookupId,
      //        MessageQueueTransactionType.Automatic);

      //  }

      //  Console.WriteLine("Success.");

      //  return;
      //}
    }


    private string GetOriginQueue(BrokeredMessage msg) {
      var name = string.Empty;

      if( msg.Properties.ContainsKey(KEY_FailedQueue) )
        name = msg.Properties[KEY_FailedQueue] as string;

      if( !string.IsNullOrEmpty(name) ) {
        var i = name.IndexOf('@');

        if( i > 0 )
          name = name.Substring(0, i);
      }

      return name;
    }
    private string GetOriginQueue(ServiceBusMQ.Model.QueueItem msg) {
      var name = string.Empty;

      if( msg.Headers.ContainsKey(KEY_FailedQueue) )
        name = msg.Headers[KEY_FailedQueue] as string;

      if( !string.IsNullOrEmpty(name) ) {
        var i = name.IndexOf('@');

        if( i > 0 )
          name = name.Substring(0, i);
      }

      return name;
    }



    //const string NonTransactionalQueueErrorMessageFormat = "Queue '{0}' must be transactional.";

    //readonly string NoMessageFoundErrorFormat =
    //    string.Format("INFO: No message found with ID '{0}'. Going to check headers of all messages for one with '{0}' or '{1}'.", Headers.MessageId, Headers.CorrelationId);

    static readonly TimeSpan TimeoutDuration = TimeSpan.FromSeconds(5);


  }
}