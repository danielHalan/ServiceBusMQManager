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

		public string[] AvailableMessageQueueTypes
		{
			get { return new string[] { "MSMQ" }; }
		}

		public string[] AvailableMessageContentTypes
		{
			get { return new string[] { "XML", "JSON" }; }
		}


		public bool CanAccessServer(string server)
		{
			return true;
		}

		public bool CanAccessQueue(string server, string queueName)
		{
			var queue = Msmq.Create(server, queueName, QueueAccessMode.ReceiveAndAdmin);

			return queue != null ? queue.CanRead : false;
		}

		public string[] GetAllAvailableQueueNames(string server)
		{
			return MessageQueue.GetPrivateQueuesByMachine(server).Where(q => !IsIgnoredQueue(q.QueueName)).
				Select(q => q.QueueName.Replace("private$\\", "")).ToArray();
		}

		private bool IsIgnoredQueue(string queueName)
		{
			return (queueName.EndsWith("_retries") || queueName.EndsWith("_timeouts") || queueName.EndsWith("_timeoutsdispatcher"));
		}
	}
}
