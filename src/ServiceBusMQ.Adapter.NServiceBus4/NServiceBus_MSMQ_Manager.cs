#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    NServiceBusMSMQManager.cs
  Created: 2012-09-23

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using NLog;
using NServiceBus;
using NServiceBus.Persistence.Raven.SubscriptionStorage;
using NServiceBus.Tools.Management.Errors.ReturnToSourceQueue;
using NServiceBus.Transports.Msmq;
using Raven.Client.Document;
//using NServiceBus;
//using NServiceBus.Utils;
using ServiceBusMQ.Manager;
using ServiceBusMQ.Model;
using ServiceBusMQ.NServiceBus;


namespace ServiceBusMQ.NServiceBus4 {

  //[PermissionSetAttribute(SecurityAction.LinkDemand, Name = "FullTrust")]
  public class NServiceBus_MSMQ_Manager : NServiceBusManagerBase<MsmqMessageQueue>, ISendCommand, IViewSubscriptions {

    protected Logger _log = LogManager.GetCurrentClassLogger();

    protected List<QueueItem> EMPTY_LIST = new List<QueueItem>();


    public override string ServiceBusName { get { return "NServiceBus"; } }
    public override string ServiceBusVersion { get { return "4"; } }
    public override string MessageQueueType { get { return "MSMQ"; } }

    public static readonly string CS_SERVER = "server";
    public static readonly string CS_RAVEN_PERSISTANCE = "ravenPersistence";


    class PeekThreadParam {
      public Queue Queue { get; set; }
      public MessageQueue MsmqQueue { get; set; }
    }

    bool _terminated = false;

    public NServiceBus_MSMQ_Manager() {
    }

    public override void Initialize(Dictionary<string, string> connectionSettings, Queue[] monitorQueues, SbmqmMonitorState monitorState) {
      base.Initialize(connectionSettings, monitorQueues, monitorState);

      LoadQueues();

      StartPeekThreads();
    }
    public override void Terminate() {
      _terminated = true;
    }


    void StartPeekThreads() {
      foreach( QueueType qt in Enum.GetValues(typeof(QueueType)) ) {

        if( qt != QueueType.Error ) {
          foreach( var q in GetQueueListByType(qt) ) {
            var t = new Thread(new ParameterizedThreadStart(PeekMessages));
            if( q.Main.CanRead ) {
              t.Name = "peek-msmq-" + q.GetDisplayName();
              t.Start(new PeekThreadParam() { MsmqQueue = q.Main, Queue = q.Queue });
            }
          }


        }
      }
    }

    object _peekItemsLock = new object();
    List<QueueItem> _peekedItems = new List<QueueItem>();


    public void PeekMessages(object prm) {
      PeekThreadParam p = prm as PeekThreadParam;
      string qName = p.MsmqQueue.GetDisplayName();
      uint sameCount = 0;
      string lastId = string.Empty;

      bool _isPeeking = false;

      SetupMessageReadPropertyFilters(p.MsmqQueue, p.Queue.Type);

      p.MsmqQueue.PeekCompleted += (source, asyncResult) => {
        if( _monitorState.IsMonitoringQueueType(p.Queue.Type) ) {
          Message msg = p.MsmqQueue.EndPeek(asyncResult.AsyncResult);

          if( msg.Id == lastId )
            sameCount++;

          else {
            sameCount = 0;
            TryAddItem(msg, p.Queue);
          }

          if( lastId != msg.Id )
            lastId = msg.Id;

        }
        _isPeeking = false;
      };

      while( !_terminated ) {

        while( !_monitorState.IsMonitoringQueueType(p.Queue.Type) ) {
          Thread.Sleep(1000);

          if( _terminated )
            return;
        }


        if( !_isPeeking ) {

          if( sameCount > 0 ) {
            if( sameCount / 10.0F == 1.0F )
              Thread.Sleep(100);

            else if( sameCount / 100.0F == 1.0F )
              Thread.Sleep(200);

            else if( sameCount % 300 == 0 )
              Thread.Sleep(500);
          }
          p.MsmqQueue.BeginPeek();
          _isPeeking = true;
        }

        Thread.Sleep(100);
      }


    }

    private bool TryAddItem(Message msg, Queue q) {

      lock( _peekItemsLock ) {

        if( !_peekedItems.Any(i => i.Id == msg.Id) ) {

          var itm = CreateQueueItem(q, msg);

          if( PrepareQueueItemForAdd(itm) )
            _peekedItems.Add(itm);


          return true;

        } else return false;
      }

    }



    private void LoadQueues() {
      _monitorQueues.Clear();

      foreach( var queue in MonitorQueues )
        AddMsmqQueue(_connectionSettings["server"], queue);

    }
    private void AddMsmqQueue(string serverName, Queue queue) {
      try {
        _monitorQueues.Add(new MsmqMessageQueue(serverName, queue));
      } catch( Exception e ) {
        OnError("Error occured when loading queue: '{0}\\{1}'\n\r".With(serverName, queue.Name), e, false);
      }
    }


    private void SetupMessageReadPropertyFilters(MessageQueue q, QueueType type) {

      q.MessageReadPropertyFilter.Id = true;
      q.MessageReadPropertyFilter.ArrivedTime = true;
      q.MessageReadPropertyFilter.Label = true;
      q.MessageReadPropertyFilter.Body = false;

      //if( type == QueueType.Error )
      q.MessageReadPropertyFilter.Extension = true;
    }


    public override QueueFetchResult GetUnprocessedMessages(QueueType type, IEnumerable<QueueItem> currentItems) {
      var result = new QueueFetchResult();
      var queues = _monitorQueues.Where(q => q.Queue.Type == type);

      if( queues.Count() == 0 ) {
        result.Items = EMPTY_LIST;
        return result;
      }

      List<QueueItem> r = new List<QueueItem>();
      result.Items = r;

      foreach( var q in queues ) {
        var msmqQueue = q.Main;

        if( IsIgnoredQueue(q.Queue.Name) || !q.Main.CanRead )
          continue;

        SetupMessageReadPropertyFilters(q.Main, q.Queue.Type);

        // Add peaked items
        lock( _peekItemsLock ) {
          if( _peekedItems.Count > 0 ) {

            r.AddRange(_peekedItems);
            _peekedItems.Clear();
          }
        }

        try {
          var msgs = q.Main.GetAllMessages();
          result.Count += (uint)msgs.Length;

          foreach( var msg in msgs ) {

            QueueItem itm = currentItems.FirstOrDefault(i => i.Id == msg.Id);

            if( itm == null && !r.Any(i => i.Id == msg.Id) ) {
              itm = CreateQueueItem(q.Queue, msg);

              // Load Message names and check if its not an infra-message
              if( !PrepareQueueItemForAdd(itm) )
                itm = null;
            }

            if( itm != null )
              r.Insert(0, itm);

            // Just fetch first 500
            if( r.Count > SbmqSystem.MAX_ITEMS_PER_QUEUE )
              break;
          }

        } catch( Exception e ) {
          OnError("Error occured when processing queue " + q.Queue.Name + ", " + e.Message, e, false);
        }

      }

      return result;
    }
    public override QueueFetchResult GetProcessedMessages(QueueType type, DateTime since, IEnumerable<QueueItem> currentItems) {
      var result = new QueueFetchResult();

      var queues = GetQueueListByType(type);
      if( queues.Count() == 0 ) {
        result.Items = EMPTY_LIST;
        return result;
      }
      List<QueueItem> r = new List<QueueItem>();
      result.Items = r;

      foreach( var q in queues ) {
        string qName = q.GetDisplayName();

        if( IsIgnoredQueue(qName) || !q.CanReadJournalQueue )
          continue;

        SetupMessageReadPropertyFilters(q.Journal, type);

        try {
          List<Message> messages = new List<Message>();

          // Enumete from the earliest item
          MessageEnumerator msgs = q.Journal.GetMessageEnumerator2();
          try {
            while( msgs.MoveNext() ) {
              Message msg = msgs.Current;

              if( msg.ArrivedTime >= since )
                messages.Add(msg);
            }
          } finally {
            msgs.Close();
          }

          foreach( var msg in messages ) {
            QueueItem itm = currentItems.FirstOrDefault(i => i.Id == msg.Id);

            if( itm == null ) {
              itm = CreateQueueItem(q.Queue, msg);
              itm.Processed = true;

              if( !PrepareQueueItemForAdd(itm) )
                itm = null;
            }

            if( itm != null )
              r.Insert(0, itm);
          }



        } catch( Exception e ) {
          OnError("Error occured when getting processed messages from queue \"" + qName + "\", " + e.Message, e, false);
        }

      }

      result.Count = (uint)r.Count;

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

        } catch {
#if DEBUG
          Console.WriteLine("Failed to parse NServiceBus.ProcessingStarted");
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

    private static readonly XmlSerializer headerSerializer = new XmlSerializer(typeof(List<HeaderInfo>));

    private QueueItem CreateQueueItem(Queue queue, Message msg) {
      var itm = new QueueItem(queue);
      itm.DisplayName = msg.Label;
      itm.Id = msg.Id;
      itm.ArrivedTime = msg.ArrivedTime;
      //itm.Content = ReadMessageStream(msg.BodyStream);

      itm.Headers = new Dictionary<string, string>();
      if( msg.Extension.Length > 0 ) {
        var stream = new MemoryStream(msg.Extension);
        var o = headerSerializer.Deserialize(stream);

        foreach( var pair in o as List<HeaderInfo> )
          if( pair.Key != null )
            itm.Headers.Add(pair.Key, pair.Value);
      }


      return itm;
    }


    private MsmqMessageQueue GetMessageQueue(QueueItem itm) {
      return _monitorQueues.Single(i => i.Queue.Type == itm.Queue.Type && i.Queue.Name == itm.Queue.Name);
    }

    public override string LoadMessageContent(QueueItem itm) {
      if( itm.Content == null ) {

        MsmqMessageQueue msmq = GetMessageQueue(itm);

        msmq.LoadMessageContent(itm);
      }

      return itm.Content;
    }


    public override MessageSubscription[] GetMessageSubscriptions(Dictionary<string, string> connectionSettings, IEnumerable<string> queues) {
      var server = connectionSettings[CS_SERVER];
      List<MessageSubscription> r = new List<MessageSubscription>();

      // Raven Persistance
      var ravenUrl = connectionSettings.GetValue(NServiceBus_MSMQ_Manager.CS_RAVEN_PERSISTANCE, null) ?? "http://" + server + ":8080"; 
      var db = new DocumentStore {
        Url = ravenUrl
      };
      db.Initialize();

      // MSMQ Persistance
      var msmqQ = MessageQueue.GetPrivateQueuesByMachine(server).Where(q => q.QueueName.EndsWith(".subscriptions")).Select(q => q.QueueName);

      foreach( var queueName in queues ) {

        // First check MSMQ
        if( msmqQ.Any(mq => mq == queueName + ".subscriptions") ) {

          MessageQueue q = Msmq.Create(server, queueName + ".subscriptions", QueueAccessMode.ReceiveAndAdmin);

          q.MessageReadPropertyFilter.Label = true;
          q.MessageReadPropertyFilter.Body = true;

          try {
            foreach( var msg in q.GetAllMessages() ) {

              var itm = new MessageSubscription();
              itm.FullName = GetSubscriptionType(ReadMessageStream(msg.BodyStream));
              itm.Name = ParseClassName(itm.FullName);
              itm.Subscriber = msg.Label;
              itm.Publisher = queueName;

              r.Add(itm);
            }
          } catch( Exception e ) {
            OnError("Error occured when getting subcriptions", e, true);
          }


        } else { // RavenDB

          var headers = db.DatabaseCommands.Head("Raven/Databases/" + queueName);
          if( headers != null ) {

            using( var s = db.OpenSession(queueName) ) {

              //s.Advanced.AllowNonAuthoritativeInformation = false;
              //var doc = s.Load<Subscription>("subscriptions/88dc7793-0cbd-ec39-5dd2-6b982e1e3c76");
              //if( doc != null ) 
              //  Console.Write("WOW");

              var list = s.Advanced.LoadStartingWith<Subscription>("subscription", null, 0, 1024).ToArray();

              foreach( var subr in list ) { //.StartsWith("Subscriptions/")) ) {

                foreach( var client in subr.Clients ) {

                  var itm = new MessageSubscription();
                  itm.FullName = subr.MessageType.ToString();
                  itm.Name = ParseClassName(itm.FullName);
                  itm.Subscriber = client.ToString();
                  itm.Publisher = queueName;

                  r.Add(itm);
                }

              }
            }
          }



        }


      }




      //foreach( var queueName in MessageQueue.GetPrivateQueuesByMachine(server).
      //                                      Where(q => q.QueueName.EndsWith(".subscriptions")).Select(q => q.QueueName) ) {

      //}

      return r.ToArray();
    }

    private string ParseClassName(string asmName) {

      if( asmName.IsValid() ) {

        int iEnd = asmName.IndexOf(',');
        int iStart = asmName.LastIndexOf('.', iEnd);

        if( iEnd > -1 && iStart > -1 ) {
          iStart++;
          return asmName.Substring(iStart, iEnd - iStart);
        }

      }

      return asmName;
    }

    public override void PurgeErrorMessages(string queueName) {
      //string name = "private$\\" + queueName;

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

    public override void PurgeMessage(QueueItem itm) {
      MessageQueue q = GetMessageQueue(itm);

      if( q != null ) {
        q.ReceiveById(itm.Id);

        itm.Processed = true;

        OnItemsChanged();
      }
    }
    public override void PurgeAllMessages() {
      _monitorQueues.ForEach(q => q.Purge());

      OnItemsChanged();
    }



    public override void MoveErrorMessageToOriginQueue(QueueItem itm) {
      if( string.IsNullOrEmpty(itm.Id) )
        throw new ArgumentException("MessageId can not be null or empty");

      if( itm.Queue.Type != QueueType.Error )
        throw new ArgumentException("Queue is not of type Error, " + itm.Queue.Type);

      var mgr = new ErrorManager();

      // TODO:
      // Check if Clustered Queue, due if Clustered && NonTransactional, then Error

      mgr.InputQueue = Address.Parse(itm.Queue.Name);

      mgr.ReturnMessageToSourceQueue(itm.Id);
    }
    public override void MoveAllErrorMessagesToOriginQueue(string errorQueue) {
      var mgr = new ErrorManager();

      // TODO:
      // Check if Clustered Queue, due if Clustered && NonTransactional, then Error

      mgr.InputQueue = Address.Parse(errorQueue);

      mgr.ReturnAll();
    }



    private static readonly string[] IGNORE_DLL = new string[] { "\\Autofac.dll", "\\AutoMapper.dll", "\\log4net.dll", 
                                                                  "\\MongoDB.Driver.dll", "\\MongoDB.Bson.dll", 
                                                                  "\\NServiceBus.dll" };

    #region Send Command


    public Type[] GetAvailableCommands(string[] asmPaths) {
      return GetAvailableCommands(asmPaths, _commandDef, false);
    }
    public Type[] GetAvailableCommands(string[] asmPaths, CommandDefinition commandDef, bool suppressErrors) {
      List<Type> arr = new List<Type>();


      List<string> nonExistingPaths = new List<string>();


      foreach( var path in asmPaths ) {

        if( Directory.Exists(path) ) {

          foreach( var dll in Directory.GetFiles(path, "*.dll") ) {

            if( IGNORE_DLL.Any(a => dll.EndsWith(a)) )
              continue;

            try {
              var asm = Assembly.LoadFrom(dll);
              //var asm = Assembly.ReflectionOnlyLoadFrom(dll);

              foreach( Type t in asm.GetTypes() ) {

                if( commandDef.IsCommand(t) )
                  arr.Add(t);

              }

            } catch( ReflectionTypeLoadException fte ) {

              if( suppressErrors )
                continue;

              StringBuilder sb = new StringBuilder();
              if( fte.LoaderExceptions != null ) {

                if( fte.LoaderExceptions.All(a => a.Message.EndsWith("does not have an implementation.")) )
                  continue;

                string lastMsg = null;
                foreach( var ex in fte.LoaderExceptions ) {
                  if( ex.Message != lastMsg )
                    sb.AppendFormat(" - {0}\n\n", lastMsg = ex.Message);
                }
              }

              OnWarning("Could not search for Commands in Assembly '{0}'".With(Path.GetFileName(dll)), sb.ToString());

            } catch { }

          }
        } else nonExistingPaths.Add(path);
      }

      if( nonExistingPaths.Count > 0 )
        OnError("The paths '{0}' doesn't exist, could not search for commands.".With(nonExistingPaths.Concat()));


      return arr.ToArray();
    }



    protected IBus _bus;


    public void SetupServiceBus(string[] assemblyPaths, CommandDefinition cmdDef, Dictionary<string, string> connectionSettings) {
      _commandDef = cmdDef;

      Console.Write(typeof(global::NServiceBus.Configure).FullName);

      List<Assembly> asms = new List<Assembly>();

      foreach( string path in assemblyPaths ) {

        foreach( string file in Directory.GetFiles(path, "*.dll") ) {
          try {
            asms.Add(Assembly.LoadFrom(file));
          } catch { }
        }

      }

      if( CommandContentFormat == "XML" ) {

        _bus = Configure.With(asms)
                  .DefineEndpointName("SBMQM_NSB")
                  .DefaultBuilder()
            .DefiningCommandsAs(t => _commandDef.IsCommand(t))
                  .XmlSerializer()
                  .UseTransport<global::NServiceBus.Msmq>()
                  .UnicastBus()
              .SendOnly();

      } else if( CommandContentFormat == "JSON" ) {

        _bus = Configure.With(asms)
                .DefineEndpointName("SBMQM_NSB")
                .DefaultBuilder()
          .DefiningCommandsAs(t => _commandDef.IsCommand(t))
                .JsonSerializer()
                .UseTransport<global::NServiceBus.Msmq>()
                .UnicastBus()
            .SendOnly();
      }

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


    public void SendCommand(Dictionary<string, string> connectionStrings, string destinationQueue, object message) {
      var srv = connectionStrings["server"];

      if( Tools.IsLocalHost(srv) )
        srv = null;

      string dest = !string.IsNullOrEmpty(srv) ? destinationQueue + "@" + srv : destinationQueue;


      //var assemblies = message.GetType().Assembly
      // .GetReferencedAssemblies()
      // .Select(n => Assembly.Load(n))
      // .ToList();
      //assemblies.Add(GetType().Assembly);


      if( message != null )
        _bus.Send(dest, message);
      else OnError("Can not send an incomplete message");

    }

    #endregion

  }
}
