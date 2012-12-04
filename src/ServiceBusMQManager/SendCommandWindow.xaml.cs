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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

    private HwndSource _hwndSource;

    string[] _asmPath;

    IMessageManager _mgr;

    List<CommandItem> _commands = new List<CommandItem>();

    ObservableCollection<SavedCommand> _recent = new ObservableCollection<SavedCommand>();

    public SendCommandWindow(SbmqSystem system) {
      InitializeComponent();

      _mgr = system.Manager;

      _asmPath = system.Config.CommandsAssemblyPaths;

      Topmost = system.UIState.AlwaysOnTop;

      BindCommands();


      savedCommands.Init(system.SavedCommands);

      //BindRecent();


      cbQueue.ItemsSource = system.Config.WatchCommandQueues;
      cbQueue.SelectedIndex = 0;

    }

    //private void BindRecent() {
    //  foreach( var cmd in _history.Items ) 
    //    _recent.Add(cmd);

    //  cbRecent.ItemsSource = _recent;
    //  cbRecent.DisplayMemberPath = "DisplayName";
    //  cbRecent.SelectedValuePath = "Command";

    //  cbRecent.SelectedValue = null;
    //}

    private void BindCommands() {
      var cmdTypes = _mgr.GetAvailableCommands(_asmPath);

      foreach( Type t in cmdTypes ) {
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


    private void Window_SourceInitialized_1(object sender, EventArgs e) {
      _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
    }

    private void cbCommands_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      
      if( !_recentUpdating ) {
        var cmd = cbCommands.SelectedItem as CommandItem;

        cmdAttrib.SetDataType(cmd.Type, null);

        savedCommands.SelectedItem = null;
      }
    }

    bool _recentUpdating = false;
    private void savedCommands_SavedCommandSelected(object sender, RoutedEventArgs e) {
      var recent = savedCommands.SelectedItem;

      _recentUpdating = true;
      try {
        if( !savedCommands.Updating ) {

          if( recent != null ) {
            var t = recent.Command.GetType();
            cmdAttrib.SetDataType(t, recent.Command);
            cbCommands.SelectedValue = t.FullName;

          } 

        }
      } finally {
        _recentUpdating = false;
      }
    }


    bool _isBusStarted = false;

    private void btnSend_Click(object sender, RoutedEventArgs e) {

      if( !_isBusStarted )
        _mgr.SetupBus(_asmPath);

      var cmd = cmdAttrib.CreateObject();
      var queue = (string)cbQueue.SelectedItem;

      _mgr.SendCommand(tbServer.Text, queue, cmd);

      savedCommands.CommandSent(cmd, _mgr.BusName, _mgr.BusQueueType, tbServer.Text, queue);
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e) {
      Close();
    }
    private void HandleCloseClick(Object sender, RoutedEventArgs e) {
      Close();
    }


    private void Button_Click_1(object sender, RoutedEventArgs e) {
      // TODO:
    }


    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {

      CursorPosition pos = this.GetCursorPosition();

      if( e.LeftButton == MouseButtonState.Pressed ) {
        if( pos == CursorPosition.Body )
          DragMove();
        else ResizeWindow(pos);
      }

    }

    private void ResizeWindow(CursorPosition pos) {
      Native.SendMessage(_hwndSource.Handle, Native.WM_SYSCOMMAND,
          (IntPtr)( 61440 + pos ), IntPtr.Zero);
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





  }

}
