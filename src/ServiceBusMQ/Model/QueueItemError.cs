using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceBusMQ.Model {
  public class QueueItemError {
    public string Message { get; set; }
    public DateTime TimeOfFailure { get; set; }
    public int Retries { get; set; }
  }
}
