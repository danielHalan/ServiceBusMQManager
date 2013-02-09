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

namespace ServiceBusMQ.NServiceBus {

  public abstract class NServiceBusManagerBase : MessageManagerBase {


    //protected string _ignoreMessageBody;

    protected List<MsmqMessageQueue> _eventQueues = new List<MsmqMessageQueue>();
    protected List<MsmqMessageQueue> _cmdQueues = new List<MsmqMessageQueue>();
    protected List<MsmqMessageQueue> _msgQueues = new List<MsmqMessageQueue>();
    protected List<MsmqMessageQueue> _errorQueues = new List<MsmqMessageQueue>();


    public NServiceBusManagerBase() {
    }
    public override void Init(string serverName, string[] commandQueues, string[] eventQueues,
                      string[] messageQueues, string[] errorQueues, CommandDefinition commandDef) {
      base.Init(serverName, commandQueues, eventQueues, messageQueues, errorQueues, commandDef);


    }



    private static readonly string NSERVICEBUS_INFRA_MESSAGE = "NServiceBus.Unicast.Transport.CompletionMessage";

    public override bool IsIgnoredQueueItem(QueueItem itm) {
      return ( itm.MessageNames.Length == 1 && itm.MessageNames[0] == NSERVICEBUS_INFRA_MESSAGE );
    }
    public override bool IsIgnoredQueue(string queueName) {
      return ( queueName.EndsWith(".subscriptions") || queueName.EndsWith(".retries") || queueName.EndsWith(".timeouts") );
    }

    public override void MoveErrorItemToOriginQueue(QueueItem itm) {
      if( string.IsNullOrEmpty(itm.Id) )
        throw new ArgumentException("MessageId can not be null or empty");

      if( itm.QueueType != QueueType.Error )
        throw new ArgumentException("Queue is not of type Error, " + itm.QueueType);

      var mgr = new ErrorManager();

      // TODO:
      // Check if Clustered Queue, due if Clustered && NonTransactional, then Error

      mgr.InputQueue = Address.Parse(itm.QueueName);

      mgr.ReturnMessageToSourceQueue(itm.Id);
    }
    public override void MoveAllErrorItemsToOriginQueue(string errorQueue) {
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


    protected override IEnumerable<QueueItem> FetchQueueItems(QueueType type, IList<QueueItem> currentItems) {
      return DoFetchQueueItems(GetQueueListByType(type), type, currentItems);
    }
    protected abstract IEnumerable<QueueItem> DoFetchQueueItems(IEnumerable<MsmqMessageQueue> queues, QueueType type, IList<QueueItem> currentItems);

    protected IEnumerable<MsmqMessageQueue> GetQueueListByType(QueueType type) {

      if( type == QueueType.Command )
        return _cmdQueues;

      else if( type == QueueType.Event )
        return _eventQueues;

      else if( type == QueueType.Message )
        return _msgQueues;

      else if( type == QueueType.Error )
        return _errorQueues;

      return null;
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


    protected string[] GetMessageNames(string content, bool includeNamespace) {

      if( content.StartsWith("<?xml version=\"1.0\"") )
        return GetXmlMessageNames(content, includeNamespace);
      else return GetJsonMessageNames(content, includeNamespace);

    }


    static readonly string JSON_START = "\"$type\":\"";
    static readonly string JSON_END = ",";

    private string[] GetJsonMessageNames(string content, bool includeNamespace) {
      List<string> r = new List<string>();
      try {
        int iStart = content.IndexOf(JSON_START) + JSON_START.Length;
        int iEnd = content.IndexOf(JSON_END, iStart);

        if( !includeNamespace ) {
          iStart = content.LastIndexOf(".", iEnd) + 1;
        }

        r.Add( content.Substring(iStart, iEnd-iStart) );

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

  }

}
