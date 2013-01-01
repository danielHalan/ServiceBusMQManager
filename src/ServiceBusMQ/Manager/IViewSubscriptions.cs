using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceBusMQ.Manager {
  public interface IViewSubscriptions {

    MessageSubscription[] GetMessageSubscriptions(string server);

  }
}
