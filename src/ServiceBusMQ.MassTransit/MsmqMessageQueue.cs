#region File Information
/********************************************************************
  Project: ServiceBusMQ.MassTransit
  File:    MsmqMessageQueue.cs
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
using ServiceBusMQ.Model;
using System.Messaging;
using System.IO;

namespace ServiceBusMQ.MassTransit
{
	public class MsmqMessageQueue
	{

		public Queue Queue { get; set; }

		public MessageQueue Main { get; set; }
		public MessageQueue Journal { get; set; }

		public bool UseJournalQueue { get { return Main.UseJournalQueue; } }
		public bool CanReadJournalQueue { get { return Main.UseJournalQueue && Journal.CanRead; } }

		public MessageQueue _mainContent;
		public MessageQueue _journalContent;


		public MsmqMessageQueue(string serverName, Queue queue)
		{
			Queue = queue;

			Main = Msmq.Create(serverName, queue.Name, QueueAccessMode.ReceiveAndAdmin);

			_mainContent = Msmq.Create(serverName, queue.Name, QueueAccessMode.ReceiveAndAdmin);
			_mainContent.MessageReadPropertyFilter.ClearAll();
			_mainContent.MessageReadPropertyFilter.Body = true;


			if (Main.UseJournalQueue)
			{ // Error when trying to use FormatName, strange as it should work according to MSDN. Temp solution for now.
				Journal = new MessageQueue(string.Format(@"{0}\Private$\{1};JOURNAL", serverName, queue.Name));

				_journalContent = new MessageQueue(string.Format(@"{0}\Private$\{1};JOURNAL", serverName, queue.Name));
				_journalContent.MessageReadPropertyFilter.ClearAll();
				_journalContent.MessageReadPropertyFilter.Body = true;
			}
		}

		public static implicit operator MessageQueue(MsmqMessageQueue q)
		{
			return q.Main;
		}

		public void LoadMessageContent(QueueItem itm)
		{
			Message msg = null;

			if (!itm.Processed)
			{
				try
				{
					msg = _mainContent.PeekById(itm.Id);

				}
				catch
				{

					if (_journalContent != null)
					{
						try
						{
							msg = _journalContent.ReceiveById(itm.Id);

						}
						catch
						{
							itm.Content = "**MESSAGE HAS BEEN PROCESSED OR PURGED**";
						}

					}
					else itm.Content = "**MESSAGE HAS BEEN PROCESSED OR PURGED AND JOURNALING IS TURNED OFF**";

				}
			}
			else
			{

				if (_journalContent != null)
				{

					try
					{
						msg = _journalContent.ReceiveById(itm.Id);
					}
					catch
					{
						itm.Content = "**MESSAGE HAS BEEN PURGED FROM JOURNAL**";
					}

				}
				else
				{
					itm.Content = "**MESSAGE HAS BEEN PROCESSED OR PURGED AND JOURNALING IS TURNED OFF**";
				}

			}

			if (msg != null)
				itm.Content = ReadMessageStream(msg.BodyStream);
		}
		private string ReadMessageStream(Stream s)
		{
			using (StreamReader r = new StreamReader(s, Encoding.Default))
				return r.ReadToEnd().Replace("\0", "");
		}


		internal string GetDisplayName()
		{
			return "hola enfermera";
			//return Main.GetDisplayName();
		}

		internal void Purge()
		{
			Main.Purge();
		}



	}
}
