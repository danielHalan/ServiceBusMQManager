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
using System.Reflection;
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


    public static object CreateNullable(Type baseType, object value) {
      Type nullableType = typeof(Nullable<>).MakeGenericType(baseType);
      
      ConstructorInfo constructor = nullableType.GetConstructor(new Type[] { baseType });
      
      return constructor.Invoke(new object[] { value });
    }

    public static object ChangeType(object obj, Type type) {

      if( type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)) ) {
        Type t = Nullable.GetUnderlyingType(type);

        if( obj != null )
          return CreateNullable(t, System.Convert.ChangeType(obj, t));
          
        else return null;
      
      } else return System.Convert.ChangeType(obj, type);

    }

    public static object Convert(object obj, Type type) {
      bool isNullable = type.IsNullableValueType();

      if( isNullable ) {
        type = Nullable.GetUnderlyingType(type);
      }

      object res = null;


      if( obj == null && isNullable )
        return ChangeType(obj, type);


      else if( type == typeof(string) )
        res = obj.ToString();

      else if( type == typeof(Int16) )
        res = System.Convert.ToInt16(obj);

      else if( type == typeof(Int32) )
        res = System.Convert.ToInt32(obj);

      else if( type == typeof(Int64) )
        res = System.Convert.ToInt64(obj);

      else if( type == typeof(UInt16) )
        res = System.Convert.ToUInt16(obj);

      else if( type == typeof(UInt32) )
        res = System.Convert.ToUInt32(obj);

      else if( type == typeof(UInt64) )
        res = System.Convert.ToUInt64(obj);

      else if( type == typeof(SByte) )
        res = System.Convert.ToSByte(obj);

      else if( type == typeof(Byte) )
        res = System.Convert.ToByte(obj);

      else if( type == typeof(Single) )
        res = System.Convert.ToSingle(obj);

      else if( type == typeof(Double) )
        res = System.Convert.ToDouble(obj);

      else if( type == typeof(double) )
        res = (double)System.Convert.ToDouble(obj);

      else if( type == typeof(Decimal) )
        res = System.Convert.ToDecimal(obj);

      else if( type == typeof(Guid) ) {

        if( obj.GetType() == typeof(string) )
          res = new Guid((string)obj);
        else if( obj.GetType() == typeof(byte[]) )
          res = new Guid((byte[])obj);
      }

      else if( type == typeof(DateTime) )
        res = System.Convert.ToDateTime(obj);

      else if( type == typeof(Boolean) )
        res = System.Convert.ToBoolean(obj);

      else if( type == typeof(Char) )
        res = System.Convert.ToChar(obj);

      else throw new NotSupportedException("Unhandled data type, Tools.Convert::" + type.ToString());

      return isNullable ? CreateNullable(type, res) : res;
      

    }

    public static object CreateInstance(Type type, Dictionary<string, object> attributes) {
      object i = null;
      
      try {
        i = Activator.CreateInstance(type);

      } catch( MissingMethodException e ) {
        // try match parameters

        foreach( var construct in type.GetConstructors().OrderBy(c => c.GetParameters().Length) ) {

          try {
            var cParams = construct.GetParameters();
            var dict = cParams.Select(c => c.Name).ToDictionary<string, string, object>(d => d, x => null);

            for( int n = 0; n < dict.Count; n++ ) {
              var element = dict.ElementAt(n);

              if( attributes.Any(ke => string.Compare(ke.Key, element.Key, true) == 0) )
                dict[element.Key] = Tools.ChangeType(attributes.Single(ke => string.Compare(ke.Key, element.Key, true) == 0).Value, cParams[n].ParameterType);
              else dict[element.Key] = cParams[n].DefaultValue != System.DBNull.Value ? cParams[n].DefaultValue : Tools.GetDefault(cParams[n].ParameterType);
            }

            var args = dict.Select(d => d.Value).ToArray();
            i = Activator.CreateInstance(type, args);

          } catch {
            continue;
          }

        }
      }

      foreach( var v in attributes ) {
        try {
          type.GetProperty(v.Key).SetValue(i, v.Value, null);
        } catch { }
      }

      if( i == null )
        throw new Exception("Failed to create instance of " + type.Name);


      return i;
    }
  }
}
