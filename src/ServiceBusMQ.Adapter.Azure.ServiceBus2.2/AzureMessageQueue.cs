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
<<<<<<< HEAD
using NLog;
using ServiceBusMQ.Model;

namespace ServiceBusMQ.Adapter.Azure.ServiceBus22 {

  public class AzureMessageQueue {

    protected Logger _log = LogManager.GetCurrentClassLogger();

    static Dictionary<string, NamespaceManager> _nsManagers = new Dictionary<string, NamespaceManager>();

    string _connectionStr;

    public Queue Queue { get; set; }
    //public Queue ErrorQueue { get; private set; }

    public QueueClient Main { get; set; }

    public QueueDescription Info { get; set; }

    public bool IsDeadLetterQueue { get; private set; }

    long _checkSum = 0;

    public long GetMessageCount() {

      if( IsDeadLetterQueue )
        return Info.MessageCountDetails.DeadLetterMessageCount +
                  Info.MessageCountDetails.TransferDeadLetterMessageCount;
      else
        return Info.MessageCountDetails.ActiveMessageCount +
                      Info.MessageCountDetails.ScheduledMessageCount +
                      Info.MessageCountDetails.TransferMessageCount;

    }

    public AzureMessageQueue(string connectionString, Queue queue, bool deadLetter = false) {
      _connectionStr = connectionString;
      IsDeadLetterQueue = deadLetter;

      Queue = !deadLetter ? queue : new Model.Queue(queue.Name, QueueType.Error, 0xFF0000, queue.ContentFormat);

      var name = !deadLetter ? queue.Name : QueueClient.FormatDeadLetterPath(queue.Name);
      Main = QueueClient.CreateFromConnectionString(connectionString, name, ReceiveMode.ReceiveAndDelete);
=======
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
>>>>>>> 3dd34e76b2bd5c60a3431e8f5fa66de0154cca6c
    }

    public static implicit operator QueueClient(AzureMessageQueue q) {
      return q.Main;
    }


<<<<<<< HEAD
    public void Purge() {
      var q = Main; //QueueClient.CreateFromConnectionString(_connectionStr, Queue.Name, ReceiveMode.ReceiveAndDelete);
      
      int max = 0xFFFF;

      // DH: Seems as the returned count varies on the size of the message content, returning usually around 150 msgs
      IEnumerable<BrokeredMessage> msgs = null;
      do {
        msgs = q.ReceiveBatch(max);
      } while( msgs.Count() > 0 );
=======
    internal void Purge() {
      var q = QueueClient.CreateFromConnectionString(_connectionStr, Queue.Name, ReceiveMode.ReceiveAndDelete);

      int max = 0xFFFF;
      var msgs = q.ReceiveBatch(max);
      Console.WriteLine(msgs.Count());
      //.Count() == max ) { }

>>>>>>> 3dd34e76b2bd5c60a3431e8f5fa66de0154cca6c

      //while( q.ReceiveBatch(max).Count() == max ) { }
    }

<<<<<<< HEAD
    public bool HasUpdatedSince(DateTime dt) {
      return Info.UpdatedAt > dt;
    }
    public bool HasChanged() {

      try {
        NamespaceManager mgr = null;

        if( !_nsManagers.ContainsKey(_connectionStr) ) {
          mgr = NamespaceManager.CreateFromConnectionString(_connectionStr);
          _nsManagers.Add(_connectionStr, mgr);
        } else mgr = _nsManagers[_connectionStr];

        Info = mgr.GetQueue(Queue.Name);

        if( _checkSum != Info.MessageCount + Info.SizeInBytes ) { 
          _log.Debug(" === " + Queue.Name + " - " + Info.UpdatedAt + " =======================");
          _log.Debug("++ Has Changed, MessageCount: " + Info.MessageCount + ", SizeInBytes: " + Info.SizeInBytes);
        } else {
          _log.Debug("=== {0} = {1}:{2}b =======================".With(Queue.Name, Info.MessageCount, Info.SizeInBytes));
        }
        return _checkSum != Info.MessageCount + Info.SizeInBytes;

      } finally {
        if( Info != null )
          _checkSum = Info.MessageCount + Info.SizeInBytes;
      }
    }


  }


=======
    internal bool HasUpdatedSince(DateTime dt) {
      return Info.UpdatedAt > dt;
    }


    public bool HasChanged { get { return HasUpdatedSince(LastPeek); } }
  }
>>>>>>> 3dd34e76b2bd5c60a3431e8f5fa66de0154cca6c
}
