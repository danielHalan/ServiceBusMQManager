using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceBusMQ {
  public class SystemConfig1 : SystemConfig {

    public string ServerName { get; set; }
    public string[] WatchEventQueues { get; set; }
    public string[] WatchCommandQueues { get; set; }
    public string[] WatchMessageQueues { get; set; }
    public string[] WatchErrorQueues { get; set; }

    public string MessageBus { get; set; }
    public string MessageBusQueueType { get; set; }

    public bool ShowOnNewMessages { get; set; }
    public int MonitorInterval { get; set; }

    public string[] CommandsAssemblyPaths { get; set; }

    public CommandDefinition CommandDefinition { get; set; }


    protected override void FillDefaulValues() {

      if( CommandDefinition == null ) {
      
        CommandDefinition = new CommandDefinition(); 
        
        // Temp until support for more then NServiceBus is implemented
        CommandDefinition.InheritsType = "NServiceBus.ICommand, NServiceBus";

      }

    }
  }
}
