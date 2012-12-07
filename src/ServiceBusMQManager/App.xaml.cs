#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    App.xaml.cs
  Created: 2012-08-21

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ServiceBusMQ;

namespace ServiceBusMQManager {
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application {

    protected override void OnStartup(StartupEventArgs e) {

      if( e.Args.Length >= 2 ) {
        
        if( e.Args[0] == "--send" || e.Args[0] == "-s" ) {
          string cmdName = e.Args[1];
          
          var sys = SbmqSystem.Create();

          var cmd = sys.SavedCommands.Items.FirstOrDefault(c => c.DisplayName == cmdName);

          if( cmd != null ) {
            Console.WriteLine(string.Format("Sending Command '{0}'...", cmdName));
            sys.Manager.SendCommand(cmd.Server, cmd.Transport, cmd.Command);
          
          } else {
            Console.WriteLine(string.Format("No Command with name '{0}' found, exiting...", cmdName));
          }

          Application.Current.Shutdown();
          return;
        }
      
      }

      // Check if we are already running...
      Process proc = Process.GetCurrentProcess();
      if( Process.GetProcessesByName(proc.ProcessName).Length > 1 ) {
        Application.Current.Shutdown();
        return;
      }
      
      base.OnStartup(e);
    }


    private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {

#if !DEBUG
      MessageBox.Show(e.Exception.Message, "Exception Caught", MessageBoxButton.OK, MessageBoxImage.Error);
      e.Handled = true;
#endif

    }


  }
}
