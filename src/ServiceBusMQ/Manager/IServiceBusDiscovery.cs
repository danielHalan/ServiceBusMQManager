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


using System.Collections.Generic;
namespace ServiceBusMQ.Manager {

  /// <summary>
  /// Used for discovering a new service bus server.
  /// </summary>
  public interface IServiceBusDiscovery : IServiceBus {

    /// <summary>
    /// Checks if the current user can access the server specified, and that the server has a Service Bus of this type running
    /// </summary>
    /// <param name="connectionSettings">The Host name of the server</param>
    /// <returns></returns>
    bool CanAccessServer(Dictionary<string, object> connectionSettings);

    /// <summary>
    /// Checks if the current user can access a specific queue on a server.
    /// </summary>
    /// <param name="connectionSettings">The Host name of the server</param>
    /// <param name="queueName">The Queue name</param>
    /// <returns></returns>
    bool CanAccessQueue(Dictionary<string, object> connectionSettings, string queueName);

    /// <summary>
    /// Returns all available queues on specified server
    /// </summary>
    /// <param name="connectionSettings">The Host name of the server</param>
    /// <returns>List of Queue names</returns>
    string[] GetAllAvailableQueueNames(Dictionary<string, object> connectionSettings);


    /// <summary>
    /// Returns all parameters that are needed to establish a server connection to Service Bus
    /// </summary>
    /// <returns></returns>
    ServerConnectionParameter[] ServerConnectionParameters { get; }
  }
}
