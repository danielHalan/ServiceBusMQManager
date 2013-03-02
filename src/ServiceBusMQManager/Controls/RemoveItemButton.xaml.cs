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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
