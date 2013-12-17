#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    RestartRequiredException.cs
  Created: 2013-10-15

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

namespace ServiceBusMQ {
  public class RestartRequiredException : Exception {
  
    public RestartRequiredException() { 
    }
  }
}
