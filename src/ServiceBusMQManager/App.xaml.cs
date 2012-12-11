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
using System.Reflection;
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
      Process currProc = Process.GetCurrentProcess(); 
      Process existProc = Process.GetProcessesByName(currProc.ProcessName).Where(p => p.Id != currProc.Id).FirstOrDefault();
      if( existProc != null ) {
        try {
          // Show the already started SBMQM
          WindowTools.EnumWindows( new WindowTools.EnumWindowsProc( (hwnd, lparam) => 
            {
              uint procId;
              WindowTools.GetWindowThreadProcessId(hwnd, out procId);
              if( procId == existProc.Id ) {
                if( WindowTools.SendMessage(hwnd, ServiceBusMQManager.MainWindow.WM_SHOWWINDOW, 0, 0) == 1 )
                  return false;
              }
              return true;
            }), 0);
          
        } finally {
          Application.Current.Shutdown();
        }
          return;
      }

      base.OnStartup(e);
    }



    static AssemblyName _info;
    public static AssemblyName Info {
      get {
        if( _info == null )
          _info = Assembly.GetExecutingAssembly().GetName();

        return _info;
      }
    }


    private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {

#if !DEBUG
      MessageBox.Show(e.Exception.Message +"\n\r" + e.Exception.StackTrace, "Exception Caught", MessageBoxButton.OK, MessageBoxImage.Error);
      e.Handled = true;
#endif

    }



  }
}
