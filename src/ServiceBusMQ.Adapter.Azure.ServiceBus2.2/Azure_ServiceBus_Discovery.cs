#region File Information
/********************************************************************
  Project: ServiceBusMQ.NServiceBus4.Azure
  File:    NServiceBus_AzureMQ_Discovery.cs
  Created: 2013-10-11

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
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using ServiceBusMQ.Manager;

namespace ServiceBusMQ.NServiceBus4.Azure {
  public class Azure_ServiceBus_Discovery : IServiceBusDiscovery {

    public string ServiceBusName { get { return "Windows Azure"; } }
    public string ServiceBusVersion { get { return "2.2"; } }
    public string MessageQueueType { get { return "Service Bus"; } }


    public string[] AvailableMessageContentTypes {
      get { return new string[] { "XML", "JSON" }; }
    }

    public ServerConnectionParameter[] ServerConnectionParameters { 
      get { 
        return new ServerConnectionParameter[] { 
          ServerConnectionParameter.Create("connectionStr", "Connection String")
        };
      }
    }


    public bool CanAccessServer(Dictionary<string, object> connectionSettings) {
      return true;
    }

    public bool CanAccessQueue(Dictionary<string, object> connectionSettings, string queueName) {
      return true;
      //var queue = Msmq.Create(server, queueName, QueueAccessMode.ReceiveAndAdmin);

      //return queue != null ? queue.CanRead : false;
    }

    public string[] GetAllAvailableQueueNames(Dictionary<string, object> connectionSettings) {
      var mgr = NamespaceManager.CreateFromConnectionString(connectionSettings["connectionStr"] as string);
      return mgr.GetQueues().Select(q => q.Path).ToArray();
    }

    //private bool IsIgnoredQueue(string queueName) {
    //  return ( queueName.EndsWith(".subscriptions") || queueName.EndsWith(".retries") || queueName.EndsWith(".timeouts") || queueName.EndsWith(".timeoutsdispatcher") );
    //}



  }
}
