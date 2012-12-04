using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceBusMQ {
  public class SavedCommand {

    public string DisplayName { get; set; }

    public object Command { get; set; }

    public DateTime LastSent { get; set; }

    public string ServiceBus { get; set; }
    public string Transport { get; set; }

    public string Server { get; set; }
    public string Queue { get; set; }
  }
}
