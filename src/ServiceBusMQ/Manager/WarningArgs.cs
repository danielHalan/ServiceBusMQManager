using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusMQ.Manager {
  public class WarningArgs : EventArgs {

    public string Message { get; private set; }
    public string Content { get; set; }
    
    public WarningArgs(string message, string content) {
      Message = message;
      Content = content;
    }


  }
}
