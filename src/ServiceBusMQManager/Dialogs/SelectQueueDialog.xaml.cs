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
using System.ComponentModel;
using ServiceBusMQ.Configuration;

namespace ServiceBusMQManager.Dialogs {

  public class QueueListItem {
    public string Name { get; set; }
    public SelectQueueDialog.QueueAccess Access { get; set; }

    public string AccessString {
      get {
        return Access == SelectQueueDialog.QueueAccess.None ? "Permission Denied" : string.Empty;
      }
    }

    public override string ToString() {
      return Name;
    }
  }

  /// <summary>
  /// Interaction logic for SelectQueueDialog.xaml
  /// </summary>
  public partial class SelectQueueDialog : Window {

    public enum QueueAccess { Unknown, None, RW }


    IServiceBusDiscovery _disc;
    Dictionary<string, object> _serverSettings;

    public SelectQueueDialog(IServiceBusDiscovery discovery, ServerConfig3 serverCfg, string[] queueNames) {
      InitializeComponent();

      _disc = discovery;
      _serverSettings = serverCfg.ConnectionSettings;

      Topmost = SbmqSystem.UIState.AlwaysOnTop;

      var qItems = queueNames.Select(n => new QueueListItem() { Name = n, Access = QueueAccess.Unknown }).ToList();
      lbQueues.ItemsSource = qItems;

      BackgroundWorker bw = new BackgroundWorker();
      bw.DoWork += (s, e) => {

        foreach( var q in qItems ) {
          if( _disc.CanAccessQueue(_serverSettings, q.Name) )
            q.Access = QueueAccess.RW;
          else {
            q.Access = QueueAccess.None;
          }
        }
      };
      bw.RunWorkerCompleted += (s, e) => {
        lbQueues.Items.Refresh();
      };

      bw.RunWorkerAsync();

    }

    public List<string> SelectedQueueNames { get; set; }

    private void btnOK_Click(object sender, RoutedEventArgs e) {
      SelectedQueueNames = lbQueues.SelectedItems.Cast<QueueListItem>().Where( l => l.Access == QueueAccess.RW ).Select( l => l.Name).ToList();
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

      //if( btnOK.IsEnabled ) {
      //  SelectedQueueNames = lbQueues.SelectedItems.Cast<string>().ToList();
      //  DialogResult = true;
      //}

    }

    private void lbQueues_SelectionChanged(object sender, SelectionChangedEventArgs e) {

      if( lbQueues.SelectedItems.Cast<QueueListItem>().Any( q => q.Access == QueueAccess.RW) ) 
        btnOK.IsEnabled = true;
      else 
        btnOK.IsEnabled = false;
      

      if( lbQueues.SelectedItems.Cast<QueueListItem>().Any( q => q.Access == QueueAccess.None) ) 
        lbInfo.Content = "You don't have read access to some of the selected queues";
      else
        lbInfo.Content = string.Empty;
    }
  }
}
