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
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using ServiceBusMQ;
using ServiceBusMQ.Configuration;

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

      lbInfo.Opacity = 0;
      lbInfo.Visibility = System.Windows.Visibility.Hidden;

      Topmost = SbmqSystem.UIState.AlwaysOnTop;

    }

    private void BindServers() {

      cbServer.ItemsSource = _sys.Config.Servers;
      cbServer.DisplayMemberPath = "Name";
      cbServer.SelectedValuePath = "Name";

      cbServer.SelectedValue = _sys.Config.CurrentServer.Name;
      cbServer.IsEnabled = false;
    }



    private void frmViewSubscriptions_SourceInitialized(object sender, EventArgs e) {
      SbmqSystem.UIState.RestoreWindowState(this);
      SbmqSystem.UIState.RestoreControlState(cbServer, _sys.Config.CurrentServer.ConnectionSettings);

      //LoadSubscriptionTypes();

      BindServers();

      lvTypes.ItemsSource = _items;

      tbFilter.Focus();
      WindowTools.SetSortColumn(lvTypes, "Name");
    }

    private void LoadSubscriptionTypes(ServerConfig3 server = null, Action onCompleted = null) {
      if( server == null )
        server = ( cbServer.SelectedItem as ServerConfig3 );

      var currSrv = _sys.Config.CurrentServer;

      if( server.ServiceBus == currSrv.ServiceBus && server.ServiceBusVersion != currSrv.ServiceBusVersion ) {

        _allItems.Clear();
        _items.Clear();
        SetInfoText("Can't load subscriptions as Service Bus has a different version from the Currently Active");
        return;
      }


      var serverName = server.ConnectionSettings["server"];

      imgServerLoading.Visibility = System.Windows.Visibility.Visible;
      btnRefresh.Visibility = System.Windows.Visibility.Hidden;
      cbServer.IsEnabled = false;

      BackgroundWorker w = new BackgroundWorker();
      w.DoWork += (s, e) => { e.Result = _sys.GetMessageSubscriptions(server); };
      w.RunWorkerCompleted += (s, e) => {
        MessageSubscription[] subs = e.Result as MessageSubscription[];

        _allItems.Clear();
        _items.Clear();
        foreach( var ms in subs ) {

            var key = ms.FullName.ToLower() + " " + ms.Publisher.ToLower() + " " + ms.Subscriber.ToLower();
            if(!_allItems.ContainsKey(key))
                _allItems.Add(key, ms);

          _items.Add(ms);
        }

        imgServerLoading.Visibility = System.Windows.Visibility.Hidden;
        btnRefresh.Visibility = System.Windows.Visibility.Visible;
        cbServer.IsEnabled = true;

        Filter();

        if( onCompleted != null )
          onCompleted();
      };
      w.RunWorkerAsync();

    }

    void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e) {
      GridViewColumnHeader h = e.OriginalSource as GridViewColumnHeader;

      if( ( h != null ) && ( h.Role != GridViewColumnHeaderRole.Padding ) ) {
        WindowTools.SetSortColumn(lvTypes, ( h.Column.DisplayMemberBinding as Binding ).Path.Path);
      }

    }


    string _filter = null;
    private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e) {
      Filter(tbFilter.Text);
    }

    private void Filter(string str) {
      _filter = str.ToLower();

      Filter();
    }

    private void Filter() {
      if( _filter.IsValid() ) {

        foreach( var itm in _allItems.Where(t => !t.Key.Contains(_filter)) )
          _items.Remove(itm.Value);

        foreach( var itm in _allItems.Where(t => t.Key.Contains(_filter)) ) {
          if( _items.IndexOf(itm.Value) == -1 )
            _items.Add(itm.Value);
        }

      } else {

        foreach( var itm in _allItems ) {
          if( _items.IndexOf(itm.Value) == -1 )
            _items.Add(itm.Value);
        }
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
      SbmqSystem.UIState.StoreWindowState(this);
    }


    System.Threading.Timer _t;

    private void btnRefresh_Click(object sender, RoutedEventArgs e) {
      LoadSubscriptionTypes(null, () => SetInfoText("Subscription list refreshed"));
    }

    void SetInfoText(string text) {
      lbInfo.Content = text;
      lbInfo.Visibility = System.Windows.Visibility.Visible;

      DoubleAnimation da = new DoubleAnimation();
      da.From = 0;
      da.To = 1;
      da.Duration = new Duration(TimeSpan.FromMilliseconds(200));
      lbInfo.BeginAnimation(Label.OpacityProperty, da, HandoffBehavior.SnapshotAndReplace);

      _t = new System.Threading.Timer((o) => { ClearInfo(); }, null, 3000, Timeout.Infinite);
    }

    void ClearInfo() {
      Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {

        DoubleAnimation da = new DoubleAnimation();
        da.From = 1;
        da.To = 0;
        da.Duration = new Duration(TimeSpan.FromMilliseconds(700));
        da.Completed += (s,arg) => { lbInfo.Visibility = System.Windows.Visibility.Hidden; };
        lbInfo.BeginAnimation(Label.OpacityProperty, da, HandoffBehavior.SnapshotAndReplace);

        //lbInfo.Visibility = System.Windows.Visibility.Hidden;
      }));
    }

    private void cbServer_SelectionChanged(object sender, SelectionChangedEventArgs e) {

      if( e.AddedItems.Count > 0 )
        LoadSubscriptionTypes(e.AddedItems[0] as ServerConfig3);
    }


  }
}
