#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    ISendCommand.cs
  Created: 2013-01-01

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
  /// Interface to Send Commands. 
  /// ISendCommand should be implemented in same class as IServiceBusManager if it supports to send commands.
  /// </summary>
  public interface ISendCommand {
    
    /// <summary>
    /// Get all available commands from libraries found in the Assembly Paths provided.
    /// Use provided CommandDefinition class to determinate what classes are commands.
    /// </summary>
    /// <param name="asmPaths">Array of assembly paths to look for commands in</param>
    /// <returns></returns>
    Type[] GetAvailableCommands(string[] asmPaths, CommandDefinition cmdDef);


    /// <summary>
    /// Initialize Service Bus. Called once, before first command is sent.
    /// </summary>
    /// <param name="assemblyPaths"></param>
    void SetupServiceBus(string[] assemblyPaths, CommandDefinition cmdDef);
    
    /// <summary>
    /// Send provided Command object to destination Server and Queue.
    /// </summary>
    /// <param name="destinationServer"></param>
    /// <param name="destinationQueue"></param>
    /// <param name="message"></param>
    void SendCommand(string destinationServer, string destinationQueue, object message);

    /// <summary>
    /// Serialize command to a textformat, the format should correlate to the MessageContentFormat property.
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns></returns>
    string SerializeCommand(object cmd);
    
    /// <summary>
    /// Deserialize command from text back to an object, the provided text format should correlate to the MessageContentFormat property.
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns></returns>
    object DeserializeCommand(string cmd);

    /// <summary>
    /// Property that indicates what transportation format the Message content is being stored in.
    /// </summary>
    MessageContentFormat MessageContentFormat { get; }

  }
}
