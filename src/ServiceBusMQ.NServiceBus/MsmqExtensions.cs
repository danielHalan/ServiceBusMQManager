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
