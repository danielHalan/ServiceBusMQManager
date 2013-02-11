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
