#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    ServerConnectionParamControl.xaml.cs
  Created: 2013-10-11

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
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
  /// Interaction logic for ServerConnectionParamControl.xaml
  /// </summary>
  public partial class ServerConnectionParamControl : UserControl {

    public ServiceBusMQ.Manager.ServerConnectionParameter Param { get; set; }


    public ServerConnectionParamControl() {
      InitializeComponent();
    }

    public ServerConnectionParamControl(string label) {
      InitializeComponent();

      Text.Content = label;
    }

    public ServerConnectionParamControl(ServiceBusMQ.Manager.ServerConnectionParameter p, object value = null) {
      InitializeComponent();

      Param = p;
      Text.Content = p.DisplayName + ":";

      if( p.Type == ServiceBusMQ.Manager.ParamType.String ) {
        cbValue.Visibility = System.Windows.Visibility.Hidden;

        tbValue.Init(value ?? p.DefaultValue, typeof(string), p.Optional);

        tbValue.ValueChanged += tbValue_ValueChanged;
        tbValue.LostFocus += tbValue_LostFocus;

      } else if( p.Type == ServiceBusMQ.Manager.ParamType.Bool ) {

        tbValue.Visibility = System.Windows.Visibility.Hidden;

        cbValue.IsChecked = ((bool)value);
        cbValue.Checked += cbValue_Checked;
        cbValue.Unchecked += cbValue_Unchecked;
      }

      Req.Visibility = p.Optional ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
    }

    void cbValue_Unchecked(object sender, RoutedEventArgs e) {
      OnValueChanged();
    }

    void cbValue_Checked(object sender, RoutedEventArgs e) {
      OnValueChanged();
    }

    void tbValue_LostFocus(object sender, System.Windows.RoutedEventArgs e) {
      OnLostFocus(e);
    }

    void tbValue_ValueChanged(object sender, EventArgs e) {
      OnValueChanged();
    }

    public object Value {
      get {
        if( Param.Type == ServiceBusMQ.Manager.ParamType.String )
          return tbValue.RetrieveValue<string>();
        
        if( Param.Type == ServiceBusMQ.Manager.ParamType.Bool )
          return cbValue.IsChecked == true;

        return null;
      }

      set {
        if( Param.Type == ServiceBusMQ.Manager.ParamType.String )
          tbValue.UpdateValue(value);

        else if( Param.Type == ServiceBusMQ.Manager.ParamType.Bool )
          cbValue.IsChecked = (bool)value;
      }
    }

    public bool Validate() {
      return tbValue.Validate();
    }

    public event EventHandler<EventArgs> ValueChanged;
    void OnValueChanged() {
      if( ValueChanged != null )
        ValueChanged(this, EventArgs.Empty);
    }


    public new event RoutedEventHandler LostFocus;
    void OnLostFocus(RoutedEventArgs e) {
      if( LostFocus != null )
        LostFocus(this, e);
    }



  }
}
