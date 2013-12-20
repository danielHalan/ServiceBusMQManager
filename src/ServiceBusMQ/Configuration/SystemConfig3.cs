#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    SystemConfig3.cs
  Created: 2013-10-12

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
using Newtonsoft.Json;

namespace ServiceBusMQ.Configuration {

    [Serializable]
    public class ServerConfig3 {
      
      public static readonly int DEFAULT_MONITOR_INTERVAL = 750;
      
      public string Name { get; set; }

      public string ServiceBus { get; set; }
      public string ServiceBusVersion { get; set; }
      public string ServiceBusQueueType { get; set; }

      public int MonitorInterval { get; set; }

      public QueueConfig[] MonitorQueues { get; set; }

      public Dictionary<string, object> ConnectionSettings { get; set; }

      public string[] CommandsAssemblyPaths { get; set; }

      public CommandDefinition CommandDefinition { get; set; }
      public string CommandContentType { get; set; }

      public ServerConfig3() { 
        MonitorInterval = DEFAULT_MONITOR_INTERVAL;
        CommandDefinition = new CommandDefinition();
      }

      public static ServerConfig3 Default {
        get {
          var r = new ServerConfig3() {
            ServiceBus = "NServiceBus", ServiceBusVersion = "4", ServiceBusQueueType = "MSMQ",
            MonitorInterval = DEFAULT_MONITOR_INTERVAL,
            Name = Environment.MachineName,
            ConnectionSettings = new Dictionary<string, object>(),
            MonitorQueues = new QueueConfig[0],

            CommandsAssemblyPaths = new string[0],
            CommandDefinition = new CommandDefinition(),
            CommandContentType = "XML"
          };

          r.ConnectionSettings.Add("server", Environment.MachineName);

          return r;
        }
      }

      public void CopyTo(ServerConfig3 obj) {
        obj.Name = Name;
        obj.ServiceBus = ServiceBus;
        obj.ServiceBusVersion = ServiceBusVersion;
        obj.ServiceBusQueueType = ServiceBusQueueType;
        obj.MonitorInterval = MonitorInterval;
        obj.MonitorQueues = MonitorQueues;
        obj.ConnectionSettings = new Dictionary<string, object>(ConnectionSettings);

        obj.CommandsAssemblyPaths = CommandsAssemblyPaths;
        obj.CommandDefinition = CommandDefinition;
        obj.CommandContentType = CommandContentType;
      }

      public static string GetFullMessageBusName(string name, string version) {
        if( version.IsValid() )
          return "{0} v{1}".With(name, version);

        else return name;      
      }

      public string FullMessageBusName { 
        get { 
          return GetFullMessageBusName(ServiceBus, ServiceBusVersion);
        }
      }
    }

    [Serializable]
    public class SystemConfig3 : SystemConfig {
      private ServerConfig3 _currentServer;
      private string _monitorServernName;
      
      public string Id { get; set; }

      public List<ServerConfig3> Servers { get; set; }

      [JsonIgnore]
      public ServerConfig3 CurrentServer {
        get {
          if( _currentServer == null ) {
            _currentServer = Servers.SingleOrDefault(s => s.Name == MonitorServerName);
          }

          return _currentServer;
        }
      }

      public string MonitorServerName {
        get { return _monitorServernName; }
        set {
          if( _monitorServernName != value ) {
            _currentServer = null;
            _monitorServernName = value;
          }
        }
      }

      [JsonIgnore]
      public QueueConfig[] MonitorQueues { get { return CurrentServer.MonitorQueues; } }

      [JsonIgnore]
      public string ServiceBus { get { return CurrentServer.ServiceBus; } }

      [JsonIgnore]
      public string ServiceBusVersion { get { return CurrentServer.ServiceBusVersion; } }

      [JsonIgnore]
      public string ServiceBusQueueType { get { return CurrentServer.ServiceBusQueueType; } }

      [JsonIgnore]
      public int MonitorInterval { get { return CurrentServer.MonitorInterval; } }

      public bool ShowOnNewMessages { get; set; }


      public VersionCheck VersionCheck { get; set; }
      public int StartCount { get; set; }

      protected override void FillDefaulValues() {

        if( Id == null )
          Id = Guid.NewGuid().ToString();

        if( VersionCheck == null )
          VersionCheck = new VersionCheck();

        if( Servers == null ) {
          Servers = new List<ServerConfig3>();
          Servers.Add(ServerConfig3.Default);

          MonitorServerName = Servers[0].Name;
        }

        // Convert MSMQ plain to XML, as we now support more then one content serializer
        foreach( var srv in this.Servers ) {
          if( srv.ServiceBus == "NServiceBus" ) {

            if( srv.ServiceBusQueueType == "MSMQ (XML)" ) {
              srv.ServiceBusQueueType = "MSMQ";
              srv.CommandContentType = "XML";
            } else if( srv.ServiceBusQueueType == "MSMQ (JSON)" ) {
              srv.ServiceBusQueueType = "MSMQ";
              srv.CommandContentType = "JSON";
            }

            if( srv.CommandDefinition == null ) {
              srv.CommandDefinition = new CommandDefinition();
              srv.CommandDefinition.InheritsType = "NServiceBus.ICommand, NServiceBus";
            }

          }

          if( !srv.CommandContentType.IsValid() )
            srv.CommandContentType = "XML";
        }

      }
    }




}
