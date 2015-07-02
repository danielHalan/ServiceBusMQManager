using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using NLog;
using ServiceBusMQ.Model;

namespace ServiceBusMQ.Adapter.Azure.ServiceBus22 {

  public class AzureServiceBusReceiver {
    
    static Logger _log = LogManager.GetCurrentClassLogger();

    protected static readonly List<QueueItem> EMPTY_LIST = new List<QueueItem>();


    // We Store what 'Azure' think they have, not the actual count, which may differ at times.
    private static Dictionary<string, uint> _azureQueueItemsCount = new Dictionary<string,uint>();
    public static uint GetAzureQueueCount(string queueName) {
      if( !_azureQueueItemsCount.ContainsKey(queueName) )
        _azureQueueItemsCount.Add(queueName, 0);

      return _azureQueueItemsCount[queueName];
    }
    private static void SetAzureQueueCount(string queueName, uint count) {
      _azureQueueItemsCount[queueName] = count;
    }

    // The REAL Items Count based on our findings
    private static Dictionary<string, uint> _queueItemsCount = new Dictionary<string, uint>();
    public static uint GetRealQueueCount(string queueName) {
      if( !_queueItemsCount.ContainsKey(queueName) )
        _queueItemsCount.Add(queueName, 0);

      return _queueItemsCount[queueName];
    }
    private static void SetRealQueueCount(string queueName, uint count) {
      _queueItemsCount[queueName] = count;
    }



    public static Model.QueueFetchResult GetUnprocessedMessages(QueueFetchUnprocessedMessagesRequest req, IEnumerable<AzureMessageQueue> monitoringQueues, Func<QueueItem, bool> prepareAddFunc) {
      var result = new QueueFetchResult(QueueFetchResultType.Cumulative);
      result.Status = QueueFetchResultStatus.NotChanged;

      IEnumerable<AzureMessageQueue> queues = monitoringQueues.Where(q => q.Queue.Type == req.Type); 

      if( queues.Count() == 0 ) {
        result.Items = EMPTY_LIST;
        return result;
      }

      List<QueueItem> r = new List<QueueItem>();
      result.Items = r;

      foreach( var q in queues ) {
        //var azureQueue = q.Main;

        //if( IsIgnoredQueue(q.Queue.Name) )
        //  continue;
        
        var queueItemsCount = GetAzureQueueCount(q.Queue.Name);

        try {

          if( q.HasChanged(queueItemsCount) ) {  //q.HasChanged(req.TotalCount) ) {

            if( result.Status == QueueFetchResultStatus.NotChanged )
              result.Status = QueueFetchResultStatus.OK;

            long msgCount = q.GetMessageCount();
            SetAzureQueueCount(q.Queue.Name, (uint)msgCount);

            if( msgCount > 0 ) {
              var msgs = q.Main.PeekBatch(0, SbmqSystem.MAX_ITEMS_PER_QUEUE); // Need to specify 0 as it otherwise just retrieves new ones since last call

              if( msgs.Any() ) {
                long seqNr = 0;
                do {
                  _log.Trace("About to retrieve " + msgs.Count());

                  foreach( var msg in msgs ) {
                    if( seqNr == msg.SequenceNumber)
                      continue;

                    QueueItem itm = req.CurrentItems.FirstOrDefault(i => i.Id == msg.MessageId);

                    if( itm == null && !r.Any(i => i.Id == msg.MessageId) ) {
                      itm = CreateQueueItem(q.Queue, msg);

                      // Load Message names and check if its not an infra-message
                      if( !prepareAddFunc(itm) )
                        itm = null;

                      // Move this up to the Core
                      /*
                      if( q.Queue.Type == QueueType.Error && itm.Error != null && 
                          itm.Error.OriginQueue.IsValid() ) {
                        
                        var i = itm.Error.OriginQueue.IndexOf('@');
                        var errorQueue = ( i > -1 ) ? itm.Error.OriginQueue.Substring(0, i) : itm.Error.OriginQueue;

                        if( !monitoringQueues.Any( mq => string.Compare(mq.Queue.Name, errorQueue, true) == 0 ) )
                          itm = null;
                      }
                      */
                    }

                    if( itm != null )
                      r.Insert(0, itm);

                    seqNr = msg.SequenceNumber;
                  }

                  if( r.Count >= msgCount || seqNr == 0 )
                    break;

                  msgs = q.Main.PeekBatch(seqNr, SbmqSystem.MAX_ITEMS_PER_QUEUE);

                } while( msgs.Count() > 1 );

                //q.LastSequenceNumber = seqNr;
              }

              result.Count = (uint)msgCount; // (uint)r.Count;
              //SetRealQueueCount(q.Queue.Name, result.Count);

            }
          }

        } catch( MessagingCommunicationException mce ) {
          //OnWarning(mce.Message, null, Manager.WarningType.ConnectonFailed);
          result.Status = QueueFetchResultStatus.ConnectionFailed;
          result.StatusMessage = mce.Message;
          break;

        } catch( SocketException se ) {
          //OnWarning(se.Message, null, Manager.WarningType.ConnectonFailed);
          result.Status = QueueFetchResultStatus.ConnectionFailed;
          result.StatusMessage = se.Message;
          break;

        } catch( Exception e ) {
          //OnError("Error occured when processing queue " + q.Queue.Name + ", " + e.Message, e, false);
          result.Status = QueueFetchResultStatus.HasErrors;
          result.StatusMessage = e.Message;
        }

      }

      return result;
    }


    private static QueueItem CreateQueueItem(Queue queue, BrokeredMessage msg) {
      var itm = new QueueItem(queue);
      itm.DisplayName = msg.Label;
      itm.MessageQueueItemId = msg.SequenceNumber;
      itm.Id = msg.SequenceNumber.ToString(); //msg.MessageId;
      itm.ArrivedTime = msg.EnqueuedTimeUtc;
      itm.Content = ReadMessageStream(new System.IO.MemoryStream(msg.GetBody<byte[]>()));
      //itm.Content = ReadMessageStream(msg.BodyStream);

      itm.Headers = new Dictionary<string, string>();
      if( msg.Properties.Count > 0 )
        msg.Properties.ForEach(p => itm.Headers.Add(p.Key, Convert.ToString(p.Value)));

      return itm;
    }

    protected static string ReadMessageStream(Stream s) {
      using( StreamReader r = new StreamReader(s, Encoding.Default) )
        return r.ReadToEnd().Replace("\0", "");
    }

    
  }
}
