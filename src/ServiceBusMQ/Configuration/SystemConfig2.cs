#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    SystemConfig2.cs
  Created: 2013-02-10

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ServiceBusMQ.Model;

namespace ServiceBusMQ.Configuration {

  [Serializable]
  public class QueueConfig {
    public string Name { get; set; }
    public QueueType Type { get; set; }
    public int Color { get; set; }

    public QueueConfig() { }

    public QueueConfig(string name, QueueType type, int color = 0) { 
      Name = name;
      Type = type;
      Color = color;
    }

  }

  [Serializable]
  public class ServerConfig2 {
    public string Name { get; set; }

    public string MessageBus { get; set; }
    public string MessageBusQueueType { get; set; }

    public int MonitorInterval { get; set; }

    public QueueConfig[] MonitorQueues { get; set; }
  }

  [Serializable]
  public class SystemConfig2 : SystemConfig {
    private ServerConfig2 _currentServer;
    private string _monitorServer;

    public List<ServerConfig2> Servers { get; set; }

    [JsonIgnore]
    public ServerConfig2 CurrentServer {
      get {
        if( _currentServer == null ) {
          _currentServer = Servers.SingleOrDefault(s => s.Name == MonitorServer);
        }

        return _currentServer;
      }
    }

    public string MonitorServer {
      get { return _monitorServer; }
      set {
        if( _monitorServer != value ) {
          _currentServer = null;
          _monitorServer = value;
        }
      }
    }

    [JsonIgnore]
    public QueueConfig[] MonitorQueues { get { return CurrentServer.MonitorQueues; } }

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

        // Re-evaluate after suppot for more then NServiceBus is implemented
        if( this.Servers.Count == 0 || this.Servers.First().MessageBus == "NServiceBus" )
          CommandDefinition.InheritsType = "NServiceBus.ICommand, NServiceBus";

      }

    }

    public int StartCount { get; set; }

  }
}
