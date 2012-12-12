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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using ServiceBusMQ;

namespace ServiceBusMQManager.Dialogs {
  /// <summary>
  /// Interaction logic for SelectQueueDialog.xaml
  /// </summary>
  public partial class SelectQueueDialog : Window {
    
    SbmqSystem _sys;
    string _server;

    public SelectQueueDialog(SbmqSystem system, string server, string[] queueNames) {
      InitializeComponent();

      _sys = system;
      _server = server;

      Topmost = system.UIState.AlwaysOnTop;

      lbQueues.ItemsSource = queueNames;
    }

    public string SelectedQueueName { get; set; }

    private void btnOK_Click(object sender, RoutedEventArgs e) {

      SelectedQueueName = lbQueues.SelectedItem as string;
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
        SelectedQueueName = lbQueues.SelectedItem as string;
        DialogResult = true;
      }

    }

    private void lbQueues_SelectionChanged(object sender, SelectionChangedEventArgs e) {
    
      if( lbQueues.SelectedItem != null ) {
        if( !_sys.Manager.CanAccessQueue(_server, lbQueues.SelectedItem as string ) ) {
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
