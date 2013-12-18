#region File Information
/********************************************************************
  Project: ServiceBusMQ.MassTransit
  File:    MassTransitBusDiscovery.cs
  Created: 2013-10-11

  Author(s):
    Juan J. Chiw
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceBusMQ.Manager;
using System.Messaging;

namespace ServiceBusMQ.MassTransit
{
    public class MassTransitBusDiscovery : IServiceBusDiscovery
    {
        public string ServiceBusName
        {
            get { return "MassTransit"; }
        }
        public string ServiceBusVersion
        {
            get { return "3"; }
        }

        public string MessageQueueType
        {
            get { return "MSMQ"; }
        }

        public string[] AvailableMessageContentTypes
        {
            get { return new string[] { "XML", "JSON" }; }
        }

        public ServiceBusFeature[] Features
        {
            get { return ServiceBusFeatures.All; }
        }

        public ServerConnectionParameter[] ServerConnectionParameters
        {
            get
            {
                return new ServerConnectionParameter[] { 
          ServerConnectionParameter.Create("server", "Server Name"),
          ServerConnectionParameter.Create("subscriptionQueueService", "Subscription Queue Service", ParamType.String, null, true)
        };
            }
        }


        public bool CanAccessServer(Dictionary<string, object> connectionSettings)
        {
            return true;
        }

        public bool CanAccessQueue(Dictionary<string, object> connectionSettings, string queueName)
        {
            var queue = Msmq.Create(connectionSettings["server"] as string, queueName, QueueAccessMode.ReceiveAndAdmin);

            return queue != null ? queue.CanRead : false;
        }

        public string[] GetAllAvailableQueueNames(Dictionary<string, object> connectionSettings)
        {
            return MessageQueue.GetPrivateQueuesByMachine(connectionSettings["server"] as string).Where(q => !IsIgnoredQueue(q.QueueName)).
                Select(q => q.QueueName.Replace("private$\\", "")).ToArray();
        }

        private bool IsIgnoredQueue(string queueName)
        {
            return (queueName.EndsWith("_retries") || queueName.EndsWith("_timeouts") || queueName.EndsWith("_timeoutsdispatcher"));
        }
    }
}
