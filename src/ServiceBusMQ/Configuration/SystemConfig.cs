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
using ServiceBusMQ.Configuration;

namespace ServiceBusMQ {
  public abstract class SystemConfig {

    static SystemConfig() {
    }

    protected abstract void FillDefaulValues();

    static ConfigFactory _configFac;

    public static SystemConfig2 Load() {
      if( _configFac == null )
        _configFac = new ConfigFactory();

      var cfg = _configFac.Create();
      cfg.FillDefaulValues();

      return cfg;
    }

    public void Save() {
      if( _configFac == null )
        _configFac = new ConfigFactory();

      _configFac.Store(this);
    }

    public static object cfg1 { get; set; }
  }
}
