#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    ComboBoxInputControl.xaml.cs
  Created: 2012-11-22

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace ServiceBusMQManager.Controls {
  /// <summary>
  /// Interaction logic for ComboBoxInputControl.xaml
  /// </summary>
  public partial class ComboBoxInputControl : UserControl, IInputControl {
    
    Type _type;

    public ComboBoxInputControl(Type type, object value) {
      InitializeComponent();

      _type = type;

      BindControl();

      UpdateValue(value);
    }

    private void BindControl() {
      Type t = ( _type.Name.StartsWith("Nullable") ) ? Nullable.GetUnderlyingType(_type) : _type;

      cb.ItemsSource = Enum.GetNames(t).Select(n => n.Replace('_', ' ')).ToArray();
    }

    public void UpdateValue(object value) {
      
      if( value != null ) {
        Type t = ( _type.Name.StartsWith("Nullable") ) ? Nullable.GetUnderlyingType(_type) : _type;

        cb.SelectedIndex = value != null ? ( (int)value ) : 0;
      }
    }


    public object RetrieveValue() {
      Type t = ( _type.Name.StartsWith("Nullable") ) ? Nullable.GetUnderlyingType(_type) : _type;
      
      try {
        return Enum.ToObject(t, cb.SelectedIndex);
      } catch { 
        return cb.SelectedIndex; 
      }
    }

    bool _isListItem;

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

      if( _isListItem ) {
        cb.Background = Brushes.Gray;
      }
    }


    public event EventHandler<EventArgs> ValueChanged;
    void OnValueChanged() {
      if( ValueChanged != null )
        ValueChanged(this, EventArgs.Empty);
    }

    private void cb_SelectionChanged(object sender, SelectionChangedEventArgs e) {

      OnValueChanged();
    }


  }
}
