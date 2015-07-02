using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace ServiceBusMQ.Adapter.Azure.ServiceBus22 {
  public static class AzureServiceBusHelper {

    static Logger _log = LogManager.GetCurrentClassLogger();

    public static void PurgeAllMessages(IEnumerable<AzureMessageQueue> monitorQueues) {
      List<Task> tasks = new List<Task>();

      foreach( var queue in monitorQueues ) {

        if( AzureServiceBusReceiver.GetAzureQueueCount(queue.Queue.Name) > 0 ) {
          tasks.Add(Task.Factory.StartNew(() => { 
            try { 
              _log.Trace("Purging Queue {0}...".With(queue.Queue.Name));
              queue.Purge();
              _log.Trace("Finished Purging Queue " + queue.Queue.Name);

            } catch(Exception ex) { 
              _log.Error("Error when Purgin queue {0}, {1}".With(queue.Queue.Name, ex));
            }
          }));
          Thread.Sleep(500);
        }


        if( tasks.Count > 0 && ( tasks.Count % 15) == 0 ) {
          Task.WaitAll(tasks.ToArray());
          tasks.Clear();
        }
      }

      if( tasks.Count > 0 )
        Task.WaitAll(tasks.ToArray());

    }

  
  }
}
