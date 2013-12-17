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
    /// <param name="suppressErrors">If errors in finding commands should be reported or not, if true no errors should be raised</param>
    /// <returns></returns>
    Type[] GetAvailableCommands(string[] asmPaths, CommandDefinition cmdDef, bool suppressErrors);


    /// <summary>
    /// Initialize Service Bus. Called once, before first command is sent.
    /// </summary>
    /// <param name="assemblyPaths"></param>
    void SetupServiceBus(string[] assemblyPaths, CommandDefinition cmdDef, Dictionary<string, object> connectionSettings);
    
    /// <summary>
    /// Send provided Command object to destination Server and Queue.
    /// </summary>
    /// <param name="connectionSettings"></param>
    /// <param name="destinationQueue"></param>
    /// <param name="message"></param>
    void SendCommand(Dictionary<string, object> connectionSettings, string destinationQueue, object message);

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
    object DeserializeCommand(string cmd, Type cmdType);

    /// <summary>
    /// Property that indicates what transportation format the Message content is being stored in.
    /// </summary>
    string CommandContentFormat { set; get; }
    string[] AvailableMessageContentTypes { get; }

  }
}
