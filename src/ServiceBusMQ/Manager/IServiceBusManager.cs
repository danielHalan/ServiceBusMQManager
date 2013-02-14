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
using System.Linq;
using System.Text;
using ServiceBusMQ.Model;

namespace ServiceBusMQ.Manager {
  public interface IServiceBusManager : IServiceBus {


    /// <summary>
    /// Called once to initialize the manager
    /// </summary>
    /// <param name="serverName">The Host name of the server</param>
    /// <param name="monitorQueues">Queues to monitor</param>
    /// <param name="commandDef">Command definition</param>
    void Init(string serverName, Queue[] monitorQueues, CommandDefinition commandDef);

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
    /// <param name="currentItems">Already fetched items, that doesn't need to be return</param>
    /// <returns></returns>
    IEnumerable<QueueItem> GetUnprocessedMessages(QueueType type, IEnumerable<QueueItem> currentItems);

    /// <summary>
    /// Return new processed messages that are in queue(s) of specified type
    /// </summary>
    /// <param name="type">Type of queues to scan</param>
    /// <param name="since">Items to return since specifed DateTime</param>
    /// <param name="currentItems">Already fetched items, that doesn't need to be return</param>
    /// <returns></returns>
    IEnumerable<QueueItem> GetProcessedMessages(QueueType type, DateTime since, IEnumerable<QueueItem> currentItems);
    

    void MoveErrorMessageToOriginQueue(QueueItem itm);
    void MoveAllErrorMessagesToOriginQueue(string errorQueue);

    void PurgeMessage(QueueItem itm);
    void PurgeAllMessages();

    void PurgeErrorMessages(string queueName);
    void PurgeErrorAllMessages();


    /// <summary>
    /// Queues that are being monitored
    /// </summary>
    Queue[] MonitorQueues { get; }

    event EventHandler ItemsChanged;
    event EventHandler<ErrorArgs> ErrorOccured;

  }
}
