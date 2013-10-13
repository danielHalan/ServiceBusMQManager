#region File Information
/********************************************************************
  Project: ServiceBusMQ.MassTransit
  File:    Msmq.cs
  Created: 2013-10-11

  Author(s):
    Daniel Halan
    Juan J. Chiw

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Messaging;

namespace ServiceBusMQ.MassTransit
{
	public static class Msmq
	{

		public static MessageQueue Create(string serverName, string queueName, QueueAccessMode accessMode)
		{
			if (!queueName.StartsWith("private$\\"))
				queueName = "private$\\" + queueName;

			queueName = string.Format("FormatName:DIRECT=OS:{0}\\{1}", !Tools.IsLocalHost(serverName) ? serverName : ".", queueName);

			return new MessageQueue(queueName, false, true, accessMode);
		}

		public static MessageQueue Create(string queueFormatName, QueueAccessMode accessMode)
		{
			return new MessageQueue(queueFormatName, false, true, accessMode);
		}

		public static string GetDisplayName(this MessageQueue queue)
		{
			return queue.FormatName.Substring(queue.FormatName.LastIndexOf('\\') + 1);
		}

	}
}
