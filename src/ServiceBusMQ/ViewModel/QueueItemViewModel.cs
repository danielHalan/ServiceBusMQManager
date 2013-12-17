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

    public QueueItemViewModel(QueueItem item, bool showMilliSeconds)
      : base(item.Queue) {

      MapQueueItem(item);

      BindImage();
      
      if( ArrivedTime.Date == DateTime.Today.Date ) 
        ArrivedTimeString = ArrivedTime.ToString("HH:mm:ss");
      else ArrivedTimeString = "{0} {1} {2} - {3}".With(
													  ArrivedTime.Day,
													  Tools.MONTH_NAMES_ABBR[ArrivedTime.Month - 1],
													  ArrivedTime.Year,
													  ArrivedTime.ToString("HH:mm:ss"));
      
      if( showMilliSeconds )
        ArrivedTimeMSString = ArrivedTime.ToString(".fff");

      SetTextWidth();
    }

    protected override void ProcessedChanged() {

      BindImage();
    }

    private void BindImage() {
      string imageName = GetImageName();
      ImagePath = "Images/{0}.png".With(imageName);
      SelectedImagePath = "Images/{0}.selected.png".With(imageName);
    }

    private string GetImageName() {
      StringBuilder sb = new StringBuilder(Queue.Type.ToString(), 50);
      if( Processed ) 
        sb.Append("-processed");

      if( Queue.Type != QueueType.Error && Error != null )
        sb.Append("-warn");

      return sb.ToString();
    }

    private void MapQueueItem(QueueItem item) {
      Id = item.Id;
      MessageQueueItemId = item.MessageQueueItemId;
      DisplayName = item.DisplayName;
      Messages = item.Messages;

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

    public string ArrivedTimeString { get; set; }
    public string ArrivedTimeMSString { get; set; }

    public string ImagePath { get; private set; }
    public string SelectedImagePath { get; private set; }



  
  }
}
