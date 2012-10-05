#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    QueueItem.cs
  Created: 2012-08-21

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

namespace ServiceBusMQ.Model {

  public enum QueueType { Command=0, Event, Message, Error }

  public class QueueItem {

    public string DisplayName { get; set; }

    public string QueueDisplayName { get; set; }
    public string QueueName { get; set; }
    public QueueType QueueType { get; set; }

    public string Label { get; set; }


    public DateTime ArrivedTime { get; set; }

    public string Id { get; set; }

    public bool Deleted { get; set; }

    public string ImagePath { get { return "Images/" + QueueType + ".png"; } }
    public string SelectedImagePath { get { return "Images/" + QueueType + ".selected.png"; } }

    public string Content { get; set; }


  }
}
