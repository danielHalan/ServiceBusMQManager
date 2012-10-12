#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    ErrorArgs.cs
  Created: 2012-10-06

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

namespace ServiceBusMQ.Manager {
  public class ErrorArgs : EventArgs {

    public string Message { get; private set; }
    public bool Fatal { get; private set; }
  
    public ErrorArgs(string message, bool fatal) {
    
      Message = message;
      Fatal = fatal;
    }
  
  }
}
