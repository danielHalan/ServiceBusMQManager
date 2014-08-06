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
      var queue = GetInputQueue(fromQueueName);

      foreach( var msg in queue.ReceiveBatch(0xFFFF) ) {

        var itm = new QueueItem(null);
        itm.MessageQueueItemId = msg.SequenceNumber;

        itm.Headers = new Dictionary<string, string>();
        if( msg.Properties.Count > 0 )
          msg.Properties.ForEach(p => itm.Headers.Add(p.Key, p.Value.ToString()));

        ReturnMessageToSourceQueue(queue, itm);
      }
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
      //try {



      var message = FindMessage(queue, itm); //queue.Receive((long)itm.MessageQueueItemId);

      string failedQ = null;
      if( itm.Headers.ContainsKey(KEY_FailedQueue) ) {
        failedQ = itm.Headers[KEY_FailedQueue];
      }

      if( string.IsNullOrEmpty(failedQ) )
        throw new Exception("Message does not have a header indicating from which queue it came. Cannot be automatically returned to queue. " + itm.DisplayName);

      var i = failedQ.IndexOf('@');

      if( i > 0 )
        failedQ = failedQ.Substring(0, i);


      var q = GetInputQueue(failedQ);
      q.Send(message);

      message.Complete();

      //} catch( Exception ex ) {
      ////} catch( MessageQueueException ex ) {
      //  TryFindMessage(itm);
      //}
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

    //const string NonTransactionalQueueErrorMessageFormat = "Queue '{0}' must be transactional.";

    //readonly string NoMessageFoundErrorFormat =
    //    string.Format("INFO: No message found with ID '{0}'. Going to check headers of all messages for one with '{0}' or '{1}'.", Headers.MessageId, Headers.CorrelationId);

    static readonly TimeSpan TimeoutDuration = TimeSpan.FromSeconds(5);

  }
}