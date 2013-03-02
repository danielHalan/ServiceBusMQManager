#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    CheckBoxInputControl.xaml.cs
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
using System.Windows.Media;

namespace ServiceBusMQManager.Controls {
  /// <summary>
  /// Interaction logic for CheckBoxInputControl.xaml
  /// </summary>
  public partial class CheckBoxInputControl : UserControl, IInputControl {
    private bool _isListItem;
    public CheckBoxInputControl(object value) {
      InitializeComponent();
    
      UpdateValue(value);
    }



    public void UpdateValue(object value) {
      try { 
        c.IsChecked = value != null && Convert.ToBoolean(value);
      } catch { }
    }


    public object RetrieveValue() {
      return c.IsChecked;
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

      if( _isListItem ) {
        Background = Brushes.Gray;
      }
    }

    public event EventHandler<EventArgs> ValueChanged;
    void OnValueChanged() {
      if( ValueChanged != null )
        ValueChanged(this, EventArgs.Empty);
    }

    private void c_Checked(object sender, RoutedEventArgs e) {
      OnValueChanged();
    }

  }
}
