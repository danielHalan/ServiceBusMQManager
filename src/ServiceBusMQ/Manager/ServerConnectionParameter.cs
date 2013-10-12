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
