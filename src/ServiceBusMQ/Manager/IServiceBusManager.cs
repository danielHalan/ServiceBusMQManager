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


    void Init(string serverName, Queue[] monitorQueues, CommandDefinition commandDef);

    MessageContentFormat MessageContentFormat { get; }

    string LoadMessageContent(QueueItem itm);

    IEnumerable<QueueItem> GetUnprocessedQueueItems(QueueType type, IEnumerable<QueueItem> currentItems);
    IEnumerable<QueueItem> GetProcessedQueueItems(QueueType type, DateTime since, IEnumerable<QueueItem> currentItems);
    

    void MoveErrorItemToOriginQueue(QueueItem itm);
    void MoveAllErrorItemsToOriginQueue(string errorQueue);

    void PurgeMessage(QueueItem itm);
    void PurgeAllMessages();

    void PurgeErrorMessages(string queueName);
    void PurgeErrorAllMessages();

    
    Queue[] MonitorQueues { get; }

    event EventHandler ItemsChanged;
    event EventHandler<ErrorArgs> ErrorOccured;

  }
}
