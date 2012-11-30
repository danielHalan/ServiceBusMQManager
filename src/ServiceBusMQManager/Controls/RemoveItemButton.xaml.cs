#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    RemoveItemButton.xaml.cs
  Created: 2012-11-30

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
  /// Interaction logic for RemoveItemButton.xaml
  /// </summary>
  public partial class RemoveItemButton : UserControl {

    readonly SolidColorBrush BORDER_LISTITEM = new SolidColorBrush(Color.FromRgb(201, 201, 201));

    
    public RemoveItemButton() {
      InitializeComponent();
    }

    private void btn_Click(object sender, RoutedEventArgs e) {

      if( Click != null )
        Click(this, e);

    }

    public event EventHandler<RoutedEventArgs> Click;
  }
}
