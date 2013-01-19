#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    StringExtensions.cs
  Created: 2012-09-10

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
using System.Threading.Tasks;

namespace ServiceBusMQ {
  public static class StringExtensions {

    public static string CutBeginning(this string str, int length) {
      if( str.IsValid() ) {
        length -= 3;
        if( str.Length > length ) {
          return "..." + str.Substring(str.Length - length, length);
        }
      }

      return str;
    }
    public static string CutEnd(this string str, int length) {
      if( str.IsValid() ) {
        length -= 3;
        if( str.Length > length && !str.EndsWith("...") ) {
          return str.Substring(0, length) + "...";
        }
      }

      return str;
    }

    public static string Default(this string str, string def) {
      return string.IsNullOrEmpty(str) ? def : str;
    }


    public static string RemoveNumbers(this string str) {

      if( str.IsValid() ) {
        StringBuilder sb = new StringBuilder(str.Length);
        foreach( char c in str )
          if( !char.IsDigit(c) )
            sb.Append(c);

        return sb.ToString();

      } else return str;
    }
    public static string RemoveNonChars(this string str) {

      if( str.IsValid() ) {
        StringBuilder sb = new StringBuilder(str.Length);
        foreach( char c in str )
          if( !char.IsControl(c) )
            sb.Append(c);

        return sb.ToString();

      } else return str;
    }

    public static string OnlyNumbers(this string str) {

      if( str.Any(c => !char.IsDigit(c)) ) {

        StringBuilder sb = new StringBuilder(str.Length);
        foreach( char c in str )
          if( char.IsDigit(c) )
            sb.Append(c);

        return sb.ToString();

      } else return str;
    }


    public static int AsInt32(this string str, int def) {
      StringBuilder sb = new StringBuilder(str.Length);
      foreach( char c in str )
        if( char.IsDigit(c) )
          sb.Append(c);

      int result = 0;
      return ( ( sb.Length > 0 ) && ( int.TryParse(sb.ToString(), out result) ) ) ? result : def;
    }

    public static int AsInt32(this string str) {
      return AsInt32(str, 0);
    }

    public static bool IsNumeric(this string Expression) {
      double ret;

      return Double.TryParse(Expression,
            System.Globalization.NumberStyles.Any,
            System.Globalization.NumberFormatInfo.InvariantInfo,
            out ret);
    }

    public static bool IsInt32(this string str) {
      int ret;

      return Int32.TryParse(str,
            System.Globalization.NumberStyles.Any,
            System.Globalization.NumberFormatInfo.InvariantInfo,
            out ret);
    }

    public static bool IsDouble(this string Expression) {
      double ret;

      return Double.TryParse(Expression,
            System.Globalization.NumberStyles.Any,
            System.Globalization.NumberFormatInfo.InvariantInfo,
            out ret);
    }
    public static bool IsDecimal(this string str) {
      Decimal ret;

      return Decimal.TryParse(str,
            System.Globalization.NumberStyles.Any,
            System.Globalization.NumberFormatInfo.InvariantInfo,
            out ret);
    }

    public static bool IsValid(this string str) {
      return !string.IsNullOrEmpty(str);
    }

    public static int Convert(this string str, int @default) {
      int r = 0;
      return Int32.TryParse(str, out r) ? r : @default;
    }

    public static bool TryParseToInt32(this string str, ref object value) {
      int r = 0;
      bool result = Int32.TryParse(str, out r);
      value = r;

      return result;
    }

    public static bool TryParseToDouble(this string str, ref object value) {
      double r = 0;
      bool result = double.TryParse(str, out r);
      value = r;

      return result;
    }
    public static bool TryParseToDecimal(this string str, ref object value) {
      Decimal r = 0;
      bool result = Decimal.TryParse(str, out r);
      value = r;

      return result;
    }
    public static bool TryParseToGuid(this string str, ref object value) {
      Guid r = Guid.Empty;
      bool result = Guid.TryParse(str, out r);
      value = r;

      return result;
    }

    public static string With(this string str, params object[] prms) {
      return string.Format(str, prms);
    }

  }
}
