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
      public string Name { get; set; }
    }

    private HwndSource _hwndSource;

    string[] _asmPath;

    IMessageManager _mgr;
    CommandHistoryManager _history;

    List<CommandItem> _commands = new List<CommandItem>();

    ObservableCollection<SavedCommand> _recent = new ObservableCollection<SavedCommand>();

    public SendCommandWindow(SbmqSystem system) {
      InitializeComponent();

      _mgr = system.Manager;
      _history = system.HistoryManager;

      _asmPath = system.Config.CommandsAssemblyPaths;


      BindCommands();

      BindRecent();


      cbQueue.ItemsSource = system.Config.WatchCommandQueues;
      cbQueue.SelectedIndex = 0;

    }

    private void BindRecent() {
      foreach( var cmd in _history.Items ) 
        _recent.Add(cmd);

      cbRecent.ItemsSource = _recent;
      cbRecent.DisplayMemberPath = "DisplayName";
      cbRecent.SelectedValuePath = "Command";
      
      cbRecent.SelectedValue = null;
    }

    private void BindCommands() {
      var cmdTypes = _mgr.GetAvailableCommands(_asmPath);

      foreach( Type t in cmdTypes ) {
        var cmd = new CommandItem();
        cmd.Type = t;
        cmd.Name = string.Format("{0} ({1})", t.Name, t.Namespace);

        _commands.Add(cmd);
      }

      cbCommands.ItemsSource = _commands;
      cbCommands.DisplayMemberPath = "Name";
      cbCommands.SelectedValuePath = "Name";
      cbCommands.SelectedValue = null;
    }


    private void Window_SourceInitialized_1(object sender, EventArgs e) {
      _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
    }

    private void cbCommands_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      var cmd = cbCommands.SelectedItem as CommandItem;

      cmdAttrib.SetDataType(cmd.Type, null);
    }
    private void cbRecent_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      var recent = cbRecent.SelectedItem as SavedCommand;

      cmdAttrib.SetDataType(recent.Command.GetType(), recent.Command);
    }


    bool _isBusStarted = false;

    private void btnSend_Click(object sender, RoutedEventArgs e) {

      if( !_isBusStarted )
        _mgr.SetupBus(_asmPath);

      var cmd = cmdAttrib.CreateObject();
      var queue = (string)cbQueue.SelectedItem;

      _mgr.SendCommand(tbServer.Text, queue, cmd);

      var sentCmd = _history.CommandSent(cmd, _mgr.BusName, _mgr.BusQueueType, tbServer.Text, queue);

      int pos = _recent.IndexOf(sentCmd);
      if( pos == -1 ) {
        _recent.Insert(0, sentCmd);
      
        if( cbRecent.SelectedItem != sentCmd )
          cbRecent.SelectedValue = sentCmd.Command;
      
      } else if( pos != 0 ) { 
        _recent.Move(_recent.IndexOf(sentCmd), 0);
      }

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




  }

}
