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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace ServiceBusMQ {
  public static class Tools {
    
    public static readonly string[] MONTH_NAMES_ABBR = new string[12];

    public static Action NOOP { get { return () => { }; } }


    static Tools() {
      for(int i = 0; i < 12; i++ ) {
        MONTH_NAMES_ABBR[i] = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i+1).ToUpperFirst();
      }
    }


    public static object GetDefault(Type type) {
      if( type.IsValueType ) {

        if( type == typeof(DateTime) )
          return DateTime.Now;

        else if( type == typeof(Guid) )
          return Guid.Empty;

        else if( type == typeof(string) )
          return string.Empty;

        return Activator.CreateInstance(type);
      }

      return null;
    }


    public static bool IsLocalHost(string server) {
      return ( string.Compare(server, "localhost", true) == 0 ||
            server == "127.0.0.1" ||
          string.Compare(server, Environment.MachineName, true) == 0 );
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

      } else {
        if( obj != null )
          return System.Convert.ChangeType(obj, type);
        else return GetDefault(type);
      }
    }


    public static object AddValue(object obj, Type type, float value) {

      if( obj == null )
        return value;


      if( type == typeof(Single) )
        return System.Convert.ToSingle(obj) + value;

      else if( type == typeof(Double) )
        return System.Convert.ToDouble(obj) + value;

      else if( type == typeof(Decimal) )
        return System.Convert.ToDecimal(obj) + System.Convert.ToDecimal(value);

      else throw new NotSupportedException("AddValue not supporting type " + obj.GetType());


    }
    public static object AddValue(object obj, Type type, int value) {

      if( obj == null )
        return value;

      bool add = value > 0;

      if( type == typeof(Int16) )
        return System.Convert.ToInt16(obj) + value;

      else if( type == typeof(Int32) )
        return System.Convert.ToInt32(obj) + value;

      else if( type == typeof(Int64) )
        return System.Convert.ToInt64(obj) + value;

      else if( type == typeof(UInt16) ) {
        var u = System.Convert.ToUInt16(obj);
        if( u == 0 && !add )
          return u;
        else return u + value;
      } else if( type == typeof(UInt32) ) {
        var u = System.Convert.ToUInt32(obj);
        if( u == 0 && !add )
          return u;
        else return u + value;
      }
        //else if( type == typeof(UInt64) )
        //  return System.Convert.ToUInt64(obj) + value;
        else if( type == typeof(SByte) )
        return System.Convert.ToSByte(obj) + value;

      else if( type == typeof(Byte) ) {
        var u = System.Convert.ToByte(obj);
        if( u == 0 && !add )
          return u;
        else return u + value;
      } else throw new NotSupportedException("AddValue not supporting type " + obj.GetType());

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

      else if( type == typeof(Decimal) )
        res = System.Convert.ToDecimal(obj);

      else if( type == typeof(Guid) ) {

        if( obj.GetType() == typeof(string) )
          res = new Guid((string)obj);
        else if( obj.GetType() == typeof(byte[]) )
          res = new Guid((byte[])obj);
      } else if( type == typeof(DateTime) )
        res = System.Convert.ToDateTime(obj);

      else if( type == typeof(Boolean) )
        res = System.Convert.ToBoolean(obj);

      else if( type == typeof(Char) )
        res = System.Convert.ToChar(obj);

      else throw new NotSupportedException("Unhandled data type, Tools.Convert::" + type.ToString());

      return isNullable ? CreateNullable(type, res) : res;


    }

    public static System.Windows.Point ComputeCartesianCoordinate(double angle, double radius) {
      // convert to radians
      double angleRad = ( Math.PI / 180.0 ) * ( angle - 90 );

      double x = radius * Math.Cos(angleRad);
      double y = radius * Math.Sin(angleRad);

      return new System.Windows.Point(x, y);
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

              if( attributes != null && attributes.Any(ke => string.Compare(ke.Key, element.Key, true) == 0) )
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

      if( attributes != null )
        foreach( var v in attributes ) {
          try {
            type.GetProperty(v.Key).SetValue(i, v.Value, null);
          } catch { }
        }

      if( i == null )
        throw new Exception("Failed to create instance of " + type.Name);


      return i;
    }

    public static string FormatXml(string xml) {
      if( !xml.IsValid() )
        return xml;

      XmlDocument doc = new XmlDocument();
      try {
        doc.LoadXml(xml);

        StringBuilder sb = new StringBuilder();
        using( XmlTextWriter wr = new XmlTextWriter(new StringWriter(sb)) ) {

          wr.Indentation = 2;
          wr.Formatting = System.Xml.Formatting.Indented;

          doc.Save(wr);
        }

        return sb.ToString();

      } catch {
        return xml;
      }
    }
    public static string FormatJson(string content) {
      try {
        return _FormatJson(content);

      } catch {
        return content;
      }
    }



    private const string INDENT_STRING = "    ";
    public static string _FormatJson(string str) {
      var indent = 0;
      var quoted = false;
      var sb = new StringBuilder(str.Length + 100);
      
      for( var i = 0; i < str.Length; i++ ) {
        var ch = str[i];
        
        switch( ch ) {
          case '{':
          case '[':
            sb.Append(ch);
            if( !quoted ) {
              sb.AppendLine();
              Enumerable.Range(0, ++indent).ForEach(item => sb.Append(INDENT_STRING));
            }
            break;
          
          case '}':
          case ']':
            if( !quoted ) {
              sb.AppendLine();
              Enumerable.Range(0, --indent).ForEach(item => sb.Append(INDENT_STRING));
            }
            sb.Append(ch);
            break;
          
          case '"':
            sb.Append(ch);
            bool escaped = false;
            var index = i;
            while( index > 0 && str[--index] == '\\' )
              escaped = !escaped;
            if( !escaped )
              quoted = !quoted;
            break;
          
          case ',':
            sb.Append(ch);
            if( !quoted ) {
              sb.AppendLine();
              Enumerable.Range(0, indent).ForEach(item => sb.Append(INDENT_STRING));
            }
            break;
          
          case ':':
            sb.Append(ch);
            if( !quoted )
              sb.Append(" ");
            break;
          
          default:
            sb.Append(ch);
            break;
        }
      }
      return sb.ToString();
    }

  }
}
