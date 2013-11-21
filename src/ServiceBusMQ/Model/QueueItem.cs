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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ServiceBusMQ.Model {

  public enum QueueType { Command = 0, Event = 1, Message = 2, Error = 3 }

  public class QueueItem  {
    private bool _processed;

    public QueueItem(Queue queue) {
      Queue = queue;
    }

    public string Id { get; set; }
    public object MessageQueueItemId { get; set; }

    public string DisplayName { get; set; }
    public MessageInfo[] Messages { get; set; }

    public Queue Queue { get; private set; }

    public DateTime ArrivedTime { get; set; }
    public int ProcessTime { get; set; }
    public bool Processed {
      get { return _processed; }
      set {
        if( _processed != value ) { 
          _processed = value; 
          ProcessedChanged(); 
        }
      }
    }

    public string Content { get; set; }

    public Dictionary<string, string> Headers { get; set; }

    public QueueItemError Error { get; set; }

    protected virtual void ProcessedChanged() { }
  }
}
