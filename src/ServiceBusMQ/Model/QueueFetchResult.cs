#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    QueueFetchResult.cs
  Created: 2013-07-28

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
using System.Threading.Tasks;

namespace ServiceBusMQ.Model {
  public enum QueueFetchResultStatus { OK=0, ConnectionFailed, HasErrors }
  
  public class QueueFetchResult {

    public IEnumerable<Model.QueueItem> Items { get; set; }
    public uint Count { get; set; }

    public QueueFetchResultStatus Status { get; set; }

  }
}
