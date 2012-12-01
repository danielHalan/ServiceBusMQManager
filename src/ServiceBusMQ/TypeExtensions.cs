using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceBusMQ {
  public static class TypeExtensions {


    public static bool IsNullableValueType(this Type type) {
      return ( type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)) );
    }


    public static bool IsDecimal(this Type t) {
      return t == typeof(decimal)
                         || t == typeof(decimal?);
    }


    public static bool IsAnyFloatType(this Type t) {
      return t == typeof(decimal)
                         || t == typeof(float)
                         || t == typeof(double)

                         || t == typeof(decimal?)
                         || t == typeof(float?)
                         || t == typeof(double?);
    }

    public static bool IsInteger(this Type t) {
      return t == typeof(int)
                         || t == typeof(uint)
                         || t == typeof(long)
                         || t == typeof(ulong)
                         || t == typeof(short)
                         || t == typeof(ushort)

                         || t == typeof(int?)
                         || t == typeof(uint?)
                         || t == typeof(long?)
                         || t == typeof(ulong?)
                         || t == typeof(short?)
                         || t == typeof(ushort?);

    }
    public static bool IsGuid(this Type t) {
      return t == typeof(Guid)
                         || t == typeof(Guid?);
    }
    public static bool IsDateTime(this Type t) {
      return t == typeof(DateTime)
                         || t == typeof(DateTime?);
    }

  
  }
}
