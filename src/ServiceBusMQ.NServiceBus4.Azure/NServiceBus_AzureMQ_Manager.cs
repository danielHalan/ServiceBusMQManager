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
using System.Text;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NServiceBus;
using ServiceBusMQ.Model;

namespace ServiceBusMQ.NServiceBus.Azure {
  public class NServiceBus_AzureMQ_Manager : NServiceBusManagerBase<AzureMessageQueue> {


    protected List<QueueItem> EMPTY_LIST = new List<QueueItem>();
    private bool _terminated;

    public override string MessageQueueType { get { return "AzureMQ"; } }



    public override void Initialize(Dictionary<string, string> connectionSettings, Queue[] monitorQueues, SbmqmMonitorState monitorState) {
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
        AddAzureQueue(_connectionSettings["connectionStr"], queue);
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

    public override MessageSubscription[] GetMessageSubscriptions(string server) {
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



    // Possibly Merge with MSMQ NSB Class

    string SerializeCommand_XML(object cmd) {
      var types = new List<Type> { cmd.GetType() };

      var mapper = new global::NServiceBus.MessageInterfaces.MessageMapper.Reflection.MessageMapper();
      mapper.Initialize(types);

      var serializr = new global::NServiceBus.Serializers.XML.XmlMessageSerializer(mapper);
      serializr.Initialize(types);

      using( Stream stream = new MemoryStream() ) {
        serializr.Serialize(new[] { cmd }, stream);
        stream.Position = 0;

        return new StreamReader(stream).ReadToEnd();
      }

    }
    public object DeserializeCommand_XML(string cmd, Type cmdType) {
      try {
        var types = new List<Type> { cmdType };

        var mapper = new global::NServiceBus.MessageInterfaces.MessageMapper.Reflection.MessageMapper();
        mapper.Initialize(types);

        var serializr = new global::NServiceBus.Serializers.XML.XmlMessageSerializer(mapper);
        serializr.Initialize(types);

        using( Stream stream = new MemoryStream(Encoding.Unicode.GetBytes(cmd)) ) {
          var obj = serializr.Deserialize(stream);

          return obj[0];
        }
      } catch( Exception e ) {
        OnError("Failed to parse command string as XML", e);
        return null;
      }

    }
    public string SerializeCommand_JSON(object cmd) {

      var types = new List<Type> { cmd.GetType() };

      var mapper = new global::NServiceBus.MessageInterfaces.MessageMapper.Reflection.MessageMapper();
      mapper.Initialize(types);

      var serializr = new global::NServiceBus.Serializers.Json.JsonMessageSerializer(mapper);

      using( Stream stream = new MemoryStream() ) {
        serializr.Serialize(new[] { cmd }, stream);
        stream.Position = 0;

        return new StreamReader(stream).ReadToEnd();
      }

    }
    public object DeserializeCommand_JSON(string cmd, Type cmdType) {
      var types = new List<Type> { cmd.GetType() };

      var mapper = new global::NServiceBus.MessageInterfaces.MessageMapper.Reflection.MessageMapper();
      mapper.Initialize(types);

      var serializr = new global::NServiceBus.Serializers.Json.JsonMessageSerializer(mapper);

      using( Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(cmd)) ) {
        var obj = serializr.Deserialize(stream);

        return obj[0];
      }

    }


    public override string SerializeCommand(object cmd) {

      if( CommandContentFormat == "XML" )
        return SerializeCommand_XML(cmd);

      else if( CommandContentFormat == "JSON" )
        return SerializeCommand_JSON(cmd);

      else throw new Exception("Unknown Command Content Format, " + CommandContentFormat);

    }
    public override object DeserializeCommand(string cmd, Type cmdType) {

      if( CommandContentFormat == "XML" )
        return DeserializeCommand_XML(cmd, cmdType);

      else if( CommandContentFormat == "JSON" )
        return DeserializeCommand_JSON(cmd, cmdType);

      else throw new Exception("Unknown Command Content Format, " + CommandContentFormat);
    }


    // END

    
    
    public override Model.QueueFetchResult GetUnprocessedMessages(QueueType type, IEnumerable<QueueItem> currentItems) {
      var result = new QueueFetchResult();
      var queues = _monitorQueues.Where(q => q.Queue.Type == type);

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

        //SetupMessageReadPropertyFilters(q.Main, q.Queue.Type);

        // Add peaked items
        /*
        lock( _peekItemsLock ) {
          if( _peekedItems.Count > 0 ) {

            r.AddRange(_peekedItems);
            _peekedItems.Clear();
          }
        } 
         */

        try {
          var msgs = q.Main.PeekBatch(SbmqSystem.MAX_ITEMS_PER_QUEUE);
          result.Count = (uint)msgs.Count();

          foreach( var msg in msgs ) {

            QueueItem itm = currentItems.FirstOrDefault(i => i.Id == msg.MessageId);

            if( itm == null && !r.Any(i => i.Id == msg.MessageId) ) {
              itm = CreateQueueItem(q.Queue, msg);

              // Load Message names and check if its not an infra-message
              if( !PrepareQueueItemForAdd(itm) )
                itm = null;
            }

            if( itm != null )
              r.Insert(0, itm);

          }

        } catch( Exception e ) {
          OnError("Error occured when processing queue " + q.Queue.Name + ", " + e.Message, e, false);
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

        } catch (Exception ex) {
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
      itm.Id = msg.MessageId;
      itm.ArrivedTime = msg.EnqueuedTimeUtc;
      itm.Content = ReadMessageStream( new System.IO.MemoryStream(msg.GetBody<byte[]>()) );
      //itm.Content = ReadMessageStream(msg.BodyStream);

      itm.Headers = new Dictionary<string, string>();
      if( msg.Properties.Count > 0 ) 
        msg.Properties.ForEach( p => itm.Headers.Add(p.Key, p.Value.ToString()) );

      return itm;
    }



    public override QueueFetchResult GetProcessedMessages(Model.QueueType type, DateTime since, IEnumerable<Model.QueueItem> currentItems) {
      var r = new QueueFetchResult();

      r.Count = 0;
      return r;
    }

    public override void PurgeMessage(Model.QueueItem itm) {
      throw new NotImplementedException();
    }

    public override void PurgeAllMessages() {
      throw new NotImplementedException();
    }

    public override void PurgeErrorMessages(string queueName) {
      throw new NotImplementedException();
    }

    public override void PurgeErrorAllMessages() {
      throw new NotImplementedException();
    }


    public override void MoveErrorMessageToOriginQueue(QueueItem itm) { 
    
    }
    
    public override void MoveAllErrorMessagesToOriginQueue(string errorQueue) { 
    }

  }
}
