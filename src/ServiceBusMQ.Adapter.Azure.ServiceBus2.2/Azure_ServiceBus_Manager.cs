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
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using ServiceBusMQ.Manager;
using ServiceBusMQ.Model;

namespace ServiceBusMQ.Adapter.Azure.ServiceBus22 {

  public class Azure_ServiceBus_Manager : IServiceBusManager {

    static readonly string JSON_START = "\"$type\":\"";
    static readonly string JSON_END = ",";

    protected List<QueueItem> EMPTY_LIST = new List<QueueItem>();
    private bool _terminated;

    public string ServiceBusName { get { return "Windows Azure"; } }
    public string ServiceBusVersion { get { return "2.2"; } }
    public string MessageQueueType { get { return "Service Bus"; } }

    static readonly string CS_CONNECTION_STRING = "connectionStr";
    static readonly string CS_MAX_MESSAGES = "msgLimit";

    public string CommandContentFormat { get; set; }


    public Queue[] MonitorQueues { get; private set; }

    protected Dictionary<string, object> _connectionSettings;

    private int MessageCountLimit {
      get {
        int r = 0;
        if( _connectionSettings.ContainsKey(CS_MAX_MESSAGES) && Int32.TryParse(_connectionSettings[CS_MAX_MESSAGES] as string, out r) )
          return r;
        else return SbmqSystem.MAX_ITEMS_PER_QUEUE;
      }
    }
    private string ConnectionString {
      get { return _connectionSettings[CS_CONNECTION_STRING] as string; }
    }

    protected SbmqmMonitorState _monitorState;
    protected CommandDefinition _commandDef;

    protected List<AzureMessageQueue> _monitorQueues = new List<AzureMessageQueue>();

    public bool MessagesHasMilliSecondPrecision { get { return false; } }

    public string[] AvailableMessageContentTypes {
      get { return new string[] { "XML", "JSON" }; }
    }


    public void Initialize(Dictionary<string, object> connectionSettings, Queue[] monitorQueues, SbmqmMonitorState monitorState) {
      _connectionSettings = connectionSettings;

      MonitorQueues = monitorQueues;
      _monitorState = monitorState;

      LoadQueues();

    }


    public void Terminate() {
      _terminated = true;
    }

    private void LoadQueues() {
      _monitorQueues.Clear();

      foreach( var queue in MonitorQueues )
        AddAzureQueue(ConnectionString, queue);

      // Add Dead Letter (Error) Queues for all Normal Queues
      foreach( var queue in MonitorQueues.Where(q => q.Type != QueueType.Error) ) {
        if( !_monitorQueues.Any(q => q.Queue.Name == queue.Name && q.IsDeadLetterQueue) ) {
          AddAzureQueue(ConnectionString, queue, true);
        }
      }

    }

    private void AddAzureQueue(string connectionStr, Model.Queue queue, bool deadLetterQueue = false) {
      try {

        //var mgr = NamespaceManager.CreateFromConnectionString(_serverName);
        //var client = QueueClient.CreateFromConnectionString(_serverName, queue);
        _monitorQueues.Add(new AzureMessageQueue(connectionStr, queue, deadLetterQueue));

      } catch( Exception e ) {
        OnError("Error occured when loading queue: '{0}\\{1}'\n\r".With(connectionStr, queue.Name), e, false);
      }
    }

    public MessageSubscription[] GetMessageSubscriptions(Dictionary<string, object> connectionSettings, IEnumerable<string> queues) {
      return new MessageSubscription[0];
    }


    private AzureMessageQueue GetMessageQueue(QueueItem itm) {
      return _monitorQueues.Single(i => i.Queue.Type == itm.Queue.Type && i.Queue.Name == itm.Queue.Name);
    }

    public string LoadMessageContent(Model.QueueItem itm) {
      ///AzureMessageQueue q = GetMessageQueue(itm);
      return "Not Implemented";
      //q.Main.Peek(
    }
    protected string ReadMessageStream(Stream s) {
      using( StreamReader r = new StreamReader(s, Encoding.Default) )
        return r.ReadToEnd().Replace("\0", "");
    }
    protected MessageInfo[] GetMessageNames(string content, bool includeNamespace) {

      if( content.StartsWith("<?xml version=\"1.0\"") )
        return GetXmlMessageNames(content, includeNamespace);
      else return GetJsonMessageNames(content, includeNamespace);

    }
    private MessageInfo[] GetJsonMessageNames(string content, bool includeNamespace) {
      List<MessageInfo> r = new List<MessageInfo>();
      try {
        foreach( var msg in GetAllRootCurlyBrackers(content) ) {

          int iStart = msg.IndexOf(JSON_START) + JSON_START.Length;
          int iEnd = msg.IndexOf(JSON_END, iStart);

          if( !includeNamespace ) {
            iStart = msg.LastIndexOf(".", iEnd) + 1;
          }

          r.Add(new MessageInfo(msg.Substring(iStart, iEnd - iStart)));
        }
      } catch { }

      return r.ToArray();
    }
    private MessageInfo[] GetXmlMessageNames(string content, bool includeNamespace) {
      List<MessageInfo> r = new List<MessageInfo>();
      try {
        XDocument doc = XDocument.Parse(content);
        string ns = string.Empty;

        if( includeNamespace ) {
          ns = doc.Root.Attribute("xmlns").Value.Remove(0, 19) + ".";
        }

        foreach( XElement e in doc.Root.Elements() ) {
          r.Add(new MessageInfo(ns + e.Name.LocalName));
        }

      } catch { }

      return r.ToArray();
    }

    protected MessageInfo[] ExtractEnclosedMessageTypeNames(string content, bool includeNamespace = false) {
      string[] types = content.Split(';');
      List<MessageInfo> r = new List<MessageInfo>(types.Length);

      foreach( string type in types ) {

        int start = 0;
        int end = type.IndexOf(',', start);

        if( !includeNamespace ) {
          start = type.LastIndexOf('.', end) + 1;
        }
        r.Add(new MessageInfo(type.Substring(start, end - start), type));
      }

      return r.ToArray();
    }

    private IEnumerable<string> GetAllRootCurlyBrackers(string content) {
      int start = -1;
      int stack = 0;
      List<string> r = new List<string>();

      int i = 0;
      do {
        if( content[i] == '{' ) {
          if( stack == 0 )
            start = i;

          stack++;
        }

        if( content[i] == '}' ) {
          stack--;

          if( stack == 0 ) {
            r.Add(content.Substring(start, i - start));
          }
        }

      } while( ++i < content.Length );

      return r;
    }
    protected string MergeStringArray(MessageInfo[] arr) {
      StringBuilder sb = new StringBuilder();
      foreach( var msg in arr ) {
        if( sb.Length > 0 ) sb.Append(", ");

        sb.Append(msg.Name);
      }

      return sb.ToString();
    }


    public string SerializeCommand(object cmd) {
      try {
        return MessageSerializer.SerializeMessage(cmd, CommandContentFormat);

      } catch( Exception e ) {
        OnError("Failed to Serialize Command to " + CommandContentFormat, e);
        return null;
      }
    }
    public object DeserializeCommand(string cmd, Type cmdType) {
      try {
        return MessageSerializer.DeserializeMessage(cmd, cmdType, CommandContentFormat);

      } catch( Exception e ) {
        OnError("Failed to Parse Command string as " + CommandContentFormat, e);
        return null;
      }
    }



    public Model.QueueFetchResult GetUnprocessedMessages(QueueFetchUnprocessedMessagesRequest req) {
      return AzureServiceBusReciever.GetUnprocessedMessages(req, _monitorQueues.Where(q => q.Queue.Type == req.Type), x => PrepareQueueItemForAdd(x));

      /*
      var result = new QueueFetchResult();
      result.Status = QueueFetchResultStatus.NotChanged;

      IEnumerable<AzureMessageQueue> queues = req.Type != QueueType.Error ? _monitorQueues.Where(q => q.Queue.Type == req.Type) : _monitorQueues.Where( q => q.IsDeadLetterQueue || q.Queue.Type == QueueType.Error );

      if( queues.Count() == 0 ) {
        result.Items = EMPTY_LIST;
        return result;
      }

      List<QueueItem> r = new List<QueueItem>();
      result.Items = r;

      foreach( var q in queues ) {
        var azureQueue = q.Main;

        //if( IsIgnoredQueue(q.Queue.Name) )
        //  continue;

        try {

          if( q.HasChanged(req.TotalCount) ) {

            if( result.Status == QueueFetchResultStatus.NotChanged )
              result.Status = QueueFetchResultStatus.OK;

            long msgCount = q.GetMessageCount();

            if( msgCount > 0 ) {
              var msgs = q.Main.PeekBatch(0, MessageCountLimit);
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
      */
    }

    /// <summary>
    /// Called when we know that we actually shall add the item, and here we can execute processes that takes extra time
    /// </summary>
    /// <param name="itm"></param>
    /// <returns></returns>
    private bool PrepareQueueItemForAdd(QueueItem itm) {

      // Ignore control messages
      //if( itm.Headers.ContainsKey(Headers.ControlMessageHeader) && Convert.ToBoolean(itm.Headers[Headers.ControlMessageHeader]) )
      //  return false;

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
      itm.Error = null; //new QueueItemError();

      return true;
    }

    private QueueItem CreateQueueItem(Queue queue, BrokeredMessage msg) {
      var itm = new QueueItem(queue);
      itm.DisplayName = msg.Label;
      itm.MessageQueueItemId = msg.SequenceNumber;
      itm.Id = msg.SequenceNumber.ToString(); //msg.MessageId;
      itm.ArrivedTime = msg.EnqueuedTimeUtc;
      try {
        itm.Content = ReadMessageStream(new System.IO.MemoryStream(msg.GetBody<byte[]>()));

      } catch( SerializationException ex ) {
        itm.Content = "** Failed to get message content, {0} ** \n\n{1}".With(ex.Message, ex.StackTrace);
        //itm.Error = new QueueItemError() { 
        //  Message = ex.Message,
        //  StackTrace = ex.StackTrace,
        //  State = QueueItemErrorState.Retry
        //};
      }
      //itm.Content = ReadMessageStream(msg.BodyStream);

      itm.Headers = new Dictionary<string, string>();
      if( msg.Properties.Count > 0 )
        msg.Properties.ForEach(p => itm.Headers.Add(p.Key, p.Value.ToString()));

      return itm;
    }




    public QueueFetchResult GetProcessedMessages(Model.QueueType type, DateTime since, IEnumerable<Model.QueueItem> currentItems) {
      var r = new QueueFetchResult();

      r.Count = 0;
      return r;
    }

    public void PurgeMessage(Model.QueueItem itm) {
      throw new NotImplementedException();
      //QueueClient q = GetMessageQueue(itm);

      //if( q != null ) {

      //  var msg = q.Receive((long)itm.MessageQueueItemId);
      //  //if( msg != null )
      //  //  msg.Complete();

      //  itm.Processed = true;

      //  OnItemsChanged();
      //}
    }
    public void PurgeAllMessages() {
      List<Task> tasks = new List<Task>();

      for( int i = 0; i < _monitorQueues.Count; i++ ) {

        tasks.Add(Task.Factory.StartNew(() => _monitorQueues[i].Purge()));
        Thread.Sleep(2000);

        if( ( ( i + 1 ) % 3 ) == 0 ) {
          Task.WaitAll(tasks.ToArray());
          tasks.Clear();
        }
      }

      if( tasks.Count > 0 )
        Task.WaitAll(tasks.ToArray());

      OnItemsChanged();
    }

    public void PurgeErrorMessages(string queueName) {
      _monitorQueues.Where(q => q.Queue.Type == QueueType.Error && q.Queue.Name == queueName).Single().Purge();

      OnItemsChanged();
    }
    public void PurgeErrorAllMessages() {
      var items = _monitorQueues.Where(q => q.Queue.Type == QueueType.Error);

      if( items.Count() > 0 ) {
        items.ForEach(q => q.Purge());

        OnItemsChanged();
      }
    }


    public void MoveErrorMessageToOriginQueue(QueueItem itm) {
      if( string.IsNullOrEmpty(itm.Id) )
        throw new ArgumentException("MessageId can not be null or empty");

      if( itm.Queue.Type != QueueType.Error )
        throw new ArgumentException("Queue is not of type Error, " + itm.Queue.Type);

      var mgr = new ErrorManager(ConnectionString);

      mgr.ReturnMessageToSourceQueue(itm.Queue.Name, itm);
    }

    public async Task MoveAllErrorMessagesToOriginQueue(string errorQueue) {
      var mgr = new ErrorManager(ConnectionString);

      if( errorQueue.IsValid() )
        mgr.ReturnAll(errorQueue);
      else {

        foreach( var queue in _monitorQueues.Where(q => q.Queue.Type == QueueType.Error) ) {
          mgr.ReturnAll(queue.Queue.Name);
        }
      }
    }



    /* EVENTS */

    public event EventHandler<ErrorArgs> ErrorOccured;
    public event EventHandler<WarningArgs> WarningOccured;  

    protected void OnError(string message, Exception exception = null, bool fatal = false) {
      if( ErrorOccured != null )
        ErrorOccured(this, new ErrorArgs(message, exception, fatal));
    }
    protected void OnWarning(string message, string content, WarningType type = WarningType.Other) {
      if( WarningOccured != null )
        WarningOccured(this, new WarningArgs(message, content, type));
    }


    protected EventHandler _itemsChanged;
    public event EventHandler ItemsChanged {
      [MethodImpl(MethodImplOptions.Synchronized)]
      add {
        _itemsChanged = (EventHandler)Delegate.Combine(_itemsChanged, value);
      }
      [MethodImpl(MethodImplOptions.Synchronized)]
      remove {
        _itemsChanged = (EventHandler)Delegate.Remove(_itemsChanged, value);
      }
    }

    protected void OnItemsChanged() {
      if( _itemsChanged != null )
        _itemsChanged(this, EventArgs.Empty);
    }





  }
}
