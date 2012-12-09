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
