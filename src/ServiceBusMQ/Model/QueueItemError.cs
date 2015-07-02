#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    QueueItemError.cs
  Created: 2012-12-14

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

namespace ServiceBusMQ.Model {

  public enum QueueItemErrorState { Retry, ErrorQueue } 

  public class QueueItemError {
    public QueueItemErrorState State { get; set; }  
    public string Message { get; set; }
    public string StackTrace { get; set; }
    public DateTime TimeOfFailure { get; set; }
    public int Retries { get; set; }

    public string OriginQueue { get; set; }
  }
}
