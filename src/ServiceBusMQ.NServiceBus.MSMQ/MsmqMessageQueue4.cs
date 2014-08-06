using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using ServiceBusMQ.Model;

namespace ServiceBusMQ.NServiceBus.MSMQ {

  public class MsmqMessageQueue4 : MsmqMessageQueue {

    //public MessageQueue Retries { get; set; }
    //public MessageQueue Timeouts { get; set; }
    //public MessageQueue TimeoutsDispatcher { get; set; }

    public MsmqMessageQueue4(string serverName, Queue queue)
      : base(serverName, queue) {

      //Retries = Msmq.Create(serverName, queue.Name + ".retries", QueueAccessMode.ReceiveAndAdmin);
      //Timeouts = Msmq.Create(serverName, queue.Name + ".timeouts", QueueAccessMode.ReceiveAndAdmin);
      //TimeoutsDispatcher = Msmq.Create(serverName, queue.Name + ".timeoutssispatcher", QueueAccessMode.ReceiveAndAdmin);
    }
  }

}
