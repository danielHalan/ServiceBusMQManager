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
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using ServiceBusMQ.Model;

namespace ServiceBusMQ.NServiceBus4 {
  public class AzureMessageQueue {

    string _connectionStr;

    public Queue Queue { get; set; }
    public Queue ErrorQueue { get; private set; }

    public DateTime LastPeek { get; set; }

    public QueueClient Main { get; set; }
    public QueueClient DeadLetter { get; set; }

    public QueueDescription Info { get; set; }

    public static long GetMessageCount(MessageCountDetails details) { 
      return details.ActiveMessageCount + 
                    details.ScheduledMessageCount + 
                    details.TransferMessageCount; 
      
    }

    public static long GetDeadLetterMessageCount(MessageCountDetails details) {
        return details.DeadLetterMessageCount +
                details.TransferDeadLetterMessageCount;
    }

    //public bool UseJournalQueue { get { return Main.UseJournalQueue; } }
    //public bool CanReadJournalQueue { get { return Main.UseJournalQueue && Journal.CanRead; } }


    public AzureMessageQueue(string connectionString, Queue queue) {
      _connectionStr = connectionString;
      Queue = queue;
      ErrorQueue = new Model.Queue(queue.Name, QueueType.Error, 0xFF0000);

      Main = QueueClient.CreateFromConnectionString(connectionString, queue.Name, ReceiveMode.ReceiveAndDelete);
      DeadLetter = QueueClient.CreateFromConnectionString(connectionString, QueueClient.FormatDeadLetterPath(queue.Name), ReceiveMode.ReceiveAndDelete);

      var ns = NamespaceManager.CreateFromConnectionString(connectionString);
      Info = ns.GetQueue(queue.Name);

      //Info = new QueueDescription(

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

    internal bool HasUpdatedSince(DateTime dt) {
      return Info.UpdatedAt > dt;
    }


    public bool HasChanged { get { return HasUpdatedSince(LastPeek); } }
  }
}
