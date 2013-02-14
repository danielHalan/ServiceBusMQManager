#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    IMessageManager.cs
  Created: 2012-08-30

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
using ServiceBusMQ.ViewModel;

namespace ServiceBusMQ.Manager {
  public interface IMessageManager : IDisposable {

    void Init(string serverName, Queue[] monitorQueues, CommandDefinition commandDef);

    MessageContentFormat MessageContentFormat { get; }

    void MoveErrorItemToOriginQueue(QueueItem itm);
    void MoveAllErrorItemsToOriginQueue(string errorQueue);


    string GetMessageContent(QueueItem itm);

    string[] GetAllAvailableQueueNames(string server);
    bool CanAccessQueue(string server, string queueName);

    void RefreshQueueItems();


    void PurgeMessage(QueueItem itm);
    void PurgeAllMessages();
    
    void PurgeErrorMessages(string queueName);
    void PurgeErrorAllMessages();


    string BusName { get; }
    string BusQueueType { get; }


    Queue[] MonitorQueues { get; }

    bool MonitorCommands { get; set; }
    bool MonitorEvents { get; set; }
    bool MonitorMessages { get; set; }
    bool MonitorErrors { get; set; }


    List<QueueItemViewModel> Items { get; }


    event EventHandler ItemsChanged;
    event EventHandler<ErrorArgs> ErrorOccured;

    void ClearProcessedItems();

    void LoadProcessedQueueItems(TimeSpan timeSpan);
  }
}
