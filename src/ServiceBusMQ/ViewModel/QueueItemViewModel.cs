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

    public QueueItemViewModel(QueueItem item)
      : base(item.Queue) {

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

    public string ArrivedTimeString {
      get {
        if( ArrivedTime.Date == DateTime.Today.Date ) {
          return ArrivedTime.ToString("HH:mm:ss");
        } else return string.Format("{1} {0} - {2}", Tools.MONTH_NAMES_ABBR[ArrivedTime.Month - 1], ArrivedTime.Day, ArrivedTime.ToString("HH:mm:ss"));
      }
    }

    public string ImagePath { get { return "Images/" + Queue.Type + ".png"; } }
    public string SelectedImagePath { get { return "Images/" + Queue.Type + ".selected.png"; } }



  
  }
}
