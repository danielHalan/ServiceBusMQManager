using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;
using ServiceBusMQ.Model;

namespace ServiceBusMQ.NServiceBus {
  public class AzureMessageQueue : IMessageQueue {

    public Queue Queue { get; set; }

    public QueueClient Main { get; set; }
    //public QueueClient Journal { get; set; }

    //public bool UseJournalQueue { get { return Main.UseJournalQueue; } }
    //public bool CanReadJournalQueue { get { return Main.UseJournalQueue && Journal.CanRead; } }


    public AzureMessageQueue(string connectionString, Queue queue) { 
      Queue = queue;

      Main = QueueClient.CreateFromConnectionString(connectionString, queue.Name);

      //Main = Msmq.Create(connectionString, queue.Name, QueueAccessMode.ReceiveAndAdmin);
      
      //_mainContent = Msmq.Create(connectionString, queue.Name, QueueAccessMode.ReceiveAndAdmin);
      //_mainContent.MessageReadPropertyFilter.ClearAll();
      //_mainContent.MessageReadPropertyFilter.Body = true;


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
  
  
  }
}
