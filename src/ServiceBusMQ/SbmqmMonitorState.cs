#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    SbmqmMonitorState.cs
  Created: 2013-02-22

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
using ServiceBusMQ.Model;

namespace ServiceBusMQ {
  public class SbmqmMonitorState {

    internal bool[] MonitorQueueType { get; set; }

    public SbmqmMonitorState() {
      MonitorQueueType = new bool[4];
    }      



    public bool IsMonitoringQueueType(QueueType type) {
      return MonitorQueueType[(int)type];
    }
  

  }
}
