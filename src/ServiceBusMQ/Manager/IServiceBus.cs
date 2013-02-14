#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    IServiceBus.cs
  Created: 2013-02-14

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

namespace ServiceBusMQ.Manager {
  public interface IServiceBus {
    /// <summary>
    /// The name of the ServiceBus this Manager manages
    /// </summary>
    string ServiceBusName { get; }

    /// <summary>
    /// The name of message transportation, and if have multiple formats, message format, i e "MSMQ (XML)"
    /// </summary>
    string TransportationName { get; }
  }
}
