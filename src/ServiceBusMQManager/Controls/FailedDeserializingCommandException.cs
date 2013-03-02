#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    FailedDeserializingCommandException.cs
  Created: 2013-01-20

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;

namespace ServiceBusMQManager.Controls {
  public class FailedDeserializingCommandException : Exception {
  
    public FailedDeserializingCommandException() : base() {}
    public FailedDeserializingCommandException(Exception e) : base(e.Message, e) { 
    
    } 
  
  }
}
