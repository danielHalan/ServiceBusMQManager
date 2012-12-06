#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    MessageSubscription.cs
  Created: 2012-12-06

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

namespace ServiceBusMQ {
  public class MessageSubscription {

    public string Name { get; set; }
    public string FullName { get; set; }

    public string Publisher { get; set; }
    public string Subscriber { get; set; }
  
  }
}
