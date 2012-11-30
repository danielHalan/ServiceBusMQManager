#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    SystemConfig.cs
  Created: 2012-11-27

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace ServiceBusMQ {
  public class SystemConfig {

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

    public SystemConfig() {

    
    }

    public void Load() {
      var appSett = ConfigurationManager.AppSettings;

      MessageBus = appSett["messageBus"];
      MessageBusQueueType = appSett["messageBusQueueType"];


      ServerName = !string.IsNullOrEmpty(appSett["server"]) ? appSett["server"] : Environment.MachineName;
      WatchEventQueues = ParseStringList("event.queues");
      WatchCommandQueues = ParseStringList("command.queues");
      WatchMessageQueues = ParseStringList("message.queues");
      WatchErrorQueues = ParseStringList("error.queues");

      ShowOnNewMessages = Convert.ToBoolean(appSett["showOnNewMessages"] ?? "false");

      MonitorInterval = Convert.ToInt32(appSett["interval"] ?? "700");

      CommandsAssemblyPaths = ParseStringList("commandsAssemblyPath");
    }


    private string[] ParseStringList(string name) {
      return ConfigurationManager.AppSettings[name].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
    }

  }
}
