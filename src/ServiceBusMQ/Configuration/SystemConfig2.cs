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
  public class ServerConfig2 {
    public string Name { get; set; }

    public string MessageBus { get; set; }
    public string MessageBusQueueType { get; set; }

    public int MonitorInterval { get; set; }

    public QueueConfig[] MonitorQueues { get; set; }

    public static ServerConfig2 Default {
      get {
        var r = new ServerConfig2() {
          MessageBus = "NServiceBus", MessageBusQueueType = "MSMQ",
          MonitorInterval = 700,
          Name = Environment.MachineName,
          MonitorQueues = new QueueConfig[0]
        };


        return r;
      }
    }
  }

  [Serializable]
  public class SystemConfig2 : SystemConfig {
    private ServerConfig2 _currentServer;
    private string _monitorServer;
    
    public string Id { get; set; }

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
    public string CommandContentType { get; set; }

    public VersionCheck VersionCheck { get; set; }

    protected override void FillDefaulValues() {

      if( Id == null )
        Id = Guid.NewGuid().ToString();

      if( VersionCheck == null )
        VersionCheck = new VersionCheck();

      if( Servers == null ) {
        Servers = new List<ServerConfig2>();
        Servers.Add(ServerConfig2.Default);

        MonitorServer = Servers[0].Name;
      }

      // Convert MSMQ plain to XML, as we now support more then one content serializer
      foreach( var srv in this.Servers ) {
        if( srv.MessageBus == "NServiceBus" ) {

          if( srv.MessageBusQueueType == "MSMQ (XML)" ) {
            srv.MessageBusQueueType = "MSMQ";
            CommandContentType = "XML";
          } else if( srv.MessageBusQueueType == "MSMQ (JSON)" ) {
            srv.MessageBusQueueType = "MSMQ";
            CommandContentType = "JSON";
          }

        }
      }

      if( CommandDefinition == null ) {

        CommandDefinition = new CommandDefinition();

        // Re-evaluate after suppot for more then NServiceBus is implemented
        if( this.Servers.Count == 0 || this.Servers.First().MessageBus == "NServiceBus" )
          CommandDefinition.InheritsType = "NServiceBus.ICommand, NServiceBus";

      }

      if( !CommandContentType.IsValid() )
        CommandContentType = "XML";

    }

    public int StartCount { get; set; }

    public string MassTransitServiceSubscriptionQueue { get; set; }
  }




}
