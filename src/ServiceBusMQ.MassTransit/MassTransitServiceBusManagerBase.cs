#region File Information
/********************************************************************
  Project: ServiceBusMQ.MassTransit
  File:    MassTransitServiceBusManagerBase.cs
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
using ServiceBusMQ.Manager;
using ServiceBusMQ.Model;
using System.IO;
using System.Xml.Linq;
using System.Runtime.CompilerServices;

namespace ServiceBusMQ.MassTransit
{
	public abstract class MassTransitServiceBusManagerBase : IServiceBusManager
	{

		static readonly string JSON_START = "\"$type\":\"";
		static readonly string JSON_END = ",";


		protected string _serverName;
		protected SbmqmMonitorState _monitorState;
		protected CommandDefinition _commandDef;

		protected List<MsmqMessageQueue> _monitorMsmqQueues = new List<MsmqMessageQueue>();

		public string[] AvailableMessageQueueTypes
		{
			get { return new string[] { "MSMQ" }; }
		}

		public string[] AvailableMessageContentTypes
		{
			get { return new string[] { "XML", "JSON" }; }
		}


		public MassTransitServiceBusManagerBase()
		{
		}
		public virtual void Initialize(string serverName, Queue[] monitorQueues, SbmqmMonitorState monitorState)
		{
			_serverName = serverName;

			MonitorQueues = monitorQueues;
			_monitorState = monitorState;
		}


		public bool IsIgnoredQueue(string queueName)
		{
			return (queueName.EndsWith(".subscriptions") || queueName.EndsWith(".retries") || queueName.EndsWith(".timeouts"));
		}

		public void MoveErrorMessageToOriginQueue(QueueItem itm)
		{
			if (string.IsNullOrEmpty(itm.Id))
				throw new ArgumentException("MessageId can not be null or empty");

			if (itm.Queue.Type != QueueType.Error)
				throw new ArgumentException("Queue is not of type Error, " + itm.Queue.Type);

			var mgr = new ErrorManager();

			// TODO:
			// Check if Clustered Queue, due if Clustered && NonTransactional, then Error

			mgr.InputQueue = Address.Parse(itm.Queue.Name);

			mgr.ReturnMessageToSourceQueue(itm.Id);
		}
		public void MoveAllErrorMessagesToOriginQueue(string errorQueue)
		{
			var mgr = new ErrorManager();

			// TODO:
			// Check if Clustered Queue, due if Clustered && NonTransactional, then Error

			mgr.InputQueue = Address.Parse(errorQueue);

			mgr.ReturnAll();
		}

		protected string ReadMessageStream(Stream s)
		{
			using (StreamReader r = new StreamReader(s, Encoding.Default))
				return r.ReadToEnd().Replace("\0", "");
		}
		protected string GetSubscriptionType(string xml)
		{
			List<string> r = new List<string>();
			try
			{
				XDocument doc = XDocument.Parse(xml);

				var e = doc.Root as XElement;
				return e.Value;


			}
			catch { }

			return string.Empty;
		}
		public abstract MessageSubscription[] GetMessageSubscriptions(string server);


		public abstract string LoadMessageContent(QueueItem itm);

		protected IEnumerable<MsmqMessageQueue> GetQueueListByType(QueueType type)
		{
			return _monitorMsmqQueues.Where(q => q.Queue.Type == type);
		}

		protected MessageInfo[] GetMessageNames(string content, bool includeNamespace)
		{

			if (content.StartsWith("<?xml version=\"1.0\""))
				return GetXmlMessageNames(content, includeNamespace);
			else return GetJsonMessageNames(content, includeNamespace);

		}
		private MessageInfo[] GetJsonMessageNames(string content, bool includeNamespace)
		{
			List<MessageInfo> r = new List<MessageInfo>();
			try
			{
				foreach (var msg in GetAllRootCurlyBrackers(content))
				{

					int iStart = msg.IndexOf(JSON_START) + JSON_START.Length;
					int iEnd = msg.IndexOf(JSON_END, iStart);

					if (!includeNamespace)
					{
						iStart = msg.LastIndexOf(".", iEnd) + 1;
					}

					r.Add(new MessageInfo(msg.Substring(iStart, iEnd - iStart)));
				}
			}
			catch { }

			return r.ToArray();
		}
		private MessageInfo[] GetXmlMessageNames(string content, bool includeNamespace)
		{
			List<MessageInfo> r = new List<MessageInfo>();
			try
			{
				XDocument doc = XDocument.Parse(content);
				string ns = string.Empty;

				if (includeNamespace)
				{
					ns = doc.Root.Attribute("xmlns").Value.Remove(0, 19) + ".";
				}

				foreach (XElement e in doc.Root.Elements())
				{
					r.Add(new MessageInfo(ns + e.Name.LocalName));
				}

			}
			catch { }

			return r.ToArray();
		}

		private IEnumerable<string> GetAllRootCurlyBrackers(string content)
		{
			int start = -1;
			int stack = 0;
			List<string> r = new List<string>();

			int i = 0;
			do
			{
				if (content[i] == '{')
				{
					if (stack == 0)
						start = i;

					stack++;
				}

				if (content[i] == '}')
				{
					stack--;

					if (stack == 0)
					{
						r.Add(content.Substring(start, i - start));
					}
				}

			} while (++i < content.Length);

			return r;
		}
		protected string MergeStringArray(MessageInfo[] arr)
		{
			StringBuilder sb = new StringBuilder();
			foreach (var msg in arr)
			{
				if (sb.Length > 0) sb.Append(", ");

				sb.Append(msg.Name);
			}

			return sb.ToString();
		}


		public abstract object DeserializeCommand(string cmd, Type cmdType);
		public abstract string SerializeCommand(object cmd);


		public bool MessagesHasMilliSecondPrecision { get { return false; } }


		public abstract QueueFetchResult GetUnprocessedMessages(QueueType type, IEnumerable<QueueItem> currentItems);
		public abstract QueueFetchResult GetProcessedMessages(QueueType type, DateTime since, IEnumerable<QueueItem> currentItems);

		public abstract void PurgeMessage(QueueItem itm);
		public abstract void PurgeAllMessages();

		public abstract void PurgeErrorMessages(string queueName);
		public abstract void PurgeErrorAllMessages();


		public Queue[] MonitorQueues { get; private set; }


		public event EventHandler<ErrorArgs> ErrorOccured;
		public event EventHandler<WarningArgs> WarningOccured;

		protected void OnError(string message, Exception exception = null, bool fatal = false)
		{
			if (ErrorOccured != null)
				ErrorOccured(this, new ErrorArgs(message, exception, fatal));
		}
		protected void OnWarning(string message, string content)
		{
			if (WarningOccured != null)
				WarningOccured(this, new WarningArgs(message, content));
		}

		public string ServiceBusName
		{
			get { return "MassTransit"; }
		}


		protected EventHandler _itemsChanged;

		public event EventHandler ItemsChanged
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				_itemsChanged = (EventHandler)Delegate.Combine(_itemsChanged, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				_itemsChanged = (EventHandler)Delegate.Remove(_itemsChanged, value);
			}
		}

		protected void OnItemsChanged()
		{
			if (_itemsChanged != null)
				_itemsChanged(this, EventArgs.Empty);
		}


		public abstract void Terminate();
	}

}
