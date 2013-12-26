using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceBusMQ.Model {
  public class QueueFetchUnprocessedMessagesRequest {

    public QueueType Type { get; set; }
    
    public IEnumerable<QueueItem> CurrentItems { get; set; }
    
    public uint TotalCount { get; set; }


    public QueueFetchUnprocessedMessagesRequest(QueueType type, IEnumerable<QueueItem> currentItems, uint totalCount) { 
      Type = type;
      CurrentItems = currentItems;
      TotalCount = totalCount;
    }

  }
}
