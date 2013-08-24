using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusMQ {
  public class WarningException : Exception {
  

    public WarningException(string msg, string content): base(msg) {
    
      Content = content;
    
    }


    public string Content { get; set; }
  }
}
