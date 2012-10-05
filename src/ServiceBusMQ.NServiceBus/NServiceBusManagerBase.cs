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

namespace ServiceBusMQManager.MessageBus.NServiceBus {
  public abstract class NServiceBusManagerBase : MessageManagerBase {


    protected string _ignoreMessageBody;

    protected List<MessageQueue> _eventQueues = new List<MessageQueue>();
    protected List<MessageQueue> _cmdQueues = new List<MessageQueue>();
    protected List<MessageQueue> _msgQueues = new List<MessageQueue>();
    protected List<MessageQueue> _errorQueues = new List<MessageQueue>();


    public NServiceBusManagerBase() {
    }

    public override void Init(string serverName, string[] commandQueues, string[] eventQueues,
                      string[] messageQueues, string[] errorQueues) {
      base.Init(serverName, commandQueues, eventQueues, messageQueues, errorQueues);

      _ignoreMessageBody = new StreamReader(this.GetType().Assembly.GetManifestResourceStream("ServiceBusMQ.NServiceBus.CheckMessage.xml")).ReadToEnd();
    }




    public override bool IsIgnoredQueueItem(QueueItem itm) {
      return string.Compare(itm.Content, _ignoreMessageBody) == 0;
    }
    public override bool IsIgnoredQueue(string queueName) {
      return ( queueName.EndsWith("subscriptions") );
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
      if( type == QueueType.Command )
        return DoFetchQueueItems(_cmdQueues, type, currentItems);
      
      else if( type == QueueType.Event )
        return DoFetchQueueItems(_eventQueues, type, currentItems);

      else if( type == QueueType.Message )
        return DoFetchQueueItems(_msgQueues, type, currentItems);

      else if( type == QueueType.Error )
        return DoFetchQueueItems(_errorQueues, type, currentItems);

      else return EMPTY_LIST;
    }
    protected abstract IEnumerable<QueueItem> DoFetchQueueItems(IList<MessageQueue> queues, QueueType type, IList<QueueItem> currentItems);


    protected string GetMessageNames(string xml) {
      StringBuilder sb = new StringBuilder();
      XDocument doc = XDocument.Parse(xml);
      foreach( XElement e in doc.Root.Elements() ) {
        if( sb.Length > 0 ) sb.Append(", ");

        sb.Append(e.Name.LocalName);
      }

      return sb.ToString();
    }



  }
}
