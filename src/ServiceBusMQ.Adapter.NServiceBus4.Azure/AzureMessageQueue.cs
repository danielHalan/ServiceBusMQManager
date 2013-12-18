#region File Information
/********************************************************************
<<<<<<< HEAD
  Project: ServiceBusMQ.Adapter.NServiceBus4.Azure.SB22
  File:    AzureMessageQueue.cs
  Created: 2013-11-30
=======
  Project: ServiceBusMQ.NServiceBus4.Azure
  File:    AzureMessageQueue.cs
  Created: 2013-10-11
>>>>>>> 3dd34e76b2bd5c60a3431e8f5fa66de0154cca6c

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
<<<<<<< HEAD
using ServiceBusMQ.Model;
using ServiceBusMQ.NServiceBus;

namespace ServiceBusMQ.Adapter.NServiceBus4.Azure.SB22 {

  public class AzureMessageQueue : ServiceBusMQ.Adapter.Azure.ServiceBus22.AzureMessageQueue, IMessageQueue {

    public AzureMessageQueue(string connectionString, Queue queue, bool deadLetter = false)
      : base(connectionString, queue, deadLetter) {
    }

=======
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

      int max = 0xFFFF;
      var msgs = q.ReceiveBatch(max);
      Console.WriteLine(msgs.Count());
      //.Count() == max ) { }


      //while( q.ReceiveBatch(max).Count() == max ) { }
    }

  
>>>>>>> 3dd34e76b2bd5c60a3431e8f5fa66de0154cca6c
  }
}
