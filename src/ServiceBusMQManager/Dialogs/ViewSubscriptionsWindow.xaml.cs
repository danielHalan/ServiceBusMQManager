#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    ViewSubscriptionsWindow.xaml.cs
  Created: 2012-12-06

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using ServiceBusMQ;

namespace ServiceBusMQManager.Dialogs {
  /// <summary>
  /// Interaction logic for ViewSubscriptionsWindow.xaml
  /// </summary>
  public partial class ViewSubscriptionsWindow : Window {

    SbmqSystem _sys;

    Dictionary<string, MessageSubscription> _allItems = new Dictionary<string, MessageSubscription>();

    ObservableCollection<MessageSubscription> _items = new ObservableCollection<MessageSubscription>();


    public ViewSubscriptionsWindow(SbmqSystem system) {
      InitializeComponent();

      _sys = system;

      Topmost = _sys.UIState.AlwaysOnTop;

      tbServer.Init(_sys.Config.ServerName, typeof(string), false);

      LoadSubscriptionTypes();

      lvTypes.ItemsSource = _items;

      WindowTools.SetSortColumn(lvTypes, "Name");
    }


    private void frmViewSubscriptions_SourceInitialized(object sender, EventArgs e) {
      _sys.UIState.RestoreWindowState(this);

      tbFilter.Focus();
    }

    private void LoadSubscriptionTypes() {
      var subs = _sys.Manager.GetMessageSubscriptions(tbServer.RetrieveValue<string>());

      _allItems.Clear();
      _items.Clear();
      foreach( var ms in subs ) {

        _allItems.Add(ms.FullName.ToLower() + " " + ms.Publisher.ToLower() + " " + ms.Subscriber.ToLower(), ms);

        _items.Add(ms);
      }

    }

    void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e) {
      GridViewColumnHeader h = e.OriginalSource as GridViewColumnHeader;

      if( (h != null) && (h.Role != GridViewColumnHeaderRole.Padding)) {
        WindowTools.SetSortColumn(lvTypes, (h.Column.DisplayMemberBinding as Binding).Path.Path );
      }

    }


    private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e) {
      Filter(tbFilter.Text);
    }

    private void Filter(string str) {
      str = str.ToLower();

      foreach( var itm in _allItems.Where(t => !t.Key.Contains(str)) )
        _items.Remove(itm.Value);

      foreach( var itm in _allItems.Where(t => t.Key.Contains(str)) ) {
        if( _items.IndexOf(itm.Value) == -1 )
          _items.Add(itm.Value);
      }

    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      this.MoveOrResizeWindow(e);
    }

    private void Window_MouseMove(object sender, MouseEventArgs e) {
      Cursor = this.GetBorderCursor();
    }

    private void HandleMaximizeClick(object sender, RoutedEventArgs e) {
      if( WindowState != System.Windows.WindowState.Maximized )
        WindowState = System.Windows.WindowState.Maximized;
      else WindowState = System.Windows.WindowState.Normal;
    }
    private void HandleCloseClick(Object sender, RoutedEventArgs e) {
      Close();
    }

    private void btnOK_Click(object sender, RoutedEventArgs e) {
      Close();
    }

    private void frmViewSubscriptions_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      _sys.UIState.StoreWindowState(this);
    }

    private void TextInputControl_LostFocus_1(object sender, RoutedEventArgs e) {

      try {
        LoadSubscriptionTypes();
        lbInfo.Content = string.Empty;

      } catch { 
        lbInfo.Content = "Could not access server";
        tbServer.UpdateValue(_sys.Config.ServerName);
      }

    }


    System.Threading.Timer _t;

    private void btnRefresh_Click(object sender, RoutedEventArgs e) {
      LoadSubscriptionTypes();

      if( _sys.Config.ServerName == (string)tbServer.RetrieveValue() ) {  
        lbInfo.Content = "Subscription list refreshed";
        _t = new System.Threading.Timer( (o) => { ClearInfo(); }, null, 2000, Timeout.Infinite);
      }
    }

    void ClearInfo() {
      Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
        lbInfo.Content = "";
      }));
    }


  }
}
