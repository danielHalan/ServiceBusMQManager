#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    CommandAttributeControl.xaml.cs
  Created: 2012-11-19

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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ServiceBusMQ;

namespace ServiceBusMQManager.Controls {
  /// <summary>
  /// Interaction logic for CommandAttributeControl.xaml
  /// </summary>

  public class ComplexTypeEventArgs : EventArgs {

    public string AttributeName;
    public Type Type;
    public object Value;

    public ComplexTypeEventArgs(string attribName, Type type, object value) {
      AttributeName = attribName;
      Type = type;
      Value = value;
    }
  }



  public partial class AttributeControl : UserControl {

    Brush HASVALUE_BRUSH;
    
    Type _type;

    DataType _dataType;
    
    Control _valueControl = null;

    public Control ValueControl { get { return _valueControl; } }

    object _value;
    bool _isNullable = false;

    public AttributeControl(string displayName, Type type, object value = null) {
      InitializeComponent();
    
      HASVALUE_BRUSH = rValue.Fill;

      Init(displayName, type, value);
    }

    public void Init(string displayName, Type type, object value = null) {
      DisplayName = displayName;

      _type = type;
      _value = value ?? Tools.GetDefault(type);
      _CreateValueControl(type);

      ValueChanged();
    }


    void _CreateValueControl(Type t) {
      if( _valueControl != null )
        RemoveVisualChild(_valueControl);

      var controlThickness = new Thickness(180, 0, 0, 0);

      var input = UIControlFactory.CreateControl(DisplayName, _type, _value);

      _valueControl = input.Control;
      _dataType = input.DataType;
      _isNullable = input.IsNullable;
      IInputControl ctl = input.Control as IInputControl;
      ctl.ValueChanged += ctl_ValueChanged;

      if( _dataType == DataType.Complex ) {
        (_valueControl as ComplexDataInputControl).DefineComplextType += cd_DefineComplextType;
      
      } else if( _dataType == DataType.Array ) {
        ( _valueControl as ArrayInputControl ).DefineComplextType += cd_DefineComplextType;
      }

      if( !_isNullable )
        rValue.Visibility = System.Windows.Visibility.Hidden;

      if( _valueControl != null ) {
        _valueControl.Margin = controlThickness; 
      }

      theGrid.Children.Add(_valueControl);
    }

    void ctl_ValueChanged(object sender, EventArgs e) {
      IInputControl ctl = sender as IInputControl;

      _value = ctl.RetrieveValue();

      if( _value == null ) {
        rValue.Fill = Brushes.Transparent;
      } else {
        rValue.Fill = HASVALUE_BRUSH;
      }

    }

    void cd_DefineComplextType(object sender, ComplexTypeEventArgs e) {
      OnDefineComplextType(sender, e.AttributeName, e.Type, e.Value);
    }

    public event EventHandler<ComplexTypeEventArgs> DefineComplextType;
    private void OnDefineComplextType(object sender, string attribName, Type t, object value) {
      if( DefineComplextType != null )
        DefineComplextType(sender, new ComplexTypeEventArgs(attribName, t, value));
    }



    private void AddTypeInfoControl(Type t) {
      Label l = new Label();
      l.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
      l.Foreground = Brushes.Gray; 
      l.Margin = new Thickness(0, 0, 4, 0);
      
      if( t.Name.StartsWith("Nullable") )
        l.Content = Nullable.GetUnderlyingType(t).Name + "?";
      
      else l.Content = t.Name;

      l.ToolTip = l.Content;

      theGrid.Children.Add(l);
    }


    T GetValueControl<T>() where T : Control, new() {
      return _valueControl as T;
    }

    void ValueChanged() {
      
      IInputControl ctl = _valueControl as IInputControl;
      ctl.UpdateValue(_value);

      if( _value == null ) {
        rValue.Fill = Brushes.Transparent;
      } else { 
        rValue.Fill = HASVALUE_BRUSH;
      }
    }

    private object GetValue() {
      Type t =  ( _type.Name.StartsWith("Nullable") ) ? Nullable.GetUnderlyingType(_type) : _type;

      IInputControl ctl = _valueControl as IInputControl;
      return ctl.RetrieveValue();
    }

    public string DisplayName {
      get { return (string)lbName.Content; }
      private set { lbName.Content = value; }
    }
    public object Value {
      get { return GetValue(); }
      set {
        _value = value;

        ValueChanged();
      }
    }

    private void rValue_KeyDown(object sender, KeyEventArgs e) {
    }

    private void rValue_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      if( _value != null )
        Value = null;
    }

  }
}
