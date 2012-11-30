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

    string[] _asmPath;

    IMessageManager _mgr;

    List<CommandItem> _commands = new List<CommandItem>();

    public SendCommandWindow(SbmqSystem system) {
      InitializeComponent();

      _mgr = system.Manager;

      _asmPath = system.Config.CommandsAssemblyPaths;

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

      cbQueue.ItemsSource = system.Config.WatchCommandQueues;
      cbQueue.SelectedIndex = 0;

    }

    private void cbCommands_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      var cmd = cbCommands.SelectedItem as CommandItem;

      cmdAttrib.SetDataType(cmd.Type, null);
    }


    bool _isBusStarted = false;

    private void btnSend_Click(object sender, RoutedEventArgs e) {

      if( !_isBusStarted )
        _mgr.SetupBus(_asmPath);

      _mgr.SendCommand(tbServer.Text, (string)cbQueue.SelectedItem, cmdAttrib.CreateObject());
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e) {
      Close();
    }

    private void Button_Click_1(object sender, RoutedEventArgs e) {
      // TODO:
    }






  }

}
