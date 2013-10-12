using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceBusMQ.Model;

namespace ServiceBusMQ.NServiceBus {
  public interface IMessageQueue {
    Queue Queue { get; set; }

  }
}
