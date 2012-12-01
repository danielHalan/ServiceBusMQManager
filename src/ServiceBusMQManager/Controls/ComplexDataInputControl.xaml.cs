#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    ComplexDataInputControl.xaml.cs
  Created: 2012-11-21

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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using ServiceBusMQ;

namespace ServiceBusMQManager.Controls {
  /// <summary>
  /// Interaction logic for ComplexDataInputControl.xaml
  /// </summary>
  public partial class ComplexDataInputControl : UserControl, IInputControl {

    Brush ACTIVE_BRUSH;

    Type _type;
    string _attributeName;

    public ComplexDataInputControl(string attributeName, Type type, object value) {
      InitializeComponent();

      ACTIVE_BRUSH = btn.Background;

      _attributeName = attributeName;
      _type = type;
      Value = value;

      btn.Content = type.Name;
    }

    public event EventHandler<ComplexTypeEventArgs> DefineComplextType;
    private void OnDefineComplextType(string attribName, Type t, object value) {
      if( DefineComplextType != null )
        DefineComplextType(this, new ComplexTypeEventArgs(attribName, t, value));
    }


    private void Button_Click_1(object sender, RoutedEventArgs e) {

      OnDefineComplextType(_attributeName, _type, Value);
    }


    object _value;
    private bool _isListItem;

    public object Value {
      get { return _value; }
      set {
        if( _value != value ) {
          _value = value;

          ValueHasChanged();
        }
      }
    }


    private void ValueHasChanged() {

      if( _value == null ) {

        btn.Content = string.Format("{0} (Undefined)", _type.Name);

        //rStatus.Fill = new SolidColorBrush(Color.FromRgb(219, 88, 88));
      } else {

        var props = _type.GetProperties().Aggregate(new StringBuilder(),
                      (sb, p) => sb.Length > 0 ? sb.Append(", " + GetAttribValue(p, _value)) : sb.Append(GetAttribValue(p, _value)));


        btn.Content = string.Format("{0} ({1})", _type.Name, props.ToString().CutEnd(80));

        //rStatus.Fill = new SolidColorBrush(Color.FromRgb(92, 219, 88));
      }

      OnValueChanged();
    }

    private string GetAttribValue(System.Reflection.PropertyInfo p, object obj) {
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

    public void UpdateValue(object value) {
      Value = value;
    }

    public object RetrieveValue() {
      return Value;
    }

    public bool IsListItem {
      get {
        return _isListItem;
      }
      set {
        _isListItem = value;
        ListItemStateChanged();
      }
    }

    private void ListItemStateChanged() {

      //if( _isListItem ) {
      //  btn.Background = Brushes.Gray;
      //} else {
      //  btn.Background = ACTIVE_BRUSH;
      //}
      
      btn.IsEnabled = !_isListItem;
    }


    public event EventHandler<EventArgs> ValueChanged;
    void OnValueChanged() {
      if( ValueChanged != null )
        ValueChanged(this, EventArgs.Empty);
    }


  }
}
