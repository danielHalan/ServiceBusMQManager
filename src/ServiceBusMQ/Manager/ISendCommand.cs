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
  public interface ISendCommand {

    Type[] GetAvailableCommands(string[] asmPaths);
    Type[] GetAvailableCommands(string[] asmPaths, CommandDefinition cmdDef);

    void SetupBus(string[] assemblyPaths);
    void SendCommand(string destinationServer, string destinationQueue, object message);

    string SerializeCommand(object cmd);
    object DeserializeCommand(string cmd);

    MessageContentFormat MessageContentFormat { get; }

  }
}
