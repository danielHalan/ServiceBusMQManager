#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    UIControlFactory.cs
  Created: 2012-11-22

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Windows;
using System.Windows.Controls;
using ServiceBusMQ;
using ServiceBusMQManager.Controls;

namespace ServiceBusMQManager {

  public enum DataType { String, Int, Decimal, Double, Single, Guid, Enum, Bool, Date, Array, Complex }

  public static class UIControlFactory {
  
    public class InputControl {
      public Control Control { get; set; }
      public DataType DataType { get; set; }
      public bool IsNullable { get; set; }
    }

    public static InputControl CreateControl(string name, Type t, object value) {
      InputControl res = new InputControl();


      if( t.Name.StartsWith("Nullable") ) {
        t = Nullable.GetUnderlyingType(t);
        res.IsNullable = true;
      }

      if( !res.IsNullable && value == null )
        value = Tools.GetDefault(t);

      if( t == typeof(string) ) {
        res.IsNullable = true;
        res.Control = new TextInputControl(value, t, res.IsNullable);
        res.DataType = DataType.String;

      } else if( IsInteger(t) ) {
        res.Control = new TextInputControl(value, t, res.IsNullable);
        res.DataType = DataType.Int;

      } else if( IsDecimal(t) ) {
        res.Control = new TextInputControl(value, t, res.IsNullable);
        res.DataType = DataType.Decimal;

      } else if( t.IsEnum ) {
        res.Control = new ComboBoxInputControl(t, value);
        res.DataType = DataType.Enum;

      } else if( t == typeof(bool) ) {
        res.Control = new CheckBoxInputControl(value);
        res.DataType = DataType.Bool;

      } else if( IsDateTime(t) ) {

        res.Control = new TextInputControl(value, t, res.IsNullable);
        res.DataType = DataType.Date;

      } else if( IsGuid(t) ) {
        res.Control = new TextInputControl(value, t, res.IsNullable);
        res.DataType = DataType.Guid;

      } else if( t.IsArray ) {

        res.Control = new ArrayInputControl(t, value, name);
        res.DataType = DataType.Array;
        res.IsNullable = true;

      } else if( t.IsClass ) {

        res.Control = new ComplexDataInputControl(name, t, value);
        res.DataType = DataType.Complex;
        res.IsNullable = true;

      } else MessageBox.Show("Unhandled type " + t.ToString());


      return res;
    
    }


    private static bool IsDecimal(Type t) {
      return t == typeof(decimal)
                         || t == typeof(float)
                         || t == typeof(double)

                         || t == typeof(decimal?)
                         || t == typeof(float?)
                         || t == typeof(double?);
    }

    private static bool IsInteger(Type t) {
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
    private static bool IsGuid(Type t) {
      return t == typeof(Guid)
                         || t == typeof(Guid?);
    }
    private static bool IsDateTime(Type t) {
      return t == typeof(DateTime)
                         || t == typeof(DateTime?);
    }
    private static bool IsComplexDataType(Type t) {
      return t.IsClass;
    }


  
  }
}
