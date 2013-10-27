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
