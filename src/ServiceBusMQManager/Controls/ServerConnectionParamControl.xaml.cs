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

    public ServerConnectionParamControl(ServiceBusMQ.Manager.ServerConnectionParameter p, string value = null) {
      InitializeComponent();
      
      Param = p;
      Text.Content = p.DisplayName + ":";
      tbValue.Init(value ?? p.DefaultValue, typeof(string), true);

      tbValue.ValueChanged += tbValue_ValueChanged;
    }

    void tbValue_ValueChanged(object sender, EventArgs e) {
      OnValueChanged();
    }

    public string Value { 
      get { 
        return tbValue.RetrieveValue<string>();
      } 
    }

    public event EventHandler<EventArgs> ValueChanged;
    void OnValueChanged() {
      if( ValueChanged != null )
        ValueChanged(this, EventArgs.Empty);
    }

  }
}
