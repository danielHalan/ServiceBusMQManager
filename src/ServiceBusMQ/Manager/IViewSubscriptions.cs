#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    IViewSubscriptions.cs
  Created: 2013-01-01

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceBusMQ.Manager {
  public interface IViewSubscriptions {

    MessageSubscription[] GetMessageSubscriptions(string server);

  }
}
