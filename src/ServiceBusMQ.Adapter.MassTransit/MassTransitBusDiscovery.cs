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
    public string ServiceBusVersion {
      get { return string.Empty; }
    }


		public string MessageQueueType
		{
			get { return  "MSMQ"; }
		}

		public string[] AvailableMessageContentTypes
		{
			get { return new string[] { "XML", "JSON" }; }
		}

    public ServerConnectionParameter[] ServerConnectionParameters { 
      get { 
        return new ServerConnectionParameter[] { 
          ServerConnectionParameter.Create("server", "Server Name"),
          ServerConnectionParameter.Create("subscriptionQueueService", "Subscription Queue Service", null, true)
        };
      }
    }


    public bool CanAccessServer(Dictionary<string, string> connectionSettings)
		{
			return true;
		}

    public bool CanAccessQueue(Dictionary<string, string> connectionSettings, string queueName)
		{
			var queue = Msmq.Create(connectionSettings["server"], queueName, QueueAccessMode.ReceiveAndAdmin);

			return queue != null ? queue.CanRead : false;
		}

		public string[] GetAllAvailableQueueNames(Dictionary<string,string> connectionSettings)
		{
			return MessageQueue.GetPrivateQueuesByMachine(connectionSettings["server"]).Where(q => !IsIgnoredQueue(q.QueueName)).
				Select(q => q.QueueName.Replace("private$\\", "")).ToArray();
		}

		private bool IsIgnoredQueue(string queueName)
		{
			return (queueName.EndsWith("_retries") || queueName.EndsWith("_timeouts") || queueName.EndsWith("_timeoutsdispatcher"));
		}
	}
}
