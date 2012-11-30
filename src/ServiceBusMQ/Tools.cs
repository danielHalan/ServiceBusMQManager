#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    Tools.cs
  Created: 2012-11-27

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceBusMQ {
  public static class Tools {

    public static object GetDefault(Type type) {
      if( type.IsValueType ) { 
        
        if( type == typeof(DateTime) )
          return DateTime.Now;
        
        return Activator.CreateInstance(type);
      }
      
      return null;
    }



  }
}
