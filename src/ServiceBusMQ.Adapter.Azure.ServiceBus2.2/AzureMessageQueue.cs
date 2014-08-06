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
        return Info.MessageCount;
      //return Info.MessageCountDetails.ActiveMessageCount +
      //              Info.MessageCountDetails.ScheduledMessageCount +
      //              Info.MessageCountDetails.TransferMessageCount;

    }

    public AzureMessageQueue(string connectionString, Queue queue, bool deadLetter = false) {
      _connectionStr = connectionString;
      IsDeadLetterQueue = deadLetter;

      Queue = !deadLetter ? queue : new Model.Queue(queue.Name, QueueType.Error, 0xFF0000, queue.ContentFormat);

      var name = !deadLetter ? queue.Name : QueueClient.FormatDeadLetterPath(queue.Name);
      Main = QueueClient.CreateFromConnectionString(connectionString, name, ReceiveMode.ReceiveAndDelete);
    }

    public static implicit operator QueueClient(AzureMessageQueue q) {
      return q.Main;
    }


    public void Purge() {
      try {
        var q = QueueClient.CreateFromConnectionString(_connectionStr, Main.Path, ReceiveMode.ReceiveAndDelete);

        int max = 0xFFFF;

        // DH: Seems as the returned count varies on the size of the message content, returning usually around 150 msgs
        IEnumerable<BrokeredMessage> msgs = null;
        do {
          msgs = q.ReceiveBatch(max);
        } while( msgs.Count() > 0 );

      } catch( Exception e ) {
        _log.Trace("Failed to Purge messages from queue, " + Main.Path);
      }

      //while( q.ReceiveBatch(max).Count() == max ) { }
    }

    public bool HasUpdatedSince(DateTime dt) {
      return Info.UpdatedAt > dt;
    }
    public bool HasChanged(uint currentMsgCount) {

      try {
        NamespaceManager mgr = null;

        if( !_nsManagers.ContainsKey(_connectionStr) ) {
          mgr = NamespaceManager.CreateFromConnectionString(_connectionStr);
          _nsManagers.Add(_connectionStr, mgr);
        } else mgr = _nsManagers[_connectionStr];

        Info = mgr.GetQueue(Queue.Name);

        if( currentMsgCount != Info.MessageCount || ( _checkSum != Info.MessageCount + Info.SizeInBytes ) ) {
          _log.Debug(" === " + Queue.Name + " - " + Info.UpdatedAt + " =======================");
          _log.Debug("++ Has Changed, MessageCount: " + Info.MessageCount + ", SizeInBytes: " + Info.SizeInBytes);
          return true;

        } else {
          _log.Debug("=== {0} = {1}:{2}b =======================".With(Queue.Name, Info.MessageCount, Info.SizeInBytes));
          return false;
        }

      } finally {
        if( Info != null )
          _checkSum = Info.MessageCount + Info.SizeInBytes;
      }
    }


  }


}
