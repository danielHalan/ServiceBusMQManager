#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    QueueColorManager.cs
  Created: 2013-02-10

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ServiceBusMQ {
  public static class QueueColorManager {

    public static readonly int[] COLORS = new int[] { 0xffffff, 0xC9C9C9, // white & Gray
                                      0xfef200, // yellow
                                      0xA200FF, // purple
                                      0xE671B8, // pink light 
                                      0xFF0097, // pink dark 
                                      0xDC572E, // orange
                                      0xF09609, // orange brown 
                                      0xA05000, // brown 
                                      0x632F00, // brown dark
                                      0x8CBF26, // lime green 
                                      0x339933, // dark green
                                      0x0E97FF, // azure
                                      0x0000fe, // blue
                                      0x0100a6, // blue dark
                                      0xE51400  // red
    };

    static List<int> _unusedColors;

    static Random _rnd = new Random();

    static QueueColorManager() {

      _unusedColors = new List<int>(COLORS);

    }

    public static int GetRandomAvailableColorAsInt() {

      if( _unusedColors.Count > 0 ) {
        int index = _rnd.Next(_unusedColors.Count);

        int color = _unusedColors[index];
        _unusedColors.Remove(color);

        return color;

      } else return Color.Azure.ToArgb();
    }

    public static Color GetRandomAvailableColor() {
      return System.Drawing.Color.FromArgb(GetRandomAvailableColorAsInt());
    }

  }


}
