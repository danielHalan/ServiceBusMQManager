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

  public enum QueueType { Command = 0, Event, Message, Error }

  public class QueueItem {


    static readonly string[] MONTH_NAMES = new string[12];

    static QueueItem() {
      for(int i = 0; i < 12; i++ ) {
        MONTH_NAMES[i] = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i+1);
      }
    }



    public string DisplayName { get; set; }

    public double TextWidth {
      get {

        var formattedText = new System.Windows.Media.FormattedText(
             DisplayName,
             CultureInfo.CurrentCulture,
             FlowDirection.LeftToRight,
             new Typeface("Calibri"),
             15.5,
             Brushes.Black);

        return formattedText.Width + 20;

      }
    }

    //TODO: Move this to an Queue Object
    public string QueueDisplayName { get; set; }
    public string QueueName { get; set; }
    public QueueType QueueType { get; set; }
    public string QueueColor { get; set; }
    public string SelectedQueueColor { get; set; }

    public string Label { get; set; }

    public string[] MessageNames { get; set; }

    public DateTime ArrivedTime { get; set; }
    public int ProcessTime { get; set; }
    
    public string ArrivedTimeString {
      get {
        if( ArrivedTime.Date == DateTime.Today.Date ) {
          return ArrivedTime.ToString("HH:mm:ss");
        } else return string.Format("{1} {0} - {2}", MONTH_NAMES[ArrivedTime.Month-1], ArrivedTime.Day, ArrivedTime.ToString("HH:mm:ss"));
      }
    }

    public string Id { get; set; }

    public bool Processed { get; set; }

    public string ImagePath { get { return "Images/" + QueueType + ".png"; } }
    public string SelectedImagePath { get { return "Images/" + QueueType + ".selected.png"; } }

    public string Content { get; set; }

    public Dictionary<string, string> Headers { get; set; }

    public QueueItemError Error { get; set; }

  }
}
