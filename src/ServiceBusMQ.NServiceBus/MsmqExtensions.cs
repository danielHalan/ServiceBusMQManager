#region File Information
/********************************************************************
  Project: ServiceBusMQ.NServiceBus
  File:    MsmqExtensions.cs
  Created: 2012-12-12

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;

namespace ServiceBusMQ.NServiceBus {

  public static class MsmqExtensions {

    public static string GetDisplayName(this MessageQueue queue) {
      return queue.FormatName.Substring( queue.FormatName.LastIndexOf('\\') + 1 );
    }
  
  }
}
