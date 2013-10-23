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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using ServiceBusMQ;
using ServiceBusMQManager.Dialogs;

namespace ServiceBusMQManager {
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application {

    enum ArgType { Unknown, Send, Silent, Minimized }

    class Arg {
      public ArgType Type { get; set; }
      public string Param { get; set; }

      public Arg(ArgType type, string param) {
        Type = type;
        Param = param;
      }
    }


    bool _silent;

    public App() {
      StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
    }


    [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
    [STAThread]
    static void Main() {
      App app = new App();
      app.InitializeComponent();
      app.Run();
    }


    [DllImport("Kernel32.dll")]
    public static extern bool AttachConsole(int processId);

    public static bool StartMinimized = false;

    protected override void OnStartup(StartupEventArgs e) {
      List<Arg> args = ProcessArgs(e.Args);

      StartMinimized = args.Any(a => a.Type == ArgType.Minimized);

      _silent = args.Any(a => a.Type == ArgType.Silent);

      var arg = args.FirstOrDefault(a => a.Type == ArgType.Send);
      if( arg != null ) {
        AttachConsole(-1);

        PrintHeader();

        string[] cmds = arg.Param.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

        var sys = SbmqSystem.Create();
        try {
          foreach( var cmd in cmds ) {
            var itm = sys.SavedCommands.Items.FirstOrDefault(c => c.DisplayName == cmd);

            if( itm != null ) {
              Out(string.Format("Sending Command '{0}'...", cmd));
              sys.SendCommand(itm.SentCommand.ConnectionStrings, itm.SentCommand.Queue, itm.SentCommand.Command);

            } else {
              Out(string.Format("No Command with name '{0}' found, exiting...", cmd));
            }
          }

        } finally {
          sys.Manager.Terminate();
        }

        Application.Current.Shutdown(0);
        return;
      }

      if( args.Where( a => a.Type != ArgType.Minimized ).Count() > 0 ) {
        AttachConsole(-1);
        PrintHeader();
        PrintHelp();

        Application.Current.Shutdown(0);
        return;
      }


      // Check if we are already running...
      Process currProc = Process.GetCurrentProcess();
      Process existProc = Process.GetProcessesByName(currProc.ProcessName).Where(p => p.Id != currProc.Id).FirstOrDefault();
      if( existProc != null ) {
        try {
          // Show the already started SBMQM
          WindowTools.EnumWindows(new WindowTools.EnumWindowsProc((hwnd, lparam) => {
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

    private void PrintHeader() {
      var ver = App.Info.Version;
      Out(string.Empty);
      Out("===============================================================================");
      Out("  Service Bus MQ Manager v{0}.{1} ".With(ver.Major, ver.Minor.ToString("D2")));
      Out("  (c)2012-2013 ITQ.COM, Daniel Halan http://halan.se");
      Out("===============================================================================");
    }

    private void PrintHelp() {

      Out(" Command Line: ServiceBusMQManager.exe --send <recentCommandName;recentCommandName> [-s] [-m]");

      Out("");
      Out("  --send   = Send one or more saved command, ");
      Out("             semicolon separated and specified by name set in Send Command Dialog");
      Out("  -s       = Silent Mode");
      Out("  -m       = Start minimized");


    }

    private void Out(string str) {
      if( !_silent )
        Console.WriteLine(str);
    }

    private List<Arg> ProcessArgs(string[] args) {
      List<Arg> r = new List<Arg>();

      try {

        for( int i = 0; i < args.Length; i++ )
          switch( args[i] ) {
            case "--send": r.Add(new Arg(ArgType.Send, args[++i])); break;
            case "-s": r.Add(new Arg(ArgType.Silent, null)); break;
            case "-m": r.Add(new Arg(ArgType.Minimized, null)); break;
            default: r.Add(new Arg(ArgType.Unknown, null)); break;
          }

      } catch( Exception e ) {
        Console.WriteLine("Failed when parsing arguments, " + e.Message);
      }

      return r;
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
      if( e.Exception is WarningException ) {
        var warn  = e.Exception as WarningException;
        MessageDialog.Show(MessageType.Warn, warn.Message, warn.Content);
        e.Handled = true;

      } else { 
        MessageDialog.Show(MessageType.Error,null, e.Exception);
        e.Handled = true;
      }
#endif

    }



  }
}
