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
