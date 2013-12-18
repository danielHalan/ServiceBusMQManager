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


namespace ServiceBusMQ.Adapter.Azure.ServiceBus22 {

  using System;
  using System.Collections.Generic;
  using Microsoft.ServiceBus.Messaging;
  using ServiceBusMQ.Model;

  public class ErrorManager {

    public const string KEY_FailedQueue = "NServiceBus.FailedQ";

    public bool ClusteredQueue { get; set; }

    public string ConnectionString { get; set; }

    public ErrorManager(string connectionString) {
      ConnectionString = connectionString;
    }

    public virtual QueueClient GetInputQueue(string queueName) {
      return QueueClient.CreateFromConnectionString(ConnectionString, queueName);
    }

    public void ReturnAll(string fromQueueName) {
      var queue = QueueClient.CreateFromConnectionString(ConnectionString, fromQueueName);
      var deadLetterQueue = QueueClient.CreateFromConnectionString(ConnectionString, QueueClient.FormatDeadLetterPath(fromQueueName), ReceiveMode.PeekLock);

      foreach( var msg in deadLetterQueue.ReceiveBatch(0xFFFF) ) {

        try {
          queue.Send(msg);
          msg.Abandon();
          //queue.Send(message.Clone());

        } catch( Exception ex ) {
          TryFindMessage(null);
        }

      }
    }


    public void ReturnMessageToSourceQueue(string fromQueueName, ServiceBusMQ.Model.QueueItem itm) {
      var queue = QueueClient.CreateFromConnectionString(ConnectionString, fromQueueName);
      var deadLetterQueue = QueueClient.CreateFromConnectionString(ConnectionString, QueueClient.FormatDeadLetterPath(fromQueueName), ReceiveMode.ReceiveAndDelete);

      ReturnMessageToSourceQueue(queue, deadLetterQueue, itm);
    }

    /// <summary>
    ///     May throw a timeout exception if a message with the given id cannot be found.
    /// </summary>
    /// <param name="seqNumber"></param>
    public void ReturnMessageToSourceQueue(QueueClient queue, QueueClient deadLetterQueue, ServiceBusMQ.Model.QueueItem itm) {
      try {
        var message = deadLetterQueue.Receive((long)itm.MessageQueueItemId);

        message.Abandon();
        //queue.Send(message.Clone());


      } catch( Exception ex ) {
        TryFindMessage(itm);
      }
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

    //const string NonTransactionalQueueErrorMessageFormat = "Queue '{0}' must be transactional.";

    //readonly string NoMessageFoundErrorFormat =
    //    string.Format("INFO: No message found with ID '{0}'. Going to check headers of all messages for one with '{0}' or '{1}'.", Headers.MessageId, Headers.CorrelationId);

    static readonly TimeSpan TimeoutDuration = TimeSpan.FromSeconds(5);

  }
}