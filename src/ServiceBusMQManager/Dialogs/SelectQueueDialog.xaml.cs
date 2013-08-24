#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    SelectQueueDialog.xaml.cs
  Created: 2012-12-05

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using ServiceBusMQ;
using ServiceBusMQ.Manager;
using System.Linq;

namespace ServiceBusMQManager.Dialogs {
  /// <summary>
  /// Interaction logic for SelectQueueDialog.xaml
  /// </summary>
  public partial class SelectQueueDialog : Window {

    IServiceBusDiscovery _disc;
    string _server;

    public SelectQueueDialog(IServiceBusDiscovery discovery, string server, string[] queueNames) {
      InitializeComponent();

      _disc = discovery;
      _server = server;

      Topmost = SbmqSystem.UIState.AlwaysOnTop;

      lbQueues.ItemsSource = queueNames;
    }

    public List<string> SelectedQueueNames { get; set; }

    private void btnOK_Click(object sender, RoutedEventArgs e) {
      SelectedQueueNames = lbQueues.SelectedItems.Cast<string>().ToList();
      DialogResult = true;
    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      this.MoveOrResizeWindow(e);
    }


    private void Window_MouseMove(object sender, MouseEventArgs e) {
      Cursor = this.GetBorderCursor();
    }
    
    private void HandleMaximizeClick(object sender, RoutedEventArgs e) {
      var s = WpfScreen.GetScreenFrom(this);

      this.Top = s.WorkingArea.Top;
      this.Height = s.WorkingArea.Height;
    }
    private void HandleCloseClick(Object sender, RoutedEventArgs e) {
      Close();
    }

    private void lbQueues_MouseDoubleClick(object sender, MouseButtonEventArgs e) {

      if( btnOK.IsEnabled ) {
        SelectedQueueNames = lbQueues.SelectedItems.Cast<string>().ToList();
        DialogResult = true;
      }

    }

    private void lbQueues_SelectionChanged(object sender, SelectionChangedEventArgs e) {
    
      if( lbQueues.SelectedItem != null ) {
        if( !_disc.CanAccessQueue(_server, lbQueues.SelectedItem as string ) ) {
          lbInfo.Content = "You don't have read access to queue " + lbQueues.SelectedItem;
          btnOK.IsEnabled = false;

        } else { 
          btnOK.IsEnabled = true;
          lbInfo.Content = string.Empty;
        }

      } else { 
        btnOK.IsEnabled = false;
        lbInfo.Content = string.Empty;
      }
    }


  }
}
