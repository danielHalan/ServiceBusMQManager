#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    SystemConfig1.cs
  Created: 2012-12-05

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using ServiceBusMQ.Model;

namespace ServiceBusMQ {

  [Serializable]
  public class VersionCheck {
    public bool Enabled = true;
    public DateTime LastCheck = DateTime.MinValue;
  }



  [Serializable]
  public class ServerConfig {
    public string Name { get; set; }

    public string MessageBus { get; set; }
    public string MessageBusQueueType { get; set; }

    public int MonitorInterval { get; set; }
    
    public string[] WatchEventQueues { get; set; }
    public string[] WatchCommandQueues { get; set; }
    public string[] WatchMessageQueues { get; set; }
    public string[] WatchErrorQueues { get; set; }
  }



  [Serializable]
  public class SystemConfig1 : SystemConfig {
    private ServerConfig _currentServer;
    private string _monitorServer;

    public List<ServerConfig> Servers { get; set; }
    
    [JsonIgnore]
    public ServerConfig CurrentServer { get { 
        if( _currentServer == null ) {
          _currentServer = Servers.SingleOrDefault( s => s.Name == MonitorServer );
        }

        return _currentServer;
      }
    }

    public string MonitorServer { get { return _monitorServer; }  
      set {
        if( _monitorServer != value ) {
          _currentServer = null;
          _monitorServer = value;
        }
      } 
    }

    //public string ServerName { get; set; }

    [JsonIgnore]
    public string[] WatchEventQueues { get { return CurrentServer.WatchEventQueues; } }
    
    [JsonIgnore]
    public string[] WatchCommandQueues { get { return CurrentServer.WatchCommandQueues; } }
    
    [JsonIgnore]
    public string[] WatchMessageQueues { get { return CurrentServer.WatchMessageQueues; } }
    
    [JsonIgnore]
    public string[] WatchErrorQueues { get { return CurrentServer.WatchErrorQueues; } }

    [JsonIgnore]
    public string MessageBus { get { return CurrentServer.MessageBus; } }
    
    [JsonIgnore]
    public string MessageBusQueueType { get { return CurrentServer.MessageBusQueueType; } }

    [JsonIgnore]
    public int MonitorInterval { get { return CurrentServer.MonitorInterval; } }

    public bool ShowOnNewMessages { get; set; }

    public string[] CommandsAssemblyPaths { get; set; }

    public CommandDefinition CommandDefinition { get; set; }

    public VersionCheck VersionCheck { get; set; }

    protected override void FillDefaulValues() {

      if( VersionCheck == null ) 
        VersionCheck = new VersionCheck();

      // Convert MSMQ plain to XML, as we now support more then one content serializer
      foreach( var srv in this.Servers ) {
        if( srv.MessageBus == "NServiceBus" && srv.MessageBusQueueType == "MSMQ" )
          srv.MessageBusQueueType = "MSMQ (XML)";
      }

      if( CommandDefinition == null ) {
      
        CommandDefinition = new CommandDefinition(); 
        
        // Temp until support for more then NServiceBus is implemented
        CommandDefinition.InheritsType = "NServiceBus.ICommand, NServiceBus";

      }

    }

    public int StartCount { get; set; }

  }
}
