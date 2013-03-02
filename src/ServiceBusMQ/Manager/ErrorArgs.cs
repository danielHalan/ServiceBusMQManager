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

namespace ServiceBusMQ.Manager {
  
  public class ErrorArgs : EventArgs {

    public string Message { get; private set; }
    public Exception Exception { get; set; }
    
    public bool Fatal { get; private set; }

    public ErrorArgs(string message, Exception exception, bool fatal = false) {
    
      Message = message;
      Fatal = fatal;
      Exception = exception;
    }
  
  }
}
