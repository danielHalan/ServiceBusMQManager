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
  /// Interaction logic for ViewSubscriptionsWindow.xaml
  /// </summary>
  public partial class ViewSubscriptionsWindow : Window {

    SbmqSystem _sys = SbmqSystem.Instance;

    Dictionary<string, MessageSubscription> _allItems = new Dictionary<string, MessageSubscription>();

    ObservableCollection<MessageSubscription> _items = new ObservableCollection<MessageSubscription>();


    public ViewSubscriptionsWindow() {
      InitializeComponent();

      Topmost = _sys.UIState.AlwaysOnTop;


      LoadSubscriptionTypes();

      lvTypes.ItemsSource = _items;

      WindowTools.SetSortColumn(lvTypes, "Name");
    }

    private void LoadSubscriptionTypes() {

      foreach( var ms in _sys.Manager.GetMessageSubscriptions() ) {

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

    private void frmViewSubscriptions_Loaded(object sender, RoutedEventArgs e) {
      _sys.UIState.RestoreWindowState(this);
    }

    private void frmViewSubscriptions_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      _sys.UIState.StoreWindowState(this);
    }


  }
}
