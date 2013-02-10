#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    SendCommandWindow.xaml.cs
  Created: 2012-11-19

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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ServiceBusMQ;
using ServiceBusMQ.Manager;
using ServiceBusMQManager.Dialogs;

namespace ServiceBusMQManager {
  /// <summary>
  /// Interaction logic for SendCommandWindow.xaml
  /// </summary>
  public partial class SendCommandWindow : Window {

    class CommandItem {
      public Type Type { get; set; }
      public string DisplayName { get; set; }

      public string FullName { get; set; }
    }


    string[] _asmPath;

    SbmqSystem _sys;

    ObservableCollection<CommandItem> _commands = new ObservableCollection<CommandItem>();

    bool _recentUpdating = false;
    bool _isBusStarted = false;


    public SendCommandWindow(SbmqSystem system) {
      InitializeComponent();

      _sys = system;

      _asmPath = system.Config.CommandsAssemblyPaths;

      Topmost = SbmqSystem.UIState.AlwaysOnTop;

      BindCommands();

      savedCommands.Init(system.SavedCommands);

      BindServers();

      cmdAttrib.SendCommandManager = _sys.Manager as ISendCommand;
    }

    private void BindServers() {

      cbServer.ItemsSource = _sys.Config.Servers;
      cbServer.DisplayMemberPath = "Name";
      cbServer.SelectedValuePath = "Name";
      cbServer.SelectedIndex = 0;

      var s = _sys.Config.Servers[0];
      cbQueue.ItemsSource = s.MonitorQueues.Where( q => q.Type == ServiceBusMQ.Model.QueueType.Command);
      cbQueue.SelectedIndex = 0;
    }


    private void Window_SourceInitialized(object sender, EventArgs e) {
      SbmqSystem.UIState.RestoreControlState(cbServer, cbServer.SelectedValue);
      SbmqSystem.UIState.RestoreControlState(cbQueue, cbQueue.SelectedValue);
      if( cbQueue.SelectedIndex == -1 )
        cbQueue.SelectedIndex = 0;

      SbmqSystem.UIState.RestoreWindowState(this);
    }
    private void frmSendCommand_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      SbmqSystem.UIState.StoreControlState(cbServer);
      SbmqSystem.UIState.StoreControlState(cbQueue);

      SbmqSystem.UIState.StoreWindowState(this);

      //savedCommands.Unload();
    }


    private void BindCommands() {
      var cmdTypes = _sys.GetAvailableCommands(_asmPath);

      _commands.Clear();

      foreach( Type t in cmdTypes.OrderBy(t => t.Name) ) {
        var cmd = new CommandItem();
        cmd.Type = t;
        cmd.DisplayName = string.Format("{0} ({1})", t.Name, t.Namespace);
        cmd.FullName = t.FullName;

        _commands.Add(cmd);
      }

      cbCommands.ItemsSource = _commands;
      cbCommands.DisplayMemberPath = "DisplayName";
      cbCommands.SelectedValuePath = "FullName";
      cbCommands.SelectedValue = null;
    }
    private void UpdateSendButton() {
      btnSend.IsEnabled = cmdAttrib.IsValid && cbQueue.SelectedIndex != -1;
    }



    private void cbCommands_SelectionChanged(object sender, SelectionChangedEventArgs e) {

      if( !_recentUpdating ) {
        var cmd = cbCommands.SelectedItem as CommandItem;

        if( cmd != null ) {
          cmdAttrib.SetDataType(cmd.Type, null);
        } else cmdAttrib.Clear();

        savedCommands.SelectedItem = null;

        UpdateSendButton();
      }
    }
    private void savedCommands_SavedCommandSelected(object sender, RoutedEventArgs e) {
      var recent = savedCommands.SelectedItem;

      _recentUpdating = true;
      try {
        if( !savedCommands.Updating ) {

          if( recent != null ) {
            var t = recent.SentCommand.Command.GetType();
            cmdAttrib.SetDataType(t, recent.SentCommand.Command);
            cbCommands.SelectedValue = t.FullName;

          }

        }
      } finally {
        _recentUpdating = false;

        UpdateSendButton();
      }
    }

    void DoSetupBus(object sender, DoWorkEventArgs e) {

      if( !_isBusStarted ) {
        _sys.SetupBus(_asmPath);

        _isBusStarted = true;
      }

    }
    void DoSendCommand(object sender, RunWorkerCompletedEventArgs e) {

      try {
        var queue = cbQueue.SelectedItem as string;
        _sys.SendCommand(cbServer.SelectedValue as string, queue, _cmd);

        savedCommands.CommandSent(_cmd, _sys.Manager.BusName, _sys.Manager.BusQueueType, cbServer.SelectedValue as string, queue);

        Close();

      } catch( Exception ex ) {
        btnSend.IsEnabled = true;
        throw ex;
      }
    }

    object _cmd;

    private void btnSend_Click(object sender, RoutedEventArgs e) {



      if( btnSend.IsEnabled ) {
        btnSend.IsEnabled = false;

        _cmd = cmdAttrib.CreateObject();

        if( _cmd != null ) {

          var thread = new BackgroundWorker();
          thread.DoWork += DoSetupBus;
          thread.RunWorkerCompleted += DoSendCommand;

          thread.RunWorkerAsync(cbQueue.SelectedItem);
        
        } else btnSend.IsEnabled = true;

      }
    }




    private void btnCancel_Click(object sender, RoutedEventArgs e) {
      Close();
    }
    private void HandleCloseClick(Object sender, RoutedEventArgs e) {
      Close();
    }


    private void btnOpenConfig_Click(object sender, RoutedEventArgs e) {
      ConfigWindow dlg = new ConfigWindow(_sys, true);
      dlg.Owner = this;

      if( dlg.ShowDialog() == true ) {
        _asmPath = _sys.Config.CommandsAssemblyPaths;

        BindCommands();
      }
    }


    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      this.MoveOrResizeWindow(e);
    }
    private void Window_MouseMove(object sender, MouseEventArgs e) {
      var pos = this.GetCursorPosition();

      if( pos != CursorPosition.Left && pos != CursorPosition.Right )
        Cursor = this.GetBorderCursor();
      else Cursor = Cursors.Arrow;
    }
    private void HandleMaximizeClick(object sender, RoutedEventArgs e) {
      var s = WpfScreen.GetScreenFrom(this);

      this.Top = s.WorkingArea.Top;
      this.Height = s.WorkingArea.Height;
    }

    private void savedCommands_EnterEditMode(object sender, RoutedEventArgs e) {
      cbCommands.IsEnabled = false;
      btnSend.IsEnabled = false;
    }
    private void savedCommands_ExitEditMode(object sender, RoutedEventArgs e) {
      cbCommands.IsEnabled = true;
      btnSend.IsEnabled = true;
    }

    private void cbServer_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      ServerConfig s = cbServer.SelectedItem as ServerConfig;

      if( s != null ) {
        cbQueue.ItemsSource = s.WatchCommandQueues;
      }

      if( cbQueue.Items.Count > 0 )
        cbQueue.SelectedIndex = 0;

    }



  }

}
