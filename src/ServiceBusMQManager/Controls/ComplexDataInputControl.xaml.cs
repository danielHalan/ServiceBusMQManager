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

    private  bool _showContentInName;
    private bool _isIgnoringClicks;

    public ComplexDataInputControl(string attributeName, Type type, object value) {
      InitializeComponent();

      ACTIVE_BRUSH = btn.Background;

      _attributeName = attributeName;
      _type = type;
      Value = value;

      

      _showContentInName = true;

      UpdateNameLabel();

    }

    private void UpdateNameLabel() {
      btn.Content = _showContentInName ? _type.GetDisplayName(_value).CutEnd(80) : _type.Name;
    }

    public event EventHandler<ComplexTypeEventArgs> DefineComplextType;
    private void OnDefineComplextType(string attribName, Type t, object value) {
      if( DefineComplextType != null )
        DefineComplextType(this, new ComplexTypeEventArgs(attribName, t, value));
    }


    private void Button_Click_1(object sender, RoutedEventArgs e) {

      if( !_isIgnoringClicks )
        OnDefineComplextType(_attributeName, _type, Value);
    }

    public void ShowComplextDataType() {
      OnDefineComplextType(_attributeName, _type, Value);
    }

    
    public bool ShowContentInName { 
      get { return _showContentInName; } 
      set { 
        _showContentInName = value;
        UpdateNameLabel();
      }  
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

    public bool IsIgnoringClicks {
      get { return _isIgnoringClicks; }
      set { _isIgnoringClicks = value; OnIsIgnoringClicksChanged(); }
    }

    private void OnIsIgnoringClicksChanged() {

    }

    private void ValueHasChanged() {

      UpdateNameLabel();

      OnValueChanged();
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
      
      _showContentInName = true;
      UpdateNameLabel();

      btn.IsEnabled = !_isListItem;
    }


    public event EventHandler<EventArgs> ValueChanged;
    void OnValueChanged() {
      if( ValueChanged != null )
        ValueChanged(this, EventArgs.Empty);
    }


   
  }
}
