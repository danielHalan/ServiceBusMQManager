#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    ArrayExtensions.cs
  Created: 2012-12-09

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
  public static class ArrayExtensions {

    public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action) {
      if( action == null )
        throw new ArgumentNullException("action");
      
      foreach( T item in sequence )
        action(item);
    }

  
  }
}
