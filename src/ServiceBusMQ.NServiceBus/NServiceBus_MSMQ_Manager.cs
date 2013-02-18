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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

using ServiceBusMQ;
using ServiceBusMQ.Manager;
using ServiceBusMQ.Model;

using NServiceBus;
using NServiceBus.Utils;
using NServiceBus.Tools.Management.Errors.ReturnToSourceQueue;
using System.Reflection;
using ServiceBusMQ.ViewModel;


namespace ServiceBusMQ.NServiceBus {

  //[PermissionSetAttribute(SecurityAction.LinkDemand, Name = "FullTrust")]
  public abstract class NServiceBus_MSMQ_Manager : NServiceBusManagerBase, ISendCommand, IViewSubscriptions {

    protected List<QueueItem> EMPTY_LIST = new List<QueueItem>();


    class PeekThreadParam {
      public Queue Queue { get; set; }
      public MessageQueue MsmqQueue { get; set; }
    }

    bool _disposed = false;

    public NServiceBus_MSMQ_Manager() {
    }

    public override void Initialize(string serverName, Queue[] monitorQueues) {
      base.Initialize(serverName, monitorQueues);

      LoadQueues();

      // TEMP Removed
      // StartPeekThreads();
    }

    //public override void Dispose() {
    //  base.Dispose();

    //  _disposed = true;
    //}

    /*
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

    public void PeekMessages(object prm) {
      PeekThreadParam p = prm as PeekThreadParam;
      string qName = p.MsmqQueue.GetDisplayName();
      uint sameCount = 0;
      string lastId = string.Empty;

      bool _isPeeking = false;

      SetupMessageReadPropertyFilters(p.MsmqQueue, p.Queue.Type);

      p.MsmqQueue.PeekCompleted += (source, asyncResult) => {
        if( IsMonitoring(p.Queue.Type) ) {
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

      while( !_disposed ) {

        while( !IsMonitoring(p.Queue.Type) ) {
          Thread.Sleep(1000);

          if( _disposed )
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

      lock( _itemsLock ) {

        if( !_items.Any(i => i.Id == msg.Id) ) {

          var itm = CreateQueueItem(q, msg);

          // TODO: Refactor, should not use internal _items list
          if( PrepareQueueItemForAdd(itm) )
            _items.Insert(0, new QueueItemViewModel(itm));

          OnItemsChanged();

          return true;

        } else return false;
      }

    }
     */


    private void LoadQueues() {
      _monitorMsmqQueues.Clear();

      foreach( var queue in MonitorQueues )
        AddMsmqQueue(_serverName, queue);

    }
    private void AddMsmqQueue(string serverName, Queue queue) {
      try {
        _monitorMsmqQueues.Add(new MsmqMessageQueue(serverName, queue));
      } catch( Exception e ) {
        OnError("Error occured when loading queue: '{0}\\{1}'\n\r".With(serverName, queue.Name), e, false);
      }
    }


    private void SetupMessageReadPropertyFilters(MessageQueue q, QueueType type) {

      q.MessageReadPropertyFilter.ArrivedTime = true;
      q.MessageReadPropertyFilter.Label = true;
      q.MessageReadPropertyFilter.Body = true;

      //if( type == QueueType.Error )
      q.MessageReadPropertyFilter.Extension = true;
    }


    public override IEnumerable<Model.QueueItem> GetUnprocessedMessages(QueueType type, IEnumerable<QueueItem> currentItems) {
      var queues = _monitorMsmqQueues.Where(q => q.Queue.Type == type);

      if( queues.Count() == 0 )
        return EMPTY_LIST;

      List<QueueItem> r = new List<QueueItem>();

      foreach( var q in queues ) {
        var msmqQueue = q.Main;

        if( IsIgnoredQueue(q.Queue.Name) || !q.Main.CanRead )
          continue;

        SetupMessageReadPropertyFilters(q.Main, q.Queue.Type);

        try {
          foreach( var msg in q.Main.GetAllMessages() ) {

            QueueItem itm = currentItems.FirstOrDefault(i => i.Id == msg.Id);

            if( itm == null ) {
              itm = CreateQueueItem(q.Queue, msg);
             
              // Load Content and check if its not an infra-message
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

      return r;
    }


    public override IEnumerable<QueueItem> GetProcessedMessages(QueueType type, DateTime since, IEnumerable<QueueItem> currentItems) {
      List<QueueItem> r = new List<QueueItem>();

      var queues = GetQueueListByType(type);
      if( queues.Count() == 0 )
        return EMPTY_LIST;

      foreach( var q in queues ) {
        string qName = q.GetDisplayName();

        if( IsIgnoredQueue(qName) || !q.CanReadJournalQueue )
          continue;

        SetupMessageReadPropertyFilters(q.Journal, type);

        try {

          MessageEnumerator msgs = q.Journal.GetMessageEnumerator2();
          try {
            while( msgs.MoveNext() ) {
              Message msg = msgs.Current;

              if( msg.ArrivedTime >= since ) {

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
            }
          } finally {
            msgs.Close();
          }


        } catch( Exception e ) {
          OnError("Error occured when getting processed messages from queue \"" + qName + "\", " + e.Message, e, false);
        }

      }

      return r;
    }


    private bool PrepareQueueItemForAdd(QueueItem itm) {
      itm.MessageNames = GetMessageNames(itm.Content, true);

      if( !IsIgnoredQueueItem(itm) ) {
        itm.DisplayName = MergeStringArray(GetMessageNames(itm.Content, false)).Default(itm.DisplayName).CutEnd(55);

        return true;
      }

      return false;
    }

    private static readonly XmlSerializer headerSerializer = new XmlSerializer(typeof(List<HeaderInfo>));

    private QueueItem CreateQueueItem(Queue queue, Message msg) {
      var itm = new QueueItem(queue);
      itm.DisplayName = msg.Label;
      itm.Id = msg.Id;
      itm.ArrivedTime = msg.ArrivedTime;
      itm.Content = ReadMessageStream(msg.BodyStream);

      itm.Headers = new Dictionary<string, string>();
      if( msg.Extension.Length > 0 ) {
        var stream = new MemoryStream(msg.Extension);
        var o = headerSerializer.Deserialize(stream);

        foreach( var pair in o as List<HeaderInfo> )
          if( pair.Key != null )
            itm.Headers.Add(pair.Key, pair.Value);
      }

      if( itm.Headers.Any(k => k.Key == "NServiceBus.ProcessingStarted") ) {

        try {
          itm.ProcessTime = Convert.ToInt32(( Convert.ToDateTime(itm.Headers["NServiceBus.ProcessingEnded"]) -
                            Convert.ToDateTime(itm.Headers["NServiceBus.ProcessingStarted"]) ).TotalSeconds);

        } catch { }

      }


      if( itm.Headers.Any(k => k.Key == "NServiceBus.ExceptionInfo.Message") ) {

        itm.Error = new QueueItemError();
        try {
          itm.Error.State = queue.Type == QueueType.Error ? QueueItemErrorState.ErrorQueue : QueueItemErrorState.Retry;

          itm.Error.Message = itm.Headers.SingleOrDefault(k => k.Key == "NServiceBus.ExceptionInfo.Message").Value;

          if( itm.Headers.Any(k => k.Key == "NServiceBus.ExceptionInfo.StackTrace") )
            itm.Error.StackTrace = itm.Headers.Single(k => k.Key == "NServiceBus.ExceptionInfo.StackTrace").Value;

          itm.Error.Retries = Convert.ToInt32(itm.Headers.SingleOrDefault(k => k.Key == "NServiceBus.Retries").Value);
          //itm.Error.TimeOfFailure = Convert.ToDateTime(itm.Headers.SingleOrDefault(k => k.Key == "NServiceBus.TimeOfFailure").Value);
        } catch {
          itm.Error = null;
        }
      }

      return itm;
    }



    private MsmqMessageQueue GetMessageQueue(QueueItem itm) {
      return _monitorMsmqQueues.Single(i => i.Queue.Type == itm.Queue.Type && i.Queue.Name == itm.Queue.Name);
    }

    public override string LoadMessageContent(QueueItem itm) {
      if( itm.Content == null ) {

        MessageQueue mq = GetMessageQueue(itm);

        mq.MessageReadPropertyFilter.ArrivedTime = false;
        mq.MessageReadPropertyFilter.Body = true;
        itm.Content = ReadMessageStream(mq.PeekById(itm.Id).BodyStream);
      }

      return itm.Content;
    }


    public override MessageSubscription[] GetMessageSubscriptions(string server) {

      List<MessageSubscription> r = new List<MessageSubscription>();

      foreach( var queueName in MessageQueue.GetPrivateQueuesByMachine(server).
                                            Where(q => q.QueueName.EndsWith(".subscriptions")).Select(q => q.QueueName) ) {

        MessageQueue q = Msmq.Create(server, queueName, QueueAccessMode.ReceiveAndAdmin);

        q.MessageReadPropertyFilter.Label = true;
        q.MessageReadPropertyFilter.Body = true;

        try {
          var publisher = q.GetDisplayName().Replace(".subscriptions", string.Empty);

          foreach( var msg in q.GetAllMessages() ) {

            var itm = new MessageSubscription();
            itm.FullName = GetSubscriptionType(ReadMessageStream(msg.BodyStream));
            itm.Name = ParseClassName(itm.FullName);
            itm.Subscriber = msg.Label;
            itm.Publisher = publisher;

            r.Add(itm);
          }
        } catch( Exception e ) {
          OnError("Error occured when getting subcriptions", e, true);
        }

      }

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

      _monitorMsmqQueues.Where(q => q.Queue.Type == QueueType.Error && q.Queue.Name == queueName).Single().Purge();

      OnItemsChanged();
    }
    public override void PurgeErrorAllMessages() {
      var items = _monitorMsmqQueues.Where(q => q.Queue.Type == QueueType.Error);

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
      _monitorMsmqQueues.ForEach(q => q.Purge());

      OnItemsChanged();
    }


    private static readonly string[] IGNORE_DLL = new string[] { "\\Autofac.dll", "\\AutoMapper.dll", "\\log4net.dll", 
                                                                  "\\MongoDB.Driver.dll", "\\MongoDB.Bson.dll", 
                                                                  "\\NServiceBus.dll" };

    public Type[] GetAvailableCommands(string[] asmPaths) {
      return GetAvailableCommands(asmPaths, _commandDef);
    }
    public Type[] GetAvailableCommands(string[] asmPaths, CommandDefinition commandDef) {
      List<Type> arr = new List<Type>();


      List<string> nonExistingPaths = new List<string>();


      foreach( var path in asmPaths ) {

        if( Directory.Exists(path) ) {

          foreach( var dll in Directory.GetFiles(path, "*.dll") ) {

            if( IGNORE_DLL.Any(a => dll.EndsWith(a)) )
              continue;

            try {
              var asm = Assembly.LoadFrom(dll);

              foreach( Type t in asm.GetTypes() ) {

                if( commandDef.IsCommand(t) )
                  arr.Add(t);

              }

            } catch { }

          }
        } else nonExistingPaths.Add(path);
      }

      if( nonExistingPaths.Count > 0 )
        OnError("The paths '{0}' doesn't exist, could not search for commands.".With(nonExistingPaths.Concat()), "mgr::GetAvailableCommands", false);


      return arr.ToArray();
    }

    protected IBus _bus;


    public abstract void SetupServiceBus(string[] assemblyPaths, CommandDefinition cmdDef);
    public void SendCommand(string destinationServer, string destinationQueue, object message) {

      if( Tools.IsLocalHost(destinationServer) )
        destinationServer = null;

      string dest = !string.IsNullOrEmpty(destinationServer) ? destinationQueue + "@" + destinationServer : destinationQueue;


      //var assemblies = message.GetType().Assembly
      // .GetReferencedAssemblies()
      // .Select(n => Assembly.Load(n))
      // .ToList();
      //assemblies.Add(GetType().Assembly);


      if( message != null )
        _bus.Send(dest, message);
      else OnError("Can not send an incomp  lete message", string.Empty, false);

    }


  }
}
