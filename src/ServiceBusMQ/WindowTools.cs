#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    WindowTools.cs
  Created: 2012-09-10

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ServiceBusMQ {

  public static class WindowTools {

    [Flags]
    public enum ExtendedWindowStyles { WS_EX_TOOLWINDOW = 0x00000080 }

    public enum GetWindowLongFields { GWL_EXSTYLE = ( -20 ) }


    public static CursorPosition GetCursorPosition(this Window window) {
      var x = Mouse.GetPosition(window).X;
      var y = Mouse.GetPosition(window).Y;
      CursorPosition pos = CursorPosition.Body;

      int THRESHOLD = 5;

      if( x < THRESHOLD && y < THRESHOLD )
        pos = CursorPosition.TopLeft;
      else if( x < THRESHOLD && y > window.Height - THRESHOLD )
        pos = CursorPosition.TopRight;

      else if( x < THRESHOLD && y > window.Height - THRESHOLD )
        pos = CursorPosition.BottomLeft;
      else if( x > window.Width - THRESHOLD && y > window.Height - THRESHOLD )
        pos = CursorPosition.BottomRight;

      else if( x < THRESHOLD )
        pos = CursorPosition.Left;
      else if( y < THRESHOLD )
        pos = CursorPosition.Top;
      else if( x > window.Width - THRESHOLD )
        pos = CursorPosition.Right;
      else if( y > window.Height - THRESHOLD )
        pos = CursorPosition.Bottom;

      return pos;
    }

    public static Cursor GetBorderCursor(this Window window) {

      switch( window.GetCursorPosition() ) {
        case CursorPosition.Top:
          return Cursors.SizeNS;

        case CursorPosition.Bottom:
          return Cursors.SizeNS;

        case CursorPosition.Left:
          return Cursors.SizeWE;

        case CursorPosition.Right:
          return Cursors.SizeWE;

        case CursorPosition.TopLeft:
          return Cursors.SizeNWSE;

        case CursorPosition.TopRight:
          return Cursors.SizeNESW;

        case CursorPosition.BottomLeft:
          return Cursors.SizeNESW;

        case CursorPosition.BottomRight:
          return Cursors.SizeNWSE;

        default:
          return Cursors.Arrow;

      }

    }

    public static void MoveOrResizeWindow(this Window window, MouseButtonEventArgs e) {
      CursorPosition pos = window.GetCursorPosition();

      if( e.LeftButton == MouseButtonState.Pressed ) {
        if( pos == CursorPosition.Body )
          window.DragMove();
        else ResizeWindow(window, pos);
      }

    }

    static Dictionary<Window, IntPtr> _winHandles = new Dictionary<Window,IntPtr>();

    private static void ResizeWindow(Window window, CursorPosition pos) {
      IntPtr handle;
      
      if( _winHandles.ContainsKey(window) ) 
        handle = _winHandles[window];
      else {
        HwndSource hs = (HwndSource)PresentationSource.FromVisual(window);
        _winHandles.Add(window, handle = hs.Handle);
      }

      Native.SendMessage(handle, Native.WM_SYSCOMMAND,
          (IntPtr)( 61440 + pos ), IntPtr.Zero);
    }


    static string _sortColumn;
    static ListSortDirection _sortDir;

    public static void SetSortColumn(ItemsControl list, string column) {
      _sortDir = column != _sortColumn ? ListSortDirection.Ascending : ( _sortDir == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending );
      ICollectionView dataView = CollectionViewSource.GetDefaultView(list.ItemsSource);

      dataView.SortDescriptions.Clear();
      SortDescription sd = new SortDescription(column, _sortDir);
      dataView.SortDescriptions.Add(sd);
      dataView.Refresh();

      _sortColumn = column;
    }


    /// <summary>
    /// Sleeps for the specified milisecs.
    /// </summary>
    /// <param name="milisec">The milisec.</param>
    public static void Sleep(int milisec) {

      for( int i = 0; i < ( milisec / 10 ); i++ ) {
        Thread.Sleep(10);
        DoEvents();
      }
    }

    /// <summary>
    /// Do the windows events.
    /// </summary>
    [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
    public static void DoEvents() {
      DispatcherFrame frame = new DispatcherFrame();
      Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
          new DispatcherOperationCallback(ExitFrame), frame);
      Dispatcher.PushFrame(frame);
    }
    static object ExitFrame(object f) {
      ( (DispatcherFrame)f ).Continue = false;

      return null;
    }


    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "SendMessage")]
    public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    public delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

    [DllImport("user32.dll")]
    public static extern int EnumWindows(EnumWindowsProc ewp, int lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    
    [DllImport("user32.dll")]
    public static extern IntPtr WindowFromPoint(int xPoint, int yPoint);

    public static void EnsureVisibility(this Window window) {

      if( !window.Topmost ) {

        var hnd = WindowTools.WindowFromPoint(Convert.ToInt32(window.Left + 5), Convert.ToInt32(window.Top + 5));

        if( hnd != new WindowInteropHelper(window).Handle ) {
          window.Topmost = true;
          window.Topmost = false;
        }

      }

    }

    public static void HideFromProgramSwitcher(this Window window) {
      IntPtr handle = new WindowInteropHelper(window).Handle;

      int exStyle = (int)GetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE);

      exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
      SetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
    }

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

    public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong) {
      int error = 0;
      IntPtr result = IntPtr.Zero;

      // Win32 SetWindowLong doesn't clear error on success
      SetLastError(0);

      if( IntPtr.Size == 4 ) {
        // use SetWindowLong
        Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
        error = Marshal.GetLastWin32Error();
        result = new IntPtr(tempResult);
      } else {
        // use SetWindowLongPtr
        result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
        error = Marshal.GetLastWin32Error();
      }

      if( ( result == IntPtr.Zero ) && ( error != 0 ) ) {
        throw new System.ComponentModel.Win32Exception(error);
      }

      return result;
    }

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
    private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

    private static int IntPtrToInt32(IntPtr intPtr) {
      return unchecked((int)intPtr.ToInt64());
    }

    [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
    public static extern void SetLastError(int dwErrorCode);

    public static Stream GetImageResourceStream(this Window window, string name) {
      return window.GetType().Assembly.GetManifestResourceStream("ServiceBusMQManager.Images." + name);
    }


  }
}
