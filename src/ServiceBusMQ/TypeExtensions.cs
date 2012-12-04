#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    TypeExtensions.cs
  Created: 2012-12-01

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

    public static string GetDisplayName(this Type type, object value) {

      if( value == null ) 
        return string.Format("{0} (Undefined)", type.Name);
      else {

        var props = type.GetProperties().OrderBy( p => p.Name ).Aggregate(new StringBuilder(),
                      (sb, p) => sb.Length > 0 ? sb.Append(", " + GetAttribValue(p, value)) : sb.Append(GetAttribValue(p, value)));

        return string.Format("{0} ({1})", type.Name, props.ToString());
      }
    
    }

    private static string GetAttribValue(System.Reflection.PropertyInfo p, object obj) {
      object value = p.GetValue(obj, null);

      string res = string.Empty;

      Type t = p.PropertyType;

      if( t == typeof(string) )
        res = (string)value;

      else if( t.IsClass && !t.IsPrimitive ) {

        if( value == null )
          res = string.Format("{0}(null)", t.Name);
        else res = t.Name;


      } else if( value != null )
        res = value.ToString();


      return res.CutEnd(16);
    }

  
  }
}
