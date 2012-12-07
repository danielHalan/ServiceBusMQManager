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
using System.IO;
using System.Linq;
using System.Text;

namespace ServiceBusMQ {
  public abstract class SystemConfig {
    private static string _configFile;
    
    private static string _configFileV1;

    static SystemConfig() {
      _configFile = _configFileV1 = SbmqSystem.AppDataPath + @"\config1.dat";
    }

    protected abstract void FillDefaulValues();

    public static SystemConfig1 Load() {
      SystemConfig1 cfg = null; 

      if( File.Exists(_configFileV1) ) {
        cfg = JsonFile.Read<SystemConfig1>(_configFileV1);
      
      } else { // Old config file fallback

        SystemConfig1 c = new SystemConfig1();

        var appSett = ConfigurationManager.AppSettings;

        c.MessageBus = appSett["messageBus"] ?? "NServiceBus";
        c.MessageBusQueueType = appSett["messageBusQueueType"] ?? "MSMQ";


        c.ServerName = !string.IsNullOrEmpty(appSett["server"]) ? appSett["server"] : Environment.MachineName;
        c.WatchEventQueues = ParseStringList("event.queues");
        c.WatchCommandQueues = ParseStringList("command.queues");
        c.WatchMessageQueues = ParseStringList("message.queues");
        c.WatchErrorQueues = ParseStringList("error.queues");

        c.ShowOnNewMessages = Convert.ToBoolean(appSett["showOnNewMessages"] ?? "false");

        c.MonitorInterval = Convert.ToInt32(appSett["interval"] ?? "700");

        c.CommandsAssemblyPaths = ParseStringList("commandsAssemblyPath");

        cfg = c;
      }

      cfg.FillDefaulValues();

      return cfg;
    }


    private static string[] ParseStringList(string name) {
      var c = ConfigurationManager.AppSettings[name];
      return c.IsValid() ? c.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries) : new string[0];
    }

    public void Save() {
      JsonFile.Write(_configFile, this);
    }
  }
}
