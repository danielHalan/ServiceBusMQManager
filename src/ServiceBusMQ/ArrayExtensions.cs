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
using System.Text;

namespace ServiceBusMQ {
  public static class ArrayExtensions {

    public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action) {
      if( action == null )
        throw new ArgumentNullException("action");
      
      foreach( T item in sequence )
        action(item);
    }

    public static string Concat<T>(this T[] list, string separator = ", ") {
      var sb = new StringBuilder(list.Length * 100);
      foreach( T itm in list ) {
      
        if( sb.Length > 0 ) 
          sb.Append(separator).Append(itm);
        else sb.Append(itm);
      }

      return sb.ToString();
    }



  
  }
}
