#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    ListExtensions.cs
  Created: 2013-02-11

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
  public static class ListExtensions {
  
    public static string Concat<T>(this List<T> list) {
      return list.Aggregate(new StringBuilder(), (sb, name) => sb.Length > 0 ? sb.Append(", ").Append(name) : sb.Append(name)).ToString();
    }
  
  }
}
