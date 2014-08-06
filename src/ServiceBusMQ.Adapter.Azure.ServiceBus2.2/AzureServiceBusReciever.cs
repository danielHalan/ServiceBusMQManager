using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using ServiceBusMQ.Model;

namespace ServiceBusMQ.Adapter.Azure.ServiceBus22 {
  
  public class AzureServiceBusReciever {

    protected static readonly List<QueueItem> EMPTY_LIST = new List<QueueItem>();


    // We Store what 'Azure' think they have, not the actual count, which may differ at times.
    private static Dictionary<string, uint> _queueItemsCount = new Dictionary<string,uint>();
    private static uint GetAzureQueueCount(string queueName) {
      if( !_queueItemsCount.ContainsKey(queueName) )
        _queueItemsCount.Add(queueName, 0);

      return _queueItemsCount[queueName];
    }
    private static void SetAzureQueueCount(string queueName, uint count) {
      _queueItemsCount[queueName] = count;
    }

    public static Model.QueueFetchResult GetUnprocessedMessages(QueueFetchUnprocessedMessagesRequest req, IEnumerable<AzureMessageQueue> queues, Func<QueueItem, bool> prepareAddFunc) {
      var result = new QueueFetchResult();
      result.Status = QueueFetchResultStatus.NotChanged;

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
              var msgs = q.Main.PeekBatch(SbmqSystem.MAX_ITEMS_PER_QUEUE);
              result.Count += (uint)msgs.Count(); // msgCount

              foreach( var msg in msgs ) {

                QueueItem itm = req.CurrentItems.FirstOrDefault(i => i.Id == msg.MessageId);

                if( itm == null && !r.Any(i => i.Id == msg.MessageId) ) {
                  itm = CreateQueueItem(q.Queue, msg);

                  // Load Message names and check if its not an infra-message
                  if( !prepareAddFunc(itm) )
                    itm = null;
                }

                if( itm != null )
                  r.Insert(0, itm);

              }
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
        msg.Properties.ForEach(p => itm.Headers.Add(p.Key, p.Value.ToString()));

      return itm;
    }

    protected static string ReadMessageStream(Stream s) {
      using( StreamReader r = new StreamReader(s, Encoding.Default) )
        return r.ReadToEnd().Replace("\0", "");
    }

    
  }
}
