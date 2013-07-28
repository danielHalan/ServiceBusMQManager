#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    IServiceBusManager.cs
  Created: 2013-02-14

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using ServiceBusMQ.Model;

namespace ServiceBusMQ.Manager {
  public interface IServiceBusManager : IServiceBus {


    /// <summary>
    /// Called once to initialize the manager
    /// </summary>
    /// <param name="serverName">The Host name of the server</param>
    /// <param name="monitorQueues">Queues to monitor</param>
    /// <param name="commandDef">Command definition</param>
    void Initialize(string serverName, Queue[] monitorQueues, SbmqmMonitorState monitorState);

    /// <summary>
    /// Called when manager should shut-down and free all resources
    /// </summary>
    void Terminate();


    /// <summary>
    /// What type of message content to expect in queues
    /// </summary>
    MessageContentFormat MessageContentFormat { get; }

    /// <summary>
    /// Load message content if not already loaded
    /// </summary>
    /// <param name="itm"></param>
    /// <returns></returns>
    string LoadMessageContent(QueueItem itm);

    /// <summary>
    /// Return new unprocessed messages that are in queue(s) of specified type
    /// </summary>
    /// <param name="type">Type of queues to scan</param>
    /// <param name="currentItems">Already fetched items, that should be reused in result if still not processed.</param>
    /// <returns></returns>
    QueueFetchResult GetUnprocessedMessages(QueueType type, IEnumerable<QueueItem> currentItems);

    /// <summary>
    /// Return new processed messages that are in queue(s) of specified type
    /// </summary>
    /// <param name="type">Type of queues to scan</param>
    /// <param name="since">Items to return since specifed DateTime</param>
    /// <param name="currentItems">Already fetched items, that should be reused in result if exist in specified selection</param>
    /// <returns></returns>
    QueueFetchResult GetProcessedMessages(QueueType type, DateTime since, IEnumerable<QueueItem> currentItems);
    

    /// <summary>
    /// Move Error message to Origin Queue.
    /// </summary>
    /// <param name="itm"></param>
    void MoveErrorMessageToOriginQueue(QueueItem itm);
    
    /// <summary>
    /// Move all Error messages to their Origin Queues
    /// </summary>
    /// <param name="errorQueue">What error queue to process</param>
    void MoveAllErrorMessagesToOriginQueue(string errorQueue);

    /// <summary>
    /// Purge message from Queue, removing the specified message.
    /// </summary>
    /// <param name="itm"></param>
    void PurgeMessage(QueueItem itm);

    /// <summary>
    /// Purge all messages from Queues, clearing the queues.
    /// </summary>
    void PurgeAllMessages();

    /// <summary>
    /// Purge all Error messages from Queue with provided name, clearing the queue.
    /// </summary>
    /// <param name="queueName"></param>
    void PurgeErrorMessages(string queueName);
    
    /// <summary>
    /// Purge all Error messages from all Queues.
    /// </summary>
    void PurgeErrorAllMessages();


    /// <summary>
    /// Queues that are being monitored
    /// </summary>
    Queue[] MonitorQueues { get; }

    /// <summary>
    /// Event triggered when the messages collection has changed.
    /// </summary>
    event EventHandler ItemsChanged;
    
    /// <summary>
    /// Called when any error inside the manager has occured, that should be presented to the user.
    /// </summary>
    event EventHandler<ErrorArgs> ErrorOccured;

  }
}
