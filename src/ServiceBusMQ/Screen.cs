#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    Screen.cs
  Created: 2012-09-16

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace ServiceBusMQ {
  public class WpfScreen {
    
    public static IEnumerable<WpfScreen> AllScreens() {
      foreach( Screen screen in System.Windows.Forms.Screen.AllScreens ) {
        yield return new WpfScreen(screen);
      }
    }

    public static WpfScreen GetScreenFrom(Window window) {
      WindowInteropHelper windowInteropHelper = new WindowInteropHelper(window);
      Screen screen = System.Windows.Forms.Screen.FromHandle(windowInteropHelper.Handle);
      WpfScreen wpfScreen = new WpfScreen(screen);
      return wpfScreen;
    }

    public static WpfScreen GetScreenFrom(System.Windows.Point point) {
      int x = (int)Math.Round(point.X);
      int y = (int)Math.Round(point.Y);

      // are x,y device-independent-pixels ??
      System.Drawing.Point drawingPoint = new System.Drawing.Point(x, y);
      Screen screen = System.Windows.Forms.Screen.FromPoint(drawingPoint);
      WpfScreen wpfScreen = new WpfScreen(screen);

      return wpfScreen;
    }

    public static WpfScreen Primary {
      get { return new WpfScreen(System.Windows.Forms.Screen.PrimaryScreen); }
    }

    private readonly System.Windows.Forms.Screen screen;

    internal WpfScreen(System.Windows.Forms.Screen screen) {
      this.screen = screen;
    }

    public Rect DeviceBounds {
      get { return this.GetRect(this.screen.Bounds); }
    }

    public Rect WorkingArea {
      get { return this.GetRect(this.screen.WorkingArea); }
    }

    private Rect GetRect(Rectangle value) {
      // should x, y, width, hieght be device-independent-pixels ??
      return new Rect {
        X = value.X,
        Y = value.Y,
        Width = value.Width,
        Height = value.Height
      };
    }

    public bool IsPrimary {
      get { return this.screen.Primary; }
    }

    public string DeviceName {
      get { return this.screen.DeviceName; }
    }
  }
}
