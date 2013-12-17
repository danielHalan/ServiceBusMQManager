#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    ServiceBusFeature.cs
  Created: 2013-12-07

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
  public enum ServiceBusFeature { PurgeMessage, PurgeAllMessages, 
                                    MoveErrorMessageToOriginQueue, MoveAllErrorMessagesToOriginQueue }

  public static class ServiceBusFeatures { 
  
    public static readonly ServiceBusFeature[] All = new ServiceBusFeature[] {
      ServiceBusFeature.PurgeMessage, 
      ServiceBusFeature.PurgeAllMessages, 
      ServiceBusFeature.MoveErrorMessageToOriginQueue, 
      ServiceBusFeature.MoveAllErrorMessagesToOriginQueue 
    };
  }

}
