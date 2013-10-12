using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using ServiceBusMQ.Manager;

namespace ServiceBusMQ.NServiceBus.Azure {
  public class NServiceBus_AzureMQ_Discovery : IServiceBusDiscovery {

    public string ServiceBusName {
      get { return "NServiceBus"; }
    }

    public string MessageQueueType {
      get { return "AzureMQ"; }
    }

    public string[] AvailableMessageContentTypes {
      get { return new string[] { "XML", "JSON" }; }
    }

    public ServerConnectionParameter[] ServerConnectionParameters { 
      get { 
        return new ServerConnectionParameter[] { 
          ServerConnectionParameter.Create("connectionStr", "Connection String")
        };
      }
    }


    public bool CanAccessServer(Dictionary<string, string> connectionSettings) {
      return true;
    }

    public bool CanAccessQueue(Dictionary<string, string> connectionSettings, string queueName) {
      return true;
      //var queue = Msmq.Create(server, queueName, QueueAccessMode.ReceiveAndAdmin);

      //return queue != null ? queue.CanRead : false;
    }

    public string[] GetAllAvailableQueueNames(Dictionary<string, string> connectionSettings) {
      var mgr = NamespaceManager.CreateFromConnectionString(connectionSettings["connectionStr"]);
      return mgr.GetQueues().Select( q => q.Path ).ToArray();

      //return MessageQueue.GetPrivateQueuesByMachine(server).Where(q => !IsIgnoredQueue(q.QueueName)).
      //    Select(q => q.QueueName.Replace("private$\\", "")).ToArray();
    }

    //private bool IsIgnoredQueue(string queueName) {
    //  return ( queueName.EndsWith(".subscriptions") || queueName.EndsWith(".retries") || queueName.EndsWith(".timeouts") || queueName.EndsWith(".timeoutsdispatcher") );
    //}



  }
}
