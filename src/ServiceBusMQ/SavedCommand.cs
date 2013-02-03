#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    SavedCommand.cs
  Created: 2012-12-04

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

namespace ServiceBusMQ {

  [Serializable]
  public class SavedCommand {

    public string DisplayName { get; set; }
    
    [JsonIgnore]
    public string FileName { get; set; }

    public object Command { get; set; }

    public DateTime LastSent { get; set; }

    public string ServiceBus { get; set; }
    public string Transport { get; set; }

    public string Server { get; set; }
    public string Queue { get; set; }
  }

  [Serializable]
  public class SavedCommand2 {

    public object Command { get; set; }

    public string ServiceBus { get; set; }
    public string Transport { get; set; }

    public string Server { get; set; }
    public string Queue { get; set; }
  }

}
