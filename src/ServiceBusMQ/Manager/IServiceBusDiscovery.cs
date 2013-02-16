#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    IServiceBusDiscovery.cs
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

  /// <summary>
  /// Used for discovering a new service bus server.
  /// </summary>
  public interface IServiceBusDiscovery : IServiceBus {

    /// <summary>
    /// Checks if the current user can access the server specified, and that the server has a Service Bus of this type running
    /// </summary>
    /// <param name="server">The Host name of the server</param>
    /// <returns></returns>
    bool CanAccessServer(string server);

    /// <summary>
    /// Checks if the current user can access a specific queue on a server.
    /// </summary>
    /// <param name="server">The Host name of the server</param>
    /// <param name="queueName">The Queue name</param>
    /// <returns></returns>
    bool CanAccessQueue(string server, string queueName);

    /// <summary>
    /// Returns all available queues on specified server
    /// </summary>
    /// <param name="server">The Host name of the server</param>
    /// <returns>List of Queue names</returns>
    string[] GetAllAvailableQueueNames(string server);
  
  }
}
