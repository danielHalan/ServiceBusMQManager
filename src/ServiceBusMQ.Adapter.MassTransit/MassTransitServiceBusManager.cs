#region File Information
/********************************************************************
  Project: ServiceBusMQ.MassTransit
  File:    MassTransitServiceBusManager.cs
  Created: 2013-10-11

  Author(s):
    Juan J. Chiw

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
using System.Xml.Linq;
using System.IO;
using System.Runtime.CompilerServices;
using System.Messaging;
using NLog;
using System.Threading;
using System.Xml.Serialization;
using MassTransit.Context;
using MassTransit.Serialization.Custom;
using System.Xml;
using MassTransit.Serialization;
using System.Text.RegularExpressions;
using System.Reflection;
using MassTransit;
using IServiceBus = MassTransit.IServiceBus;
using ServiceBusFactory = MassTransit.ServiceBusFactory;
using Newtonsoft.Json;

namespace ServiceBusMQ.MassTransit
{
	public class MassTransitServiceBusManager : IServiceBusManager, ISendCommand, IViewSubscriptions
	{

    public string ServiceBusName {
      get { return "MassTransit"; }
    }
    public string ServiceBusVersion {
      get { return string.Empty; }
    }
    public string MessageQueueType {
      get { return "MSMQ"; }
    }
    
		static readonly string JSON_START = "\"$type\":\"";
		static readonly string JSON_END = ",";

    static readonly string CS_SERVER = "server";
    static readonly string CS_SUBSCRIPTION_SERVICE = "subscriptionQueueService";

		protected List<MsmqMessageQueue> _monitorMsmqQueues = new List<MsmqMessageQueue>();

    #region GetProcessedMessages
    
    protected IEnumerable<MsmqMessageQueue> GetQueueListByType(QueueType type)
		{
			return _monitorMsmqQueues.Where(q => q.Queue.Type == type);
		}

		public bool IsIgnoredQueue(string queueName)
		{
			return (queueName.EndsWith("_subscriptions") || queueName.EndsWith("_retries") || queueName.EndsWith("_timeouts"));
		}


    private void SetupMessageReadPropertyFilters(MessageQueue q, QueueType type)
		{

			q.MessageReadPropertyFilter.Id = true;
			q.MessageReadPropertyFilter.ArrivedTime = true;
			q.MessageReadPropertyFilter.Label = true;
			q.MessageReadPropertyFilter.Body = false;

			//if( type == QueueType.Error )
			q.MessageReadPropertyFilter.Extension = true;
		}

		private QueueItem CreateQueueItem(Queue queue, Message msg)
		{
			var itm = new QueueItem(queue);
			itm.DisplayName = msg.Label;
			itm.Id = msg.Id;
			itm.ArrivedTime = msg.ArrivedTime;
			//itm.Content = ReadMessageStream(msg.BodyStream);

			itm.Headers = new Dictionary<string, string>();
			if (msg.Extension.Length > 0)
			{
				var stream = new MemoryStream(msg.Extension);
				var transportMessageHeaders = TransportMessageHeaders.Create(stream.ToArray());
				//var o = headerSerializer.Deserialize(stream);

				var contentType = transportMessageHeaders["Content-Type"];
				var originalMessageId = transportMessageHeaders["Original-Message-Id"];

				if (contentType != null)
					itm.Headers.Add("Content-Type", contentType);

				if (originalMessageId != null)
					itm.Headers.Add("Original-Message-Id", originalMessageId);
			}


			return itm;
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

		protected MessageInfo[] GetMessageNames(string content, bool includeNamespace)
		{

			if (content.StartsWith("<?xml version=\"1.0\""))
				return GetXmlMessageNames(content, includeNamespace);
			else return GetJsonMessageNames(content, includeNamespace);

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

		private bool PrepareQueueItemForAdd(QueueItem itm)
		{

			// Get from Message body
			if (itm.Content == null)
				LoadMessageContent(itm);

			itm.Messages = GetMessageNames(itm.Content, false);

			itm.DisplayName = MergeStringArray(itm.Messages).Default(itm.DisplayName).CutEnd(55);

			return true;
		}


		#endregion


		public QueueFetchResult GetProcessedMessages(QueueType type, DateTime since, IEnumerable<QueueItem> currentItems)
		{
			var result = new QueueFetchResult();

			var queues = GetQueueListByType(type);

			if (queues.Count() == 0)
			{
				result.Items = new List<QueueItem>();
				return result;
			}

			List<QueueItem> r = new List<QueueItem>();
			result.Items = r;

			foreach (var q in queues)
			{
				string qName = q.GetDisplayName();

				if (IsIgnoredQueue(qName) || !q.CanReadJournalQueue)
					continue;

				SetupMessageReadPropertyFilters(q.Journal, type);

				try
				{
					List<Message> messages = new List<Message>();

					// Enumete from the earliest item
					MessageEnumerator msgs = q.Journal.GetMessageEnumerator2();
					try
					{
						while (msgs.MoveNext())
						{
							Message msg = msgs.Current;

							if (msg.ArrivedTime >= since)
								messages.Add(msg);
						}
					}
					finally
					{
						msgs.Close();
					}

					foreach (var msg in messages)
					{
						QueueItem itm = currentItems.FirstOrDefault(i => i.Id == msg.Id);

						if (itm == null)
						{
							itm = CreateQueueItem(q.Queue, msg);
							itm.Processed = true;

							if (!PrepareQueueItemForAdd(itm))
								itm = null;
						}

						if (itm != null)
							r.Insert(0, itm);
					}
				}
				catch (Exception e)
				{
					OnError("Error occured when getting processed messages from queue \"" + qName + "\", " + e.Message, e, false);
				}
			}

			result.Count = (uint)r.Count;

			return result;
		}

		object _peekItemsLock = new object();
		List<QueueItem> _peekedItems = new List<QueueItem>();

		public QueueFetchResult GetUnprocessedMessages(QueueType type, IEnumerable<QueueItem> currentItems)
		{
			var result = new QueueFetchResult();
			var queues = _monitorMsmqQueues.Where(q => q.Queue.Type == type);

			if (queues.Count() == 0)
			{
				result.Items = new List<QueueItem>();
				return result;
			}

			List<QueueItem> r = new List<QueueItem>();
			result.Items = r;

			foreach (var q in queues)
			{
				var msmqQueue = q.Main;

				if (IsIgnoredQueue(q.Queue.Name) || !q.Main.CanRead)
					continue;

				SetupMessageReadPropertyFilters(q.Main, q.Queue.Type);

				// Add peaked items
				lock (_peekItemsLock)
				{
					if (_peekedItems.Count > 0)
					{

						r.AddRange(_peekedItems);
						_peekedItems.Clear();
					}
				}

				try
				{
					var msgs = q.Main.GetAllMessages();
					result.Count += (uint)msgs.Length;

					foreach (var msg in msgs)
					{

						QueueItem itm = currentItems.FirstOrDefault(i => i.Id == msg.Id);

						if (itm == null && !r.Any(i => i.Id == msg.Id))
						{
							itm = CreateQueueItem(q.Queue, msg);

							// Load Message names and check if its not an infra-message
							if (!PrepareQueueItemForAdd(itm))
								itm = null;
						}

						if (itm != null)
							r.Insert(0, itm);

						// Just fetch first 500
						if (r.Count > SbmqSystem.MAX_ITEMS_PER_QUEUE)
							break;
					}

				}
				catch (Exception e)
				{
					OnError("Error occured when processing queue " + q.Queue.Name + ", " + e.Message, e, false);
				}

			}

			return result;
		}

		Queue[] _monitorQueues;
    Dictionary<string, string> _connectionSettings;
		SbmqmMonitorState _monitorState;

		private void LoadQueues()
		{
			_monitorMsmqQueues.Clear();

			foreach (var queue in MonitorQueues)
				AddMsmqQueue(_connectionSettings["server"], queue);

		}
		private void AddMsmqQueue(string serverName, Queue queue)
		{
			try
			{
				_monitorMsmqQueues.Add(new MsmqMessageQueue(serverName, queue));
			}
			catch (Exception e)
			{
				OnError("Error occured when loading queue: '{0}\\{1}'\n\r".With(serverName, queue.Name), e, false);
			}
		}

		class PeekThreadParam
		{
			public Queue Queue { get; set; }
			public MessageQueue MsmqQueue { get; set; }
		}

		private bool TryAddItem(Message msg, Queue q)
		{

			lock (_peekItemsLock)
			{

				if (!_peekedItems.Any(i => i.Id == msg.Id))
				{

					var itm = CreateQueueItem(q, msg);

					if (PrepareQueueItemForAdd(itm))
						_peekedItems.Add(itm);


					return true;

				}
				else return false;
			}

		}

		bool _terminated = false;

		public void PeekMessages(object prm)
		{
			PeekThreadParam p = prm as PeekThreadParam;
			string qName = p.MsmqQueue.GetDisplayName();
			uint sameCount = 0;
			string lastId = string.Empty;

			bool _isPeeking = false;

			SetupMessageReadPropertyFilters(p.MsmqQueue, p.Queue.Type);

			p.MsmqQueue.PeekCompleted += (source, asyncResult) =>
			{
				if (_monitorState.IsMonitoringQueueType(p.Queue.Type))
				{
					Message msg = p.MsmqQueue.EndPeek(asyncResult.AsyncResult);

					if (msg.Id == lastId)
						sameCount++;

					else
					{
						sameCount = 0;
						TryAddItem(msg, p.Queue);
					}

					if (lastId != msg.Id)
						lastId = msg.Id;

				}
				_isPeeking = false;
			};

			while (!_terminated)
			{
				while (!_monitorState.IsMonitoringQueueType(p.Queue.Type))
				{
					Thread.Sleep(1000);

					if (_terminated)
						return;
				}

				if (!_isPeeking)
				{
					if (sameCount > 0)
					{
						if (sameCount / 10.0F == 1.0F)
							Thread.Sleep(100);

						else if (sameCount / 100.0F == 1.0F)
							Thread.Sleep(200);

						else if (sameCount % 300 == 0)
							Thread.Sleep(500);
					}
					p.MsmqQueue.BeginPeek();
					_isPeeking = true;
				}

				Thread.Sleep(100);
			}
		}

		void StartPeekThreads()
		{
			foreach (QueueType qt in Enum.GetValues(typeof(QueueType)))
			{
				if (qt != QueueType.Error)
				{
					foreach (var q in GetQueueListByType(qt))
					{
						var t = new Thread(new ParameterizedThreadStart(PeekMessages));
						if (q.Main.CanRead)
						{
							t.Name = "peek-msmq-" + q.GetDisplayName();
							t.Start(new PeekThreadParam() { MsmqQueue = q.Main, Queue = q.Queue });
						}
					}
				}
			}
		}

		public void Initialize(Dictionary<string, string> connectionSettings, Queue[] monitorQueues, SbmqmMonitorState monitorState)
		{
			_connectionSettings = connectionSettings;
			_monitorState = monitorState;
			_monitorQueues = monitorQueues;

			LoadQueues();
			StartPeekThreads();
		}


		private MsmqMessageQueue GetMessageQueue(QueueItem itm)
		{
			return _monitorMsmqQueues.Single(i => i.Queue.Type == itm.Queue.Type && i.Queue.Name == itm.Queue.Name);
		}

		public string LoadMessageContent(QueueItem itm)
		{
			if (itm.Content == null)
			{

				MsmqMessageQueue msmq = GetMessageQueue(itm);

				msmq.LoadMessageContent(itm);
			}

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
			var mgr = new ErrorManager();

			// TODO:
			// Check if Clustered Queue, due if Clustered && NonTransactional, then Error

			mgr.InputQueue = new EndpointAddress(errorQueue);

			mgr.ReturnAll();
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

			//var deserializer = new XmlMessageSerializer();

			XmlMessageSerializer _serializer = new XmlMessageSerializer();

			using (var stream = new MemoryStream(UTF8Encoding.Default.GetBytes(itm.Content ?? "")))
			{
				var rcx = ReceiveContext.FromBodyStream(stream);
				_serializer.Deserialize(rcx);

				//var query = rcx.DestinationAddress.Query;

				//var errorQueue = rcx.DestinationAddress.OriginalString.Replace(query, "") + "_error";

				//mgr.InputQueue = new EndpointAddress(errorQueue);

				mgr.ReturnMessageToSourceQueue(itm.Id, rcx);
			}
		}


		public void PurgeAllMessages()
		{
			_monitorMsmqQueues.ForEach(q => q.Purge());

			OnItemsChanged();
		}

		public void PurgeErrorAllMessages()
		{
			var items = _monitorMsmqQueues.Where(q => q.Queue.Type == QueueType.Error);

			if (items.Count() > 0)
			{
				items.ForEach(q => q.Purge());

				OnItemsChanged();
			}
		}

		public void PurgeErrorMessages(string queueName)
		{
			_monitorMsmqQueues.Where(q => q.Queue.Type == QueueType.Error && q.Queue.Name == queueName).Single().Purge();

			OnItemsChanged();
		}

		public void PurgeMessage(QueueItem itm)
		{
			MessageQueue q = GetMessageQueue(itm);

			if (q != null)
			{
				q.ReceiveById(itm.Id);

				itm.Processed = true;

				OnItemsChanged();
			}
		}

		public void Terminate()
		{
			_subscriptionQueueService = "";
			if(_bus != null)
				_bus.Dispose();
		}

		protected void OnItemsChanged()
		{
			if (_itemsChanged != null)
				_itemsChanged(this, EventArgs.Empty);
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

		public event EventHandler<Manager.WarningArgs> WarningOccured;

		public string[] AvailableMessageContentTypes
		{
			get { return new string[] { "XML", "JSON" }; }
		}


		public event EventHandler<ErrorArgs> ErrorOccured;

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

		private static readonly string[] IGNORE_DLL = new string[] { "\\Autofac.dll", "\\NLog.dll", 
                                                                  "\\Magnum.dll", "\\MassTransit.Transports.Msmq.dll", 
                                                                  "\\MassTransit.dll" };

		public Type[] GetAvailableCommands(string[] asmPaths, CommandDefinition cmdDef, bool suppressErrors)
		{
			List<Type> arr = new List<Type>();


			List<string> nonExistingPaths = new List<string>();


			foreach (var path in asmPaths)
			{

				if (Directory.Exists(path))
				{

					foreach (var dll in Directory.GetFiles(path, "*.dll"))
					{

						if (IGNORE_DLL.Any(a => dll.EndsWith(a)))
							continue;

						try
						{
							var asm = Assembly.LoadFrom(dll);
							foreach (Type t in asm.GetTypes())
							{

								if (cmdDef.IsCommand(t))
									arr.Add(t);

							}

						}
						catch (ReflectionTypeLoadException fte)
						{

							if (suppressErrors)
								continue;

							StringBuilder sb = new StringBuilder();
							if (fte.LoaderExceptions != null)
							{

								if (fte.LoaderExceptions.All(a => a.Message.EndsWith("does not have an implementation.")))
									continue;

								string lastMsg = null;
								foreach (var ex in fte.LoaderExceptions)
								{
									if (ex.Message != lastMsg)
										sb.AppendFormat(" - {0}\n\n", lastMsg = ex.Message);
								}
							}

							OnWarning("Could not search for Commands in Assembly '{0}'".With(Path.GetFileName(dll)), sb.ToString());

						}
						catch { }

					}
				}
				else nonExistingPaths.Add(path);
			}

			if (nonExistingPaths.Count > 0)
				OnError("The paths '{0}' doesn't exist, could not search for commands.".With(nonExistingPaths.Concat()));


			return arr.ToArray();
		}

		protected IServiceBus _bus;
		private string _subscriptionQueueService;
    public void SetupServiceBus(string[] assemblyPaths, CommandDefinition cmdDef, Dictionary<string, string> connectionSettings)
		{
      _subscriptionQueueService = connectionSettings["subscriptionQueueService"];

			if (_bus == null)
			{
				if (CommandContentFormat == "JSON")
				{
					if (_subscriptionQueueService != string.Empty)
					{

						_bus = ServiceBusFactory.New(sbc =>
						{
							sbc.UseMsmq();
							sbc.UseMsmq(x => x.UseSubscriptionService(_subscriptionQueueService));
							sbc.ReceiveFrom("msmq://localhost/ServiceBusMQ");
							sbc.UseJsonSerializer();
							sbc.UseControlBus();
						});
					}
					else
					{
						_bus = ServiceBusFactory.New(sbc =>
						{
							sbc.UseMsmq();
							//sbc.UseMsmq(x => x.UseSubscriptionService(subscriptionServiceUriString));
							sbc.ReceiveFrom("msmq://localhost/ServiceBusMQ");
							sbc.UseJsonSerializer();
							sbc.UseControlBus();
						});
					}
				}
				else
				{
					if (_subscriptionQueueService != string.Empty)
					{

						_bus = ServiceBusFactory.New(sbc =>
						{
							sbc.UseMsmq();
							sbc.UseMsmq(x => x.UseSubscriptionService(_subscriptionQueueService));
							sbc.ReceiveFrom("msmq://localhost/ServiceBusMQ");
							sbc.UseControlBus();
						});
					}
					else
					{
						_bus = ServiceBusFactory.New(sbc =>
						{
							sbc.UseMsmq();
							//sbc.UseMsmq(x => x.UseSubscriptionService(subscriptionServiceUriString));
							sbc.ReceiveFrom("msmq://localhost/ServiceBusMQ");
							sbc.UseControlBus();
						});
					}
				}
				Thread.Sleep(TimeSpan.FromSeconds(10));
			}
		}

    public void SendCommand(Dictionary<string, string> connectionStrings, string destinationQueue, object message)
		{
      var subscr = connectionStrings.GetValue(CS_SUBSCRIPTION_SERVICE);

      if( subscr.IsValid() )
				_bus.Publish(message);
			
      else {
        var sendTo = string.Format("msmq://{0}/{1}", connectionStrings.GetValue(CS_SERVER), destinationQueue);
				_bus.GetEndpoint(new Uri(sendTo)).Send(message);
			}
		}


    public string SerializeCommand(object cmd) {
      try {
        return MessageSerializer.SerializeMessage(cmd, CommandContentFormat);

      } catch( Exception e ) {
        OnError("Failed to Serialize Command to " + CommandContentFormat, e);
        return null;
      }
    }
    public object DeserializeCommand(string cmd, Type cmdType) {
      try {
        return MessageSerializer.DeserializeMessage(cmd, cmdType, CommandContentFormat);

      } catch( Exception e ) {
        OnError("Failed to Parse Command string as " + CommandContentFormat, e);
        return null;
      }
    }

		string _commandContentFormat;

		public string CommandContentFormat
		{
			get { 
				return _commandContentFormat; 
			}
			set { 
				_commandContentFormat = value; 
			}
		}

		public MessageSubscription[] GetMessageSubscriptions(string server)
		{
			List<MessageSubscription> r = new List<MessageSubscription>();

			foreach (var queueName in MessageQueue.GetPrivateQueuesByMachine(server).
												  Where(q => q.QueueName.EndsWith("_subscriptions")).Select(q => q.QueueName))
			{

				MessageQueue q = Msmq.Create(server, queueName, QueueAccessMode.ReceiveAndAdmin);

				q.MessageReadPropertyFilter.Label = true;
				q.MessageReadPropertyFilter.Body = true;

				try
				{
					var publisher = q.GetDisplayName().Replace("_subscriptions", string.Empty);

					foreach (var msg in q.GetAllMessages())
					{

						var itm = new MessageSubscription();
						itm.FullName = GetSubscriptionType(ReadMessageStream(msg.BodyStream));
						itm.Name = ParseClassName(itm.FullName);
						itm.Subscriber = msg.Label;
						itm.Publisher = publisher;

						r.Add(itm);
					}
				}
				catch (Exception e)
				{
					OnError("Error occured when getting subcriptions", e, true);
				}

			}

			return r.ToArray();
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

		protected string ReadMessageStream(Stream s)
		{
			using (StreamReader r = new StreamReader(s, Encoding.Default))
				return r.ReadToEnd().Replace("\0", "");
		}

		private string ParseClassName(string asmName)
		{

			if (asmName.IsValid())
			{

				int iEnd = asmName.IndexOf(',');
				int iStart = asmName.LastIndexOf('.', iEnd);

				if (iEnd > -1 && iStart > -1)
				{
					iStart++;
					return asmName.Substring(iStart, iEnd - iStart);
				}

			}

			return asmName;
		}
	}
}
