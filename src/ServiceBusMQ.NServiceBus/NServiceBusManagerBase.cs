#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    NServiceBusMessageManager.cs
  Created: 2012-08-24

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
using System.Text;
using System.Xml.Linq;

using ServiceBusMQ.Model;

using NServiceBus;
using NServiceBus.Tools.Management.Errors.ReturnToSourceQueue;
using ServiceBusMQ.Manager;
using System.Reflection;
using ServiceBusMQ;
using ServiceBusMQ.ViewModel;
using System.Runtime.CompilerServices;

namespace ServiceBusMQ.NServiceBus {

  public abstract class NServiceBusManagerBase : IServiceBusManager {

    static readonly string JSON_START = "\"$type\":\"";
    static readonly string JSON_END = ",";


    protected string _serverName;
    protected CommandDefinition _commandDef;

    protected List<MsmqMessageQueue> _monitorMsmqQueues = new List<MsmqMessageQueue>();


    public NServiceBusManagerBase() {
    }
    public virtual void Init(string serverName, Queue[] monitorQueues, CommandDefinition commandDef) {
      _serverName = serverName;

      MonitorQueues = monitorQueues;

      _commandDef = commandDef;
    }



    private static readonly string NSERVICEBUS_INFRA_MESSAGE = "NServiceBus.Unicast.Transport.CompletionMessage";

    public bool IsIgnoredQueueItem(QueueItem itm) {
      return ( itm.MessageNames.Length == 1 && itm.MessageNames[0] == NSERVICEBUS_INFRA_MESSAGE );
    }
    public bool IsIgnoredQueue(string queueName) {
      return ( queueName.EndsWith(".subscriptions") || queueName.EndsWith(".retries") || queueName.EndsWith(".timeouts") );
    }

    public void MoveErrorItemToOriginQueue(QueueItem itm) {
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
    public void MoveAllErrorItemsToOriginQueue(string errorQueue) {
      var mgr = new ErrorManager();

      // TODO:
      // Check if Clustered Queue, due if Clustered && NonTransactional, then Error

      mgr.InputQueue = Address.Parse(errorQueue);

      mgr.ReturnAll();
    }

    protected string ReadMessageStream(Stream s) {
      using( StreamReader r = new StreamReader(s, Encoding.Default) )
        return r.ReadToEnd().Replace("\0", "");
    }
    protected string GetSubscriptionType(string xml) {
      List<string> r = new List<string>();
      try {
        XDocument doc = XDocument.Parse(xml);

        var e = doc.Root as XElement;
        return e.Value;


      } catch { }

      return string.Empty;
    }
    public abstract MessageSubscription[] GetMessageSubscriptions(string server);


    public abstract string LoadMessageContent(QueueItem itm);

    protected IEnumerable<MsmqMessageQueue> GetQueueListByType(QueueType type) {
      return _monitorMsmqQueues.Where(q => q.Queue.Type == type);
    }


    protected string[] GetMessageNames(string content, bool includeNamespace) {

      if( content.StartsWith("<?xml version=\"1.0\"") )
        return GetXmlMessageNames(content, includeNamespace);
      else return GetJsonMessageNames(content, includeNamespace);

    }
    private string[] GetJsonMessageNames(string content, bool includeNamespace) {
      List<string> r = new List<string>();
      try {
        int iStart = content.IndexOf(JSON_START) + JSON_START.Length;
        int iEnd = content.IndexOf(JSON_END, iStart);

        if( !includeNamespace ) {
          iStart = content.LastIndexOf(".", iEnd) + 1;
        }

        r.Add(content.Substring(iStart, iEnd - iStart));

      } catch { }

      return r.ToArray();
    }
    protected string[] GetXmlMessageNames(string content, bool includeNamespace) {
      List<string> r = new List<string>();
      try {
        XDocument doc = XDocument.Parse(content);
        string ns = string.Empty;

        if( includeNamespace ) {
          ns = doc.Root.Attribute("xmlns").Value.Remove(0, 19) + ".";
        }

        foreach( XElement e in doc.Root.Elements() ) {
          r.Add(ns + e.Name.LocalName);
        }

      } catch { }

      return r.ToArray();
    }



    protected string MergeStringArray(string[] arr) {
      StringBuilder sb = new StringBuilder();
      foreach( var str in arr ) {
        if( sb.Length > 0 ) sb.Append(", ");

        sb.Append(str);
      }

      return sb.ToString();
    }

    public abstract object DeserializeCommand(string cmd);
    public abstract string SerializeCommand(object cmd);


    public abstract MessageContentFormat MessageContentFormat { get; }

    public abstract IEnumerable<QueueItem> GetUnprocessedQueueItems(QueueType type, IEnumerable<QueueItem> currentItems);
    public abstract IEnumerable<QueueItem> GetProcessedQueueItems(QueueType type, DateTime since, IEnumerable<QueueItem> currentItems);

    public abstract void PurgeMessage(QueueItem itm);
    public abstract void PurgeAllMessages();

    public abstract void PurgeErrorMessages(string queueName);
    public abstract void PurgeErrorAllMessages();


    public Queue[] MonitorQueues { get; private set; }


    public event EventHandler<ErrorArgs> ErrorOccured;

    protected void OnError(string message, string stackTrace, bool fatal) {
      if( ErrorOccured != null )
        ErrorOccured(this, new ErrorArgs(message, stackTrace, fatal));
    }
    protected void OnError(string message, Exception e, bool fatal) {
      OnError(string.Format("{0}\n\r {1} ({2})", message, e.Message, e.GetType().Name), e.StackTrace, fatal);
    }


    public string ServiceBusName {
      get { return "NServiceBus"; }
    }

    public abstract string TransportationName { get; }




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
