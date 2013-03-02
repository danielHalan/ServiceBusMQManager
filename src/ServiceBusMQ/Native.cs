#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    Native.cs
  Created: 2012-09-08

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ServiceBusMQ {


  public sealed class Native {

    public static readonly UInt32 WM_SYSCOMMAND = 0x112;

    [DllImport("dwmapi.dll", PreserveSig = true)]
    public static extern Int32 DwmSetWindowAttribute(
        IntPtr hwnd,
        Int32 attr,
        ref Int32 attrValue,
        Int32 attrSize);

    [DllImport("dwmapi.dll")]
    public static extern Int32 DwmExtendFrameIntoClientArea(
        IntPtr hWnd,
        ref MARGINS pMarInset);

    [DllImport("user32")]
    public static extern Boolean GetMonitorInfo(
        IntPtr hMonitor,
        MONITORINFO lpmi);

    [DllImport("User32")]
    public static extern IntPtr MonitorFromWindow(
        IntPtr handle,
        Int32 flags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(
        IntPtr hWnd,
        UInt32 msg,
        IntPtr wParam,
        IntPtr lParam);

    [DebuggerStepThrough]
    public static IntPtr WindowProc(
        IntPtr hwnd,
        Int32 msg,
        IntPtr wParam,
        IntPtr lParam,
        ref Boolean handled) {
      switch( msg ) {
        case 0x0024:
          WmGetMinMaxInfo(hwnd, lParam);
          handled = true;
          break;
      }

      return (IntPtr)0;
    }

    public static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam) {
      MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

      // Adjust the maximized size and position to fit the work area 
      // of the correct monitor.
      Int32 MONITOR_DEFAULTTONEAREST = 0x00000002;

      IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
      if( monitor != IntPtr.Zero ) {
        MONITORINFO monitorInfo = new MONITORINFO();
        GetMonitorInfo(monitor, monitorInfo);

        RECT rcWorkArea = monitorInfo.m_rcWork;
        RECT rcMonitorArea = monitorInfo.m_rcMonitor;

        mmi.m_ptMaxPosition.m_x = Math.Abs(rcWorkArea.m_left - rcMonitorArea.m_left);
        mmi.m_ptMaxPosition.m_y = Math.Abs(rcWorkArea.m_top - rcMonitorArea.m_top);

        mmi.m_ptMaxSize.m_x = Math.Abs(rcWorkArea.m_right - rcWorkArea.m_left);
        mmi.m_ptMaxSize.m_y = Math.Abs(rcWorkArea.m_bottom - rcWorkArea.m_top);
      }

      Marshal.StructureToPtr(mmi, lParam, true);
    }

    public static void ShowShadowUnderWindow(IntPtr intPtr) {
      MARGINS marInset = new MARGINS();
      marInset.m_bottomHeight = -1;
      marInset.m_leftWidth = -1;
      marInset.m_rightWidth = -1;
      marInset.m_topHeight = -1;

      DwmExtendFrameIntoClientArea(intPtr, ref marInset);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public sealed class MONITORINFO {
      public Int32 m_cbSize;
      public RECT m_rcMonitor;
      public RECT m_rcWork;
      public Int32 m_dwFlags;

      public MONITORINFO() {
        m_cbSize = Marshal.SizeOf(typeof(MONITORINFO));
        m_rcMonitor = new RECT();
        m_rcWork = new RECT();
        m_dwFlags = 0;
      }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct RECT {
      public static readonly RECT Empty = new RECT();

      public Int32 m_left;
      public Int32 m_top;
      public Int32 m_right;
      public Int32 m_bottom;

      public RECT(Int32 left, Int32 top, Int32 right, Int32 bottom) {
        m_left = left;
        m_top = top;
        m_right = right;
        m_bottom = bottom;
      }

      public RECT(RECT rcSrc) {
        m_left = rcSrc.m_left;
        m_top = rcSrc.m_top;
        m_right = rcSrc.m_right;
        m_bottom = rcSrc.m_bottom;
      }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MARGINS {
      public Int32 m_leftWidth;
      public Int32 m_rightWidth;
      public Int32 m_topHeight;
      public Int32 m_bottomHeight;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT {
      public Int32 m_x;
      public Int32 m_y;

      public POINT(Int32 x, Int32 y) {
        m_x = x;
        m_y = y;
      }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MINMAXINFO {
      public POINT m_ptReserved;
      public POINT m_ptMaxSize;
      public POINT m_ptMaxPosition;
      public POINT m_ptMinTrackSize;
      public POINT m_ptMaxTrackSize;
    };
  }
}
