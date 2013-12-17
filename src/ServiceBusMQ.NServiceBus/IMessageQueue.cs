#region File Information
/********************************************************************
  Project: ServiceBusMQ.NServiceBus
  File:    IMessageQueue.cs
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
using System.Threading.Tasks;
using ServiceBusMQ.Model;

namespace ServiceBusMQ.NServiceBus {
  public interface IMessageQueue {
    Queue Queue { get; set; }

  }
}
