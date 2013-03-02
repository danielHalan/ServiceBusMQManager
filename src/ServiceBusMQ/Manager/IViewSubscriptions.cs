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


namespace ServiceBusMQ.Manager {
  
  /// <summary>
  /// Used to retrieve all Event Subscriptions
  /// </summary>
  public interface IViewSubscriptions {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="server"></param>
    /// <returns></returns>
    MessageSubscription[] GetMessageSubscriptions(string server);

  }
}
