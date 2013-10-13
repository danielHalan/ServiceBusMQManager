#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    ServerConnectionParameter.cs
  Created: 2013-10-11

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

namespace ServiceBusMQ.Manager {
  public class ServerConnectionParameter {
    public string DisplayName { get; set; }
    
    public string SchemaName { get; set; }
    public string DefaultValue { get; set; }

    public static ServerConnectionParameter Create(string schemaName, string displayName, string defaultValue = null) {
      return new ServerConnectionParameter() { 
        SchemaName = schemaName,
        DisplayName = displayName,
        DefaultValue = defaultValue
      };
    }
  }
}
