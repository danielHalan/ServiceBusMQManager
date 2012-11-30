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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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

  }
}
