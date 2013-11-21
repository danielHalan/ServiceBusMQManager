#region File Information
/********************************************************************
  Project: ServiceBusMQ.NServiceBus4.Azure
  File:    AzureMessageQueue.cs
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
using Microsoft.ServiceBus.Messaging;
using ServiceBusMQ.Model;
using ServiceBusMQ.NServiceBus;

namespace ServiceBusMQ.NServiceBus4 {
  public class AzureMessageQueue : IMessageQueue {


    string _connectionStr;

    public Queue Queue { get; set; }

    public QueueClient Main { get; set; }
    //public QueueClient Journal { get; set; }

    //public bool UseJournalQueue { get { return Main.UseJournalQueue; } }
    //public bool CanReadJournalQueue { get { return Main.UseJournalQueue && Journal.CanRead; } }


    public AzureMessageQueue(string connectionString, Queue queue) {
      _connectionStr = connectionString;
      Queue = queue;

      Main = QueueClient.CreateFromConnectionString(connectionString, queue.Name, ReceiveMode.ReceiveAndDelete);
     

      //if( Main.UseJournalQueue ) { // Error when trying to use FormatName, strange as it should work according to MSDN. Temp solution for now.
      //  Journal = new MessageQueue(string.Format(@"{0}\Private$\{1};JOURNAL", connectionString, queue.Name));
        
      //  _journalContent = new MessageQueue(string.Format(@"{0}\Private$\{1};JOURNAL", connectionString, queue.Name));
      //  _journalContent.MessageReadPropertyFilter.ClearAll();
      //  _journalContent.MessageReadPropertyFilter.Body = true;
      //}
    }

    public static implicit operator QueueClient(AzureMessageQueue q) {
      return q.Main;
    }


    internal void Purge() {
      var q = QueueClient.CreateFromConnectionString(_connectionStr, Queue.Name, ReceiveMode.ReceiveAndDelete);

      while( q.Receive() != null ) { }
    }

  
  }
}
