#region File Information
/********************************************************************
  Project: ServiceBusMQ.MassTransit
  File:    ErrorManager.cs
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
using System.Messaging;
using MassTransit;
using System.Transactions;
using MassTransit.Transports.Msmq;
using MassTransit.Serialization;
using MassTransit.Context;
using MassTransit.Transports;
using Magnum.Extensions;

namespace ServiceBusMQ.MassTransit
{
	public class ErrorManager
	{
		private const string NonTransactionalQueueErrorMessageFormat = "Queue '{0}' must be transactional.";
		private const string NoMessageFoundErrorFormat = "INFO: No message found with ID '{0}'. Going to check headers of all messages for one with that original ID.";
		private MessageQueue queue;
		private static readonly TimeSpan TimeoutDuration = TimeSpan.FromSeconds(5);
		public bool ClusteredQueue { get; set; }
		/// <summary>
		/// Constant taken from V2.6: 
		/// https://github.com/NServiceBus/NServiceBus/blob/v2.5/src/impl/unicast/NServiceBus.Unicast.Msmq/MsmqTransport.cs
		/// </summary>
		private const string FAILEDQUEUE = "FailedQ";

		public virtual IEndpointAddress InputQueue
		{
			set
			{
				var path = value.Uri.GetLocalName();
				var q = new MessageQueue(path);

				//if ((!ClusteredQueue) && (!q.Transactional))
				//    throw new ArgumentException(string.Format(NonTransactionalQueueErrorMessageFormat, q.Path));

				queue = q;

				var mpf = new MessagePropertyFilter();
				mpf.SetAll();

				queue.MessageReadPropertyFilter = mpf;
			}
		}

		public void ReturnAll()
		{
			//foreach (var m in queue.GetAllMessages())
			//    ReturnMessageToSourceQueue(m.Id, null);
		}

		public ErrorManager()
		{
			Transports = new TransportCache();
			Transports.AddTransportFactory(new MsmqTransportFactory());
		}


		public static TransportCache Transports { get; private set; }

		/// <summary>
		/// May throw a timeout exception if a message with the given id cannot be found.
		/// </summary>
		/// <param name="messageId"></param>
		public void ReturnMessageToSourceQueue(string messageId, IReceiveContext context)
		{
			try
			{
				var query = context.DestinationAddress.Query;
				var errorQueue = context.DestinationAddress.OriginalString.Replace(query, "") + "_error";

				Uri fromUri = new Uri(errorQueue);
				Uri toUri = context.DestinationAddress;

				IInboundTransport fromTransport = Transports.GetInboundTransport(fromUri);
				IOutboundTransport toTransport = Transports.GetOutboundTransport(toUri);

				fromTransport.Receive(receiveContext =>
				{
					if (receiveContext.MessageId == messageId)
					{
						return ctx =>
						{
							var moveContext = new MoveMessageSendContext(ctx);
							toTransport.Send(moveContext);
						};
					}

					return null;

				}, 5.Seconds());

				Console.WriteLine("Success.");
			}
			catch (MessageQueueException ex)
			{
				if (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
				{
					Console.WriteLine(NoMessageFoundErrorFormat, context.Id);

					foreach (var m in queue.GetAllMessages())
					{
						var tm = TransportMessageHeaders.Create(m.Extension);

						if (tm[""] != null)
						{
							//if (messageId != tm[""])
							//    continue;

							Console.WriteLine("Found message - going to return to queue.");

							using (var tx = new TransactionScope())
							{
								using (var q = new MessageQueue(new EndpointAddress(tm[""]).Path))
									q.Send(m, MessageQueueTransactionType.Automatic);

								queue.ReceiveByLookupId(MessageLookupAction.Current, m.LookupId,
														MessageQueueTransactionType.Automatic);

								tx.Complete();
							}

							Console.WriteLine("Success.");
							//scope.Complete();

							return;
						}
					}
				}
			}
			//}
		}

		/// <summary>
		/// For compatibility with V2.6:
		/// Gets the label of the message stripping out the failed queue.
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		public static string GetLabelWithoutFailedQueue(Message m)
		{
			if (string.IsNullOrEmpty(m.Label))
				return string.Empty;

			if (!m.Label.Contains(FAILEDQUEUE))
				return m.Label;

			var startIndex = m.Label.IndexOf(string.Format("<{0}>", FAILEDQUEUE));
			var endIndex = m.Label.IndexOf(string.Format("</{0}>", FAILEDQUEUE));
			endIndex += FAILEDQUEUE.Length + 3;

			return m.Label.Remove(startIndex, endIndex - startIndex);
		}
		/// <summary>
		/// For compatibility with V2.6:
		/// Returns the queue whose process failed processing the given message
		/// by accessing the label of the message.
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		public static string GetFailedQueueFromLabel(Message m)
		{
			if (m.Label == null)
				return null;

			if (!m.Label.Contains(FAILEDQUEUE))
				return null;

			var startIndex = m.Label.IndexOf(string.Format("<{0}>", FAILEDQUEUE)) + FAILEDQUEUE.Length + 2;
			var count = m.Label.IndexOf(string.Format("</{0}>", FAILEDQUEUE)) - startIndex;

			return m.Label.Substring(startIndex, count);
		}
	}
}
