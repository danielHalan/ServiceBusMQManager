#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    Queue.cs
  Created: 2013-02-10

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceBusMQ.Manager;

namespace ServiceBusMQ.Model {

  public class Queue  {

    public string Name { get; private set; }
    public QueueType Type { get; private set; }
    
    public string DisplayName { get; private set; }

    public int Color { get; set; }
    public string ColorString { get; private set; }
    public string SelectedColorString { get; private set; }

    public MessageContentFormat ContentFormat { get; private set; }

    public Queue(string name, QueueType type, int color = 0) {
    
      Name = name;
      Type = type;

      DisplayName = name.CutBeginning(46);

      Color = color;

      ContentFormat = MessageContentFormat.Unknown;
      
      ColorString = "#" + color.ToString("X");
      SelectedColorString = "#" + GetSelectedColor(color).ToString("X");
    }

    private int GetSelectedColor(int color) {
      byte r = (byte) ((color & 0xFF0000) >> 16);
      byte g = (byte) ((color & 0x00FF00) >> 8);
      byte b = (byte) (color & 0x0000FF);

      return ( Math.Min(r + 50, 0xFF) << 16 ) | ( Math.Min(g + 50, 0xFF) << 8 ) | Math.Min(b + 50, 0xFF);
    }

    public void SetContentFormat(string content) {

      if( content.StartsWith("<xml") )
        ContentFormat = MessageContentFormat.Xml;
      
      else if( content.StartsWith("[") )
        ContentFormat = MessageContentFormat.Json;

      else ContentFormat = MessageContentFormat.Other;

    }
  }
}
