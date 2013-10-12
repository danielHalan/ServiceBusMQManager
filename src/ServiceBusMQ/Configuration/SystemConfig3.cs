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

      public string MessageBus { get; set; }
      public string MessageBusQueueType { get; set; }

      public int MonitorInterval { get; set; }

      public QueueConfig[] MonitorQueues { get; set; }

      public Dictionary<string, string> ConnectionSettings { get; set; }

      public ServerConfig3() { 
        MonitorInterval = DEFAULT_MONITOR_INTERVAL;
      }

      public static ServerConfig3 Default {
        get {
          var r = new ServerConfig3() {
            MessageBus = "NServiceBus", MessageBusQueueType = "MSMQ",
            MonitorInterval = DEFAULT_MONITOR_INTERVAL,
            Name = Environment.MachineName,
            ConnectionSettings = new Dictionary<string, string>(),
            MonitorQueues = new QueueConfig[0]
          };

          r.ConnectionSettings.Add("server", Environment.MachineName);

          return r;
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

    }




}
