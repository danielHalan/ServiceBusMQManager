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

<<<<<<< HEAD
namespace ServiceBusMQ.Adapter.Azure.ServiceBus22 {
=======
namespace ServiceBusMQ.NServiceBus4.Azure {
>>>>>>> 3dd34e76b2bd5c60a3431e8f5fa66de0154cca6c
  public class Azure_ServiceBus_Discovery : IServiceBusDiscovery {

    public string ServiceBusName { get { return "Windows Azure"; } }
    public string ServiceBusVersion { get { return "2.2"; } }
    public string MessageQueueType { get { return "Service Bus"; } }

<<<<<<< HEAD
=======

>>>>>>> 3dd34e76b2bd5c60a3431e8f5fa66de0154cca6c
    public string[] AvailableMessageContentTypes {
      get { return new string[] { "XML", "JSON" }; }
    }

<<<<<<< HEAD
    static readonly ServiceBusFeature[] _features = new ServiceBusFeature[] {
      //ServiceBusFeature.PurgeMessage, 
      ServiceBusFeature.PurgeAllMessages, 
      //ServiceBusFeature.MoveErrorMessageToOriginQueue, 
      ServiceBusFeature.MoveAllErrorMessagesToOriginQueue 
    };
    public ServiceBusFeature[] Features {
      get { return _features; }
    }

    public ServerConnectionParameter[] ServerConnectionParameters { 
      get { 
        return new ServerConnectionParameter[] { 
          ServerConnectionParameter.Create("connectionStr", "Connection String"),
          ServerConnectionParameter.Create("msgLimit", "Fetch Message Count Limit", ParamType.String, "100")
=======
    public ServerConnectionParameter[] ServerConnectionParameters { 
      get { 
        return new ServerConnectionParameter[] { 
          ServerConnectionParameter.Create("connectionStr", "Connection String")
>>>>>>> 3dd34e76b2bd5c60a3431e8f5fa66de0154cca6c
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
