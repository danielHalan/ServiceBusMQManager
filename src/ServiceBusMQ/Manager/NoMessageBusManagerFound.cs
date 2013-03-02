#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    NoMessageBusManagerFound.cs
  Created: 2012-09-23

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;

namespace ServiceBusMQ.Manager {

  public class NoMessageBusManagerFound : Exception {
  
    public NoMessageBusManagerFound(string name, string queueType) : base("No MessageBus Manager with Name = '" + name + "' and QueueType = '" + queueType + "'") {
    }
  
  }
}
