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


namespace ServiceBusMQ.Manager {
  
  /// <summary>
  /// Base interface used in all Service Bus adapters
  /// </summary>
  public interface IServiceBus {

    /// <summary>
    /// The name of the ServiceBus this Manager manages
    /// </summary>
    string ServiceBusName { get; }

    /// <summary>
    /// Get available transport modes, MSMQ, RabbitMQ etc
    /// </summary>
    string MessageQueueType { get; }


    /// <summary>
    /// What type of mesage content types the Service Bus supports, XML, Json etc
    /// </summary>
    string[] AvailableMessageContentTypes { get; }

  }
}
