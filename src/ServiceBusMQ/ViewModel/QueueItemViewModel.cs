#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    QueueItemViewModel.cs
  Created: 2013-02-14

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ServiceBusMQ;
using ServiceBusMQ.Model;

namespace ServiceBusMQ.ViewModel {

  public class QueueItemViewModel : QueueItem {

    static readonly Typeface TEXT_FONT = new Typeface("Calibri");

    public QueueItemViewModel(QueueItem item)
      : base(item.Queue) {

      MapQueueItem(item);

      ImagePath = "Images/{0}.png".With(Queue.Type);
      SelectedImagePath = "Images/{0}.selected.png".With(Queue.Type);
      
      if( ArrivedTime.Date == DateTime.Today.Date ) 
        ArrivedTimeString = ArrivedTime.ToString("HH:mm:ss");
      else ArrivedTimeString = "{1} {0} - {2}".With(Tools.MONTH_NAMES_ABBR[ArrivedTime.Month - 1], 
                                                      ArrivedTime.Day, 
                                                      ArrivedTime.ToString("HH:mm:ss"));

      SetTextWidth();
    }

    private void MapQueueItem(QueueItem item) {
      Id = item.Id;
      DisplayName = item.DisplayName;
      MessageNames = item.MessageNames;

      ArrivedTime = item.ArrivedTime;
      ProcessTime = item.ProcessTime;
      Processed = item.Processed;

      Content = item.Content;

      Headers = item.Headers;

      Error = item.Error;
    }

    private void SetTextWidth() {
      var formattedText = new System.Windows.Media.FormattedText(
           DisplayName,
           CultureInfo.CurrentCulture,
           FlowDirection.LeftToRight,
           TEXT_FONT,
           15.5,
           Brushes.Black);

      TextWidth = formattedText.Width + 20;
    }


    public double TextWidth { get; private set; }

    public string ArrivedTimeString { get; private set; }

    public string ImagePath { get; private set; }
    public string SelectedImagePath { get; private set; }



  
  }
}
