#region File Information
/********************************************************************
  Project: ServiceBusMQ.NServiceBus
  File:    NServiceBusDiscovery.cs
  Created: 2013-02-14

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System.Linq;
using System.Messaging;
using ServiceBusMQ.Manager;

namespace ServiceBusMQ.NServiceBus {

  public class NServiceBusDiscovery : IServiceBusDiscovery {

    public string ServiceBusName {
      get { return "NServiceBus"; }
    }

    public string[] AvailableMessageQueueTypes { 
      get { return new string[] { "MSMQ" }; } 
    }

    public string[] AvailableMessageContentTypes {
      get { return new string[] { "XML", "JSON" }; } 
    }


    public bool CanAccessServer(string server) {
      return true;
    }

    public bool CanAccessQueue(string server, string queueName) {
      var queue = Msmq.Create(server, queueName, QueueAccessMode.ReceiveAndAdmin);

      return queue != null ? queue.CanRead : false;
    }

    public string[] GetAllAvailableQueueNames(string server) {
      return MessageQueue.GetPrivateQueuesByMachine(server).Where(q => !IsIgnoredQueue(q.QueueName)).
          Select(q => q.QueueName.Replace("private$\\", "")).ToArray();
    }

    private bool IsIgnoredQueue(string queueName) {
      return ( queueName.EndsWith(".subscriptions") || queueName.EndsWith(".retries") || queueName.EndsWith(".timeouts") || queueName.EndsWith(".timeoutsdispatcher") );
    }

  }
}
