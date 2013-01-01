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

  
  }
}
