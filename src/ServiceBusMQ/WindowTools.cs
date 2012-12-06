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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;

namespace ServiceBusMQ {

  public static class WindowTools {

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


  }
}
