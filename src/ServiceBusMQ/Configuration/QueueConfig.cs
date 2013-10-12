using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceBusMQ.Model;

namespace ServiceBusMQ.Configuration {

  [Serializable]
  public class QueueConfig {
    public string Name { get; set; }
    public QueueType Type { get; set; }
    public int Color { get; set; }

    public QueueConfig() { }

    public QueueConfig(string name, QueueType type, int color = 0) {
      Name = name;
      Type = type;
      Color = color;
    }

  }
}
