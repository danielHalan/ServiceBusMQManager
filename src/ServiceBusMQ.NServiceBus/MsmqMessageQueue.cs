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
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text;
using ServiceBusMQ.Model;

namespace ServiceBusMQ.NServiceBus {

  public class MsmqMessageQueue {

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
      Message msg = ( itm.Processed ) ? _journalContent.ReceiveById(itm.Id) : _mainContent.PeekById(itm.Id);

      itm.Content = ReadMessageStream(msg.BodyStream);
    }
    private string ReadMessageStream(Stream s) {
      using( StreamReader r = new StreamReader(s, Encoding.Default) )
        return r.ReadToEnd().Replace("\0", "");
    }


    internal string GetDisplayName() {
      return Main.GetDisplayName();
    }

    internal void Purge() {
      Main.Purge();
    }



  }
}
