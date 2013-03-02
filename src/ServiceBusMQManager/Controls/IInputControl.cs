#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    IInputControl.cs
  Created: 2012-11-22

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;

namespace ServiceBusMQManager.Controls {
  public interface IInputControl {
  
    void UpdateValue(object value);
  
    object RetrieveValue();

    bool IsListItem { get; set; }

    event EventHandler<EventArgs> ValueChanged;

  }
}
