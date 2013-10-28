#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    ObjectExtensions.cs
  Created: 2013-10-27

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
  public static class ObjectExtensions {
  
    public static T As<T>(this object obj) {
      return (T)obj;
    }


  
  }
}
