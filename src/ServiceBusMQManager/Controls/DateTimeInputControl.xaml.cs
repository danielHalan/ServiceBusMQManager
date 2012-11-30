#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    DateTimeInputControl.xaml.cs
  Created: 2012-11-22

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

namespace ServiceBusMQManager.Controls {
  /// <summary>
  /// Interaction logic for DateTimeInputControl.xaml
  /// </summary>
  public partial class DateTimeInputControl : UserControl, IInputControl {
    private bool _isListItem;
    public DateTimeInputControl(object value) {
      InitializeComponent();

      try {
        tb.SelectedDate = Convert.ToDateTime(value);
      } catch { 
        tb.Text = value != null ? value.ToString() : string.Empty;
      }

    }

    private void Button_Click_1(object sender, RoutedEventArgs e) {
      tb.SelectedDate = DateTime.Now;
    }

    public void UpdateValue(object value) {
      if( value is DateTime )
        tb.SelectedDate = (DateTime)value;
      
      else if( value is DateTime? )
        tb.SelectedDate = (DateTime?)value;

      else tb.SelectedDate = value != null ? Convert.ToDateTime(value) : DateTime.Now;
    }


    public object RetrieveValue() {
      return (DateTime)tb.SelectedDate;
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
        tb.Background = Brushes.Gray;
        tb.BorderBrush = Brushes.DarkGray;
      }
    }


    public event EventHandler<EventArgs> ValueChanged;
    void OnValueChanged() {
      if( ValueChanged != null )
        ValueChanged(this, EventArgs.Empty);
    }

    private void tb_SelectedDateChanged(object sender, SelectionChangedEventArgs e) {
      OnValueChanged();
    }


  }
}
