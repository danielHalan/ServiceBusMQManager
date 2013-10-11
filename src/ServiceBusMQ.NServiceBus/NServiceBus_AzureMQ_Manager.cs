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
using System.Linq;
using System.Text;

namespace ServiceBusMQ.NServiceBus {
  public class NServiceBus_AzureMQ_Manager : NServiceBusManagerBase {
    
    public override MessageSubscription[] GetMessageSubscriptions(string server) {
      throw new NotImplementedException();
    }

    public override string LoadMessageContent(Model.QueueItem itm) {
      throw new NotImplementedException();
    }

    public override object DeserializeCommand(string cmd, Type cmdType) {
      throw new NotImplementedException();
    }

    public override string SerializeCommand(object cmd) {
      throw new NotImplementedException();
    }

    public override Model.QueueFetchResult GetUnprocessedMessages(Model.QueueType type, IEnumerable<Model.QueueItem> currentItems) {
      throw new NotImplementedException();
    }

    public override Model.QueueFetchResult GetProcessedMessages(Model.QueueType type, DateTime since, IEnumerable<Model.QueueItem> currentItems) {
      throw new NotImplementedException();
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

    public override void Terminate() {
    }
  }
}
