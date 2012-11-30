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
