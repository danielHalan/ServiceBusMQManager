#region File Information
/********************************************************************
  Project: ServiceBusMQ.NServiceBus
  File:    NServiceBus_AzureMQ_Manager.cs
  Created: 2013-10-11

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
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NServiceBus;
using ServiceBusMQ.Model;
using ServiceBusMQ.NServiceBus;

namespace ServiceBusMQ.Adapter.NServiceBus4.Azure.SB22 {
  public class NServiceBus_AzureSB_Manager : NServiceBusManagerBase<AzureMessageQueue> {

    protected List<QueueItem> EMPTY_LIST = new List<QueueItem>();
    private bool _terminated;

    public override string ServiceBusName { get { return "NServiceBus"; } }
    public override string ServiceBusVersion { get { return "4"; } }
    public override string MessageQueueType { get { return "Azure Service Bus"; } }

    static readonly string CS_SERVER = "server";
    static readonly string CS_CONNECTION_STRING = "connectionStr";


    public override void Initialize(Dictionary<string, object> connectionSettings, Queue[] monitorQueues, SbmqmMonitorState monitorState) {
      base.Initialize(connectionSettings, monitorQueues, monitorState);

      LoadQueues();

      //StartPeekThreads();
    }


    public override void Terminate() {
      _terminated = true;
    }

    private void LoadQueues() {
      _monitorQueues.Clear();

      foreach( var queue in MonitorQueues )
        AddAzureQueue(_connectionSettings[CS_CONNECTION_STRING] as string, queue);
    }

    private void AddAzureQueue(string _connectionStr, Model.Queue queue) {
      try {

        //var mgr = NamespaceManager.CreateFromConnectionString(_serverName);
        //var client = QueueClient.CreateFromConnectionString(_serverName, queue);
        _monitorQueues.Add(new AzureMessageQueue(_connectionStr, queue));
      } catch( Exception e ) {
        OnError("Error occured when loading queue: '{0}\\{1}'\n\r".With(_connectionStr, queue.Name), e, false);
      }
    }

    public override MessageSubscription[] GetMessageSubscriptions(Dictionary<string, object> connectionSettings, IEnumerable<string> queues) {
      return new MessageSubscription[0];
    }


    private AzureMessageQueue GetMessageQueue(QueueItem itm) {
      return _monitorQueues.Single(i => i.Queue.Type == itm.Queue.Type && i.Queue.Name == itm.Queue.Name);
    }

    public override string LoadMessageContent(Model.QueueItem itm) {
      ///AzureMessageQueue q = GetMessageQueue(itm);
      return "Not Implemented";
      //q.Main.Peek(
    }


    public override string SerializeCommand(object cmd) {
      try {
        return MessageSerializer.SerializeMessage(cmd, CommandContentFormat);

      } catch( Exception e ) {
        OnError("Failed to Serialize Command to " + CommandContentFormat, e);
        return null;
      }
    }
    public override object DeserializeCommand(string cmd, Type cmdType) {
      try {
        return MessageSerializer.DeserializeMessage(cmd, cmdType, CommandContentFormat);

      } catch( Exception e ) {
        OnError("Failed to Parse Command string as " + CommandContentFormat, e);
        return null;
      }
    }



    public override Model.QueueFetchResult GetUnprocessedMessages(QueueFetchUnprocessedMessagesRequest req) {
      var result = new QueueFetchResult();
      result.Status = QueueFetchResultStatus.NotChanged;

      var queues = _monitorQueues.Where(q => q.Queue.Type == req.Type);

      if( queues.Count() == 0 ) {
        result.Items = EMPTY_LIST;
        return result;
      }

      List<QueueItem> r = new List<QueueItem>();
      result.Items = r;

      foreach( var q in queues ) {
        var azureQueue = q.Main;

        if( IsIgnoredQueue(q.Queue.Name) )
          continue;

        try {

          if( q.HasChanged(req.TotalCount) ) {
            
            if( result.Status == QueueFetchResultStatus.NotChanged )
              result.Status = QueueFetchResultStatus.OK;

            long msgCount = q.GetMessageCount();

            if( msgCount > 0 ) {
              var msgs = q.Main.PeekBatch(0, SbmqSystem.MAX_ITEMS_PER_QUEUE);
              result.Count += (uint)msgCount;

              foreach( var msg in msgs ) {

                QueueItem itm = req.CurrentItems.FirstOrDefault(i => i.Id == msg.MessageId);

                if( itm == null && !r.Any(i => i.Id == msg.MessageId) ) {
                  itm = CreateQueueItem(q.Queue, msg);

                  // Load Message names and check if its not an infra-message
                  if( !PrepareQueueItemForAdd(itm) )
                    itm = null;
                }

                if( itm != null )
                  r.Insert(0, itm);

              }
            }
          }

        } catch( MessagingCommunicationException mce ) {
          OnWarning(mce.Message, null, Manager.WarningType.ConnectonFailed);
          result.Status = QueueFetchResultStatus.ConnectionFailed;
          break;

        } catch( SocketException se ) {
          OnWarning(se.Message, null, Manager.WarningType.ConnectonFailed);
          result.Status = QueueFetchResultStatus.ConnectionFailed;
          break;

        } catch( Exception e ) {
          OnError("Error occured when processing queue " + q.Queue.Name + ", " + e.Message, e, false);
          result.Status = QueueFetchResultStatus.HasErrors;
        }

      }

      return result;
    }

    /// <summary>
    /// Called when we know that we actually shall add the item, and here we can execute processes that takes extra time
    /// </summary>
    /// <param name="itm"></param>
    /// <returns></returns>
    private bool PrepareQueueItemForAdd(QueueItem itm) {

      // Ignore control messages
      if( itm.Headers.ContainsKey(Headers.ControlMessageHeader) && Convert.ToBoolean(itm.Headers[Headers.ControlMessageHeader]) )
        return false;

      if( itm.Headers.ContainsKey("NServiceBus.MessageId") )
        itm.Id = itm.Headers["NServiceBus.MessageId"];

      // Get Messages names
      if( itm.Headers.ContainsKey("NServiceBus.EnclosedMessageTypes") ) {
        itm.Messages = ExtractEnclosedMessageTypeNames(itm.Headers["NServiceBus.EnclosedMessageTypes"]);

      } else { // Get from Message body
        if( itm.Content == null )
          LoadMessageContent(itm);

        itm.Messages = GetMessageNames(itm.Content, false);
      }
      itm.DisplayName = MergeStringArray(itm.Messages).Default(itm.DisplayName).CutEnd(55);

      // Get process started time
      if( itm.Headers.ContainsKey("NServiceBus.ProcessingStarted") && itm.Headers.ContainsKey("NServiceBus.ProcessingEnded") ) {

        try {
          itm.ProcessTime = Convert.ToInt32(( Convert.ToDateTime(itm.Headers["NServiceBus.ProcessingEnded"]) -
                            Convert.ToDateTime(itm.Headers["NServiceBus.ProcessingStarted"]) ).TotalSeconds);

        } catch( Exception ex ) {
#if DEBUG
          Console.WriteLine("Failed to parse NServiceBus.ProcessingStarted, " + ex.Message);
#endif
        }

      }

      // Get Error message info
      if( itm.Headers.ContainsKey("NServiceBus.ExceptionInfo.Message") ) {

        itm.Error = new QueueItemError();
        try {
          itm.Error.State = itm.Queue.Type == QueueType.Error ? QueueItemErrorState.ErrorQueue : QueueItemErrorState.Retry;
          itm.Error.Message = itm.Headers["NServiceBus.ExceptionInfo.Message"];

          if( itm.Headers.ContainsKey("NServiceBus.ExceptionInfo.StackTrace") )
            itm.Error.StackTrace = itm.Headers["NServiceBus.ExceptionInfo.StackTrace"];

          if( itm.Headers.ContainsKey(Headers.Retries) )
            itm.Error.Retries = Convert.ToInt32(itm.Headers[Headers.Retries]);

          //itm.Error.TimeOfFailure = Convert.ToDateTime(itm.Headers.SingleOrDefault(k => k.Key == "NServiceBus.TimeOfFailure").Value);
        } catch {
          itm.Error = null;
        }
      }



      return true;
    }

    private QueueItem CreateQueueItem(Queue queue, BrokeredMessage msg) {
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



    public override QueueFetchResult GetProcessedMessages(Model.QueueType type, DateTime since, IEnumerable<Model.QueueItem> currentItems) {
      var r = new QueueFetchResult();

      r.Count = 0;
      return r;
    }

    public override void PurgeMessage(Model.QueueItem itm) {
      throw new NotImplementedException();

      //QueueClient q = GetMessageQueue(itm);

      //if( q != null ) {

      //  long msgId = (long)itm.MessageQueueItemId;

      //  var msg = q.Peek(msgId);
      //  msg.Defer();

      //  msg = q.Receive(msgId);
      //  if( msg != null )
      //    msg.Complete();

      //  itm.Processed = true;

      //  OnItemsChanged();
      //}
    }
    public override void PurgeAllMessages() {
      List<Task> tasks = new List<Task>();

      for(int i = 0; i < _monitorQueues.Count; i++) {
      
        tasks.Add(Task.Factory.StartNew(() => _monitorQueues[i].Purge()));
        Thread.Sleep(2000);

        if( ( (i+1) % 3 ) == 0 ) {

          Task.WaitAll(tasks.ToArray());
          tasks.Clear();
        }
      }
      
      if( tasks.Count > 0 )
        Task.WaitAll(tasks.ToArray());

      OnItemsChanged();
    }

    public override void PurgeErrorMessages(string queueName) {
      _monitorQueues.Where(q => q.Queue.Type == QueueType.Error && q.Queue.Name == queueName).Single().Purge();

      OnItemsChanged();
    }
    public override void PurgeErrorAllMessages() {
      var items = _monitorQueues.Where(q => q.Queue.Type == QueueType.Error);

      if( items.Count() > 0 ) {
        items.ForEach(q => q.Purge());

        OnItemsChanged();
      }
    }


    public override void MoveErrorMessageToOriginQueue(QueueItem itm) {
      if( string.IsNullOrEmpty(itm.Id) )
        throw new ArgumentException("MessageId can not be null or empty");

      if( itm.Queue.Type != QueueType.Error )
        throw new ArgumentException("Queue is not of type Error, " + itm.Queue.Type);

      var mgr = new ErrorManager(_connectionSettings[CS_CONNECTION_STRING] as string);

      // TODO:
      // Check if Clustered Queue, due if Clustered && NonTransactional, then Error

      mgr.ReturnMessageToSourceQueue(itm.Queue.Name, itm);
    }

    public override void MoveAllErrorMessagesToOriginQueue(string errorQueue) {
      var mgr = new ErrorManager(_connectionSettings[CS_CONNECTION_STRING] as string);

      mgr.ReturnAll(errorQueue);
    }

  }
}
