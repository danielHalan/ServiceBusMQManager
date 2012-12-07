#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    ConfigWindow.xaml.cs
  Created: 2012-12-02

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
using Ookii.Dialogs.Wpf;
using ServiceBusMQ;
using ServiceBusMQ.Manager;
using ServiceBusMQManager.Controls;

namespace ServiceBusMQManager.Dialogs {
  /// <summary>
  /// Interaction logic for ConfigWindow.xaml
  /// </summary>
  public partial class ConfigWindow : Window {

    SbmqSystem _sys;
    SystemConfig1 _config;
    MessageBusFactory.ServiceBusManagerType[] _managerTypes;
    string[] _allQueueNames;


    public ConfigWindow(SbmqSystem system) {
      InitializeComponent();

      Topmost = system.UIState.AlwaysOnTop;

      _sys = system;
      _config = system.Config;

      _allQueueNames = _sys.Manager.GetAllAvailableQueueNames(_config.ServerName);

      _managerTypes = MessageBusFactory.AvailableServiceBusManagers();

      cbServiceBus.ItemsSource = _managerTypes;
      cbServiceBus.DisplayMemberPath = "Name";
      cbServiceBus.SelectedValuePath = "Name";

      cbServiceBus.SelectedIndex = 0;

      cbTransport.ItemsSource = _managerTypes[0].QueueTypes;
      cbTransport.SelectedIndex = 0;

      cShowOnNewMessages.IsChecked = _config.ShowOnNewMessages;
      
      queueCommands.BindItems(_config.WatchCommandQueues);
      queueEvents.BindItems(_config.WatchEventQueues);
      queueMessages.BindItems(_config.WatchMessageQueues);
      queueErrors.BindItems(_config.WatchErrorQueues);

      asmPaths.BindItems(_config.CommandsAssemblyPaths);

      tbServer.Init(_config.ServerName, typeof(string), true);
      tbInterval.Init(_config.MonitorInterval, typeof(int), false);
      tbNamespace.Init(_config.CommandDefinition.NamespaceContains, typeof(string), true);

      tbCmdInherits.Text = _config.CommandDefinition.InheritsType;


      HandleHeight();

    }

    private void frmConfig_SourceInitialized(object sender, EventArgs e) {
      _sys.UIState.RestoreWindowState(this);
    }


    private void HandleHeight() {
      var s = WpfScreen.GetScreenFrom(this);

      if( this.Height > s.WorkingArea.Height ) {
        this.Top = s.WorkingArea.Top;
        this.Height = s.WorkingArea.Height;
      }
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


    private void StringListControl_AddItem_1(object sender, AddItemRoutedEventArgs e) {
      StringListControl s = sender as StringListControl;



      SelectQueueDialog dlg = new SelectQueueDialog(_sys, _allQueueNames.Except( s.GetItems().ToList() ).ToArray() );
      dlg.Title = "Select " + s.Title.Remove(s.Title.Length-1);
      dlg.Owner = this;

      if( dlg.ShowDialog() == true ) {
        e.Handled = true;
        e.Item = dlg.SelectedQueueName;
      }


    }

    private void StringListControl_SizeChanged(object sender, SizeChangedEventArgs e) {
      StringListControl s = sender as StringListControl;
      var grid = s.Parent as Grid;

      int ROW = 3;

      double[] max = new double[2] { 0, 0 };
      foreach( var c in grid.Children.OfType<StringListControl>().Where(c => c.Name.StartsWith("queue")) ) {

        if( !double.IsNaN(c.Height) ) {
          int row = Grid.GetRow(c) - ROW;
          max[row] = Math.Max(max[row], c.Height);
        }
      }

      for(int i = 0; i < max.Length; i++) {
        if( max[i] > 0 )
          grid.RowDefinitions[ROW + i].Height = new GridLength(max[i] );
      }

    }

    private void asmPaths_SizeChanged(object sender, SizeChangedEventArgs e) {
      StringListControl s = sender as StringListControl;
      var grid = s.Parent as Grid;
      
      double max = 0;

      if( !double.IsNaN(s.Height) ) {
        max = Math.Max(max, s.Height);
      }

      if( max > 0 )
        grid.RowDefinitions[6].Height = new GridLength(max);
    }

    private void asmPaths_AddItem(object sender, AddItemRoutedEventArgs e) {

      VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
      dialog.Description = "Please select Command Assembly Folder";
      dialog.UseDescriptionForTitle = true;
      
      if( (bool)dialog.ShowDialog(this) ) {
      
        e.Item = dialog.SelectedPath;
        e.Handled = true;
      }

    }

    private void btnOK_Click(object sender, RoutedEventArgs e) {
      
      SaveConfig();
      
      DialogResult = true;
    }

    private void SaveConfig() {
      
      _config.MessageBus = cbServiceBus.SelectedValue as string;
      _config.MessageBusQueueType = cbTransport.SelectedItem as string;

      _config.ServerName = tbServer.RetrieveValue() as string;
      _config.MonitorInterval = (int)tbInterval.RetrieveValue();

      _config.ShowOnNewMessages = cShowOnNewMessages.IsChecked == true;

      _config.WatchCommandQueues = queueCommands.GetItems();
      _config.WatchEventQueues = queueEvents.GetItems();
      _config.WatchMessageQueues = queueMessages.GetItems();
      _config.WatchErrorQueues = queueErrors.GetItems();

      _config.CommandDefinition.InheritsType = tbCmdInherits.Text;
      _config.CommandDefinition.NamespaceContains = tbNamespace.RetrieveValue() as string;

      _config.CommandsAssemblyPaths = asmPaths.GetItems();

      _config.Save();

    }

    private void Button_Click_1(object sender, RoutedEventArgs e) {
      SelectDataTypeDialog dlg = new SelectDataTypeDialog(_sys, asmPaths.GetItems());
      dlg.Owner = this;

      if( dlg.ShowDialog() == true ) {
        
        tbCmdInherits.Text = dlg.SelectedType.QualifiedName;
      }

    }

    private void frmConfig_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      _sys.UIState.StoreWindowState(this);
    }

    private void tbServer_LostFocus_1(object sender, RoutedEventArgs e) {
      var name = tbServer.RetrieveValue() as string;
      try {
        _allQueueNames = _sys.Manager.GetAllAvailableQueueNames(name);

      } catch { 
        MessageBox.Show("Can not access Message Queues at Server " + name, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        tbServer.UpdateValue(_sys.Config.ServerName);
      }

    }


  }
}
