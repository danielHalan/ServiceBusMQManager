#region File Information
/********************************************************************
  Project: ServiceBusMQ.NServiceBus
  File:    MsmqMessageQueue.cs
  Created: 2013-01-30

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;

namespace ServiceBusMQ.NServiceBus {

  public class MsmqMessageQueue {

    public MessageQueue Main { get; set; }
    public MessageQueue Journal { get; set; }

    public bool UseJournalQueue { get { return Main.UseJournalQueue; } }

    public MsmqMessageQueue(string serverName, string queueName) { 
      Main = CreateMessageQueue(serverName, queueName, QueueAccessMode.ReceiveAndAdmin);
      if( Main.UseJournalQueue )
        Journal = new MessageQueue(string.Format(@"{0}\Private$\{1};JOURNAL", serverName, queueName));
    }

    public static implicit operator MessageQueue(MsmqMessageQueue q) {
      return q.Main;
    }


    private MessageQueue CreateMessageQueue(string serverName, string queueName, QueueAccessMode accessMode) {
      if( !queueName.StartsWith("private$\\") )
        queueName = "private$\\" + queueName;

      queueName = string.Format("FormatName:DIRECT=OS:{0}\\{1}", !Tools.IsLocalHost(serverName) ? serverName : ".", queueName);

      return new MessageQueue(queueName, false, true, accessMode);
    }
    private MessageQueue CreateMessageQueue(string queueFormatName, QueueAccessMode accessMode) {
      return new MessageQueue(queueFormatName, false, true, accessMode);
    }



    internal string GetDisplayName() {
      return Main.GetDisplayName();
    }

    internal void Purge() {
      Main.Purge();
    }



  }
}
