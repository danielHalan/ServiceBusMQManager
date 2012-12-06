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
