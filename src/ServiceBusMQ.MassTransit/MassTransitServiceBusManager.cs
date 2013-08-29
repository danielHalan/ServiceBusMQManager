using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceBusMQ.Manager;
using ServiceBusMQ.Model;
using System.Xml.Linq;
using System.IO;
using System.Runtime.CompilerServices;
using ServiceBusMQ.Manager;
using System.Messaging;
using NLog;
using System.Threading;
using MassTransit;
using System.Xml.Serialization;

namespace ServiceBusMQ.MassTransit
{
	public class MassTransitServiceBusManager : IServiceBusManager
	{

		public QueueFetchResult GetProcessedMessages(QueueType type, DateTime since, IEnumerable<QueueItem> currentItems)
		{
			return new QueueFetchResult
			{
				Count = 1,
				Items = currentItems
			};
		}

		public QueueFetchResult GetUnprocessedMessages(QueueType type, IEnumerable<QueueItem> currentItems)
		{
			return new QueueFetchResult
			{
				Count = 1,
				Items = currentItems
			};
		}

		Queue[] _monitorQueues;
		string _serverName;
		SbmqmMonitorState _monitorState;

		public void Initialize(string serverName, Queue[] monitorQueues, SbmqmMonitorState monitorState)
		{
			_serverName = serverName;
			_monitorState = monitorState;
			_monitorQueues = monitorQueues;
		}

		public event EventHandler ItemsChanged;

		public string LoadMessageContent(QueueItem itm)
		{
			return itm.Content;
		}

		public bool MessagesHasMilliSecondPrecision
		{
			get { return false; }
		}

		public Queue[] MonitorQueues
		{
			get { return _monitorQueues; }
		}

		public void MoveAllErrorMessagesToOriginQueue(string errorQueue)
		{
			
		}

		public void MoveErrorMessageToOriginQueue(QueueItem itm)
		{
			
		}

		public void PurgeAllMessages()
		{
			
		}

		public void PurgeErrorAllMessages()
		{
			
		}

		public void PurgeErrorMessages(string queueName)
		{
			
		}

		public void PurgeMessage(QueueItem itm)
		{
			
		}

		public void Terminate()
		{
			
		}

		public event EventHandler<Manager.WarningArgs> WarningOccured;

		public string[] AvailableMessageContentTypes
		{
			get { return new string[] { "XML", "JSON" }; }
		}

		public string[] AvailableMessageQueueTypes
		{
			get { return new string[] { "XML", "JSON" }; }
		}

		public string ServiceBusName
		{
			get { return "MassTransit"; }
		}

		public event EventHandler<Manager.ErrorArgs> ErrorOccured;
	}
}
