using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceBusMQManager.Controls {
  public class FailedDeserializingCommandException : Exception {
  
    public FailedDeserializingCommandException() : base() {}
    public FailedDeserializingCommandException(Exception e) : base(e.Message, e) { 
    
    } 
  
  }
}
