#region File Information
/********************************************************************
  Project: ServiceBusMQ.Adapter.NServiceBus4.Azure.SB22
  File:    AzureMessageQueue.cs
  Created: 2013-11-30

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
using ServiceBusMQ.Model;
using ServiceBusMQ.NServiceBus;

namespace ServiceBusMQ.Adapter.NServiceBus4.Azure.SB22 {

  public class AzureMessageQueue : ServiceBusMQ.Adapter.Azure.ServiceBus22.AzureMessageQueue, IMessageQueue {

    public AzureMessageQueue(string connectionString, Queue queue, bool deadLetter = false)
      : base(connectionString, queue, deadLetter) {
    }

  }
}
