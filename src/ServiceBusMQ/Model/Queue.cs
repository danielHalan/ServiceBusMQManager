using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceBusMQ.Model {
  
  public class Queue {

    public string Name { get; private set; }
    public QueueType Type { get; private set; }
    
    public string DisplayName { get; private set; }

    public int Color { get; set; }
    public string ColorString { get; private set; }
    public string SelectedColorString { get; private set; }

    public Queue(string name, QueueType type, int color = 0) {
    
      Name = name;
      Type = type;

      DisplayName = name.CutBeginning(46);

      Color = color;
      
      ColorString = "#" + color.ToString("X");
      SelectedColorString = "#" + GetSelectedColor(color).ToString("X");
    }

    private int GetSelectedColor(int color) {
      byte r = (byte) ((color & 0xFF0000) >> 16);
      byte g = (byte) ((color & 0x00FF00) >> 8);
      byte b = (byte) (color & 0x0000FF);

      return ( Math.Min(r + 50, 0xFF) << 16 ) | ( Math.Min(g + 50, 0xFF) << 8 ) | Math.Min(b + 50, 0xFF);
    }


  }
}
