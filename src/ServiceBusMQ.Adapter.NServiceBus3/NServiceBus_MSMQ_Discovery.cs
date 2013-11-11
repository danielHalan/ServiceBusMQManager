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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using ServiceBusMQ.Manager;

namespace ServiceBusMQ.NServiceBus {

  public class NServiceBus_MSMQ_Discovery : IServiceBusDiscovery {

    public string ServiceBusName {
      get { return "NServiceBus"; }
    }
    public string ServiceBusVersion { 
      get { return "3"; } 
    }

    public string MessageQueueType { 
      get { return "MSMQ"; } 
    }

    public string[] AvailableMessageContentTypes {
      get { return new string[] { "XML", "JSON" }; } 
    }
    
    public ServerConnectionParameter[] ServerConnectionParameters { 
      get { 
        return new ServerConnectionParameter[] { 
          ServerConnectionParameter.Create(NServiceBus_MSMQ_Manager.CS_SERVER, "Server Name"),
          ServerConnectionParameter.Create(NServiceBus_MSMQ_Manager.CS_PEEK_THREADS, "Use Eager Peek Threads", ParamType.Bool, false, true)
        };
      }
    }

    public bool CanAccessServer(Dictionary<string, object> connectionSettings) {
      return true;
    }

    public bool CanAccessQueue(Dictionary<string, object> connectionSettings, string queueName) {
      var queue = Msmq.Create(connectionSettings["server"] as string, queueName, QueueAccessMode.ReceiveAndAdmin);

      return queue != null ? queue.CanRead : false;
    }

    public string[] GetAllAvailableQueueNames(Dictionary<string, object> connectionSettings) {
      return MessageQueue.GetPrivateQueuesByMachine(connectionSettings["server"] as string).Where(q => !IsIgnoredQueue(q.QueueName)).
          Select(q => q.QueueName.Replace("private$\\", "")).ToArray();
    }

    private bool IsIgnoredQueue(string queueName) {
      return ( queueName.EndsWith(".subscriptions") || queueName.EndsWith(".retries") || queueName.EndsWith(".timeouts") || queueName.EndsWith(".timeoutsdispatcher") );
    }

  }
}
