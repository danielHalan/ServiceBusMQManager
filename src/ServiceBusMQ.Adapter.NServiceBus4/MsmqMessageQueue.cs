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
using System.IO;
using System.Messaging;
using System.Text;
using ServiceBusMQ.Model;
using ServiceBusMQ.NServiceBus;

namespace ServiceBusMQ.NServiceBus4 {

  public class MsmqMessageQueue : IMessageQueue {

    public Queue Queue { get; set; }

    public MessageQueue Main { get; set; }
    public MessageQueue Journal { get; set; }

    public bool UseJournalQueue { get { return Main.UseJournalQueue; } }
    public bool CanReadJournalQueue { get { return Main.UseJournalQueue && Journal.CanRead; } }

    public MessageQueue _mainContent;
    public MessageQueue _journalContent;


    public MsmqMessageQueue(string serverName, Queue queue) {
      Queue = queue;

      Main = Msmq.Create(serverName, queue.Name, QueueAccessMode.ReceiveAndAdmin);

      _mainContent = Msmq.Create(serverName, queue.Name, QueueAccessMode.ReceiveAndAdmin);
      _mainContent.MessageReadPropertyFilter.ClearAll();
      _mainContent.MessageReadPropertyFilter.Body = true;


      if( Main.UseJournalQueue ) { // Error when trying to use FormatName, strange as it should work according to MSDN. Temp solution for now.
        Journal = new MessageQueue(string.Format(@"{0}\Private$\{1};JOURNAL", serverName, queue.Name));

        _journalContent = new MessageQueue(string.Format(@"{0}\Private$\{1};JOURNAL", serverName, queue.Name));
        _journalContent.MessageReadPropertyFilter.ClearAll();
        _journalContent.MessageReadPropertyFilter.Body = true;
      }
    }

    public static implicit operator MessageQueue(MsmqMessageQueue q) {
      return q.Main;
    }

    public void LoadMessageContent(QueueItem itm) {
      Message msg = null;

      if( !itm.Processed ) {
        try {
          msg = _mainContent.PeekById(itm.Id);

        } catch {

          if( _journalContent != null ) {
            try {
              msg = _journalContent.ReceiveById(itm.Id);

            } catch {
              itm.Content = "**MESSAGE HAS BEEN PROCESSED OR PURGED**";
            }

          } else itm.Content = "**MESSAGE HAS BEEN PROCESSED OR PURGED AND JOURNALING IS TURNED OFF**";

        }
      } else {

        if( _journalContent != null ) {

          try {
            msg = _journalContent.ReceiveById(itm.Id);
          } catch {
            itm.Content = "**MESSAGE HAS BEEN PURGED FROM JOURNAL**";
          }

        } else {
          itm.Content = "**MESSAGE HAS BEEN PROCESSED OR PURGED AND JOURNALING IS TURNED OFF**";
        }

      }

      if( msg != null )
        itm.Content = ReadMessageStream(msg.BodyStream);
    }
    private string ReadMessageStream(Stream s) {
      using( StreamReader r = new StreamReader(s, Encoding.Default) )
        return r.ReadToEnd().Replace("\0", "");
    }

    public Message[] GetAllMessages() {
      int retries = 0;
      do {
        try {
          return Main.GetAllMessages();
        } catch( MessageQueueException mqe ) {
          if( mqe.ErrorCode == 2147500037 ) // 0x80004005, Message that the cursor is currently pointing to has been removed from the queue by another process or by another call to Receive without the use of this cursor.
            continue;
        }
      } while( ++retries < 5 );

      throw new Exception("Failed to get messages from Queue {0}, maximum retries reached".With(Queue.Name));
    }

    internal string GetDisplayName() {
      return Main.GetDisplayName();
    }

    internal void Purge() {
      Main.Purge();
    }



  }
}
