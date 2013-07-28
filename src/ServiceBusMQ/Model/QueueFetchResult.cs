using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusMQ.Model {
  public class QueueFetchResult {

    public IEnumerable<Model.QueueItem> Items { get; set; }
    public uint Count { get; set; }


  }
}
