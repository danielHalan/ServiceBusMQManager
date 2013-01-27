#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    ContentWindow.xaml.cs
  Created: 2012-08-21

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
//using ScintillaNET;
using ServiceBusMQ;
using ServiceBusMQ.Manager;
using ServiceBusMQ.Model;

namespace ServiceBusMQManager {
  /// <summary>
  /// Interaction logic for ContentWindow.xaml
  /// </summary>
  public partial class ContentWindow : Window {


    readonly SolidColorBrush BACKGROUND_ERROR = new SolidColorBrush(Color.FromRgb(173, 28, 59));
    readonly SolidColorBrush BACKGROUND_WARNING = new SolidColorBrush(Color.FromRgb(158, 153, 8)); // #C9C42A

    readonly BitmapImage BMP_WARNING = new BitmapImage(new Uri(@"/ServiceBusMQManager;component/Images/warning-white.png", UriKind.Relative));
    readonly BitmapImage BMP_ERROR = new BitmapImage(new Uri(@"/ServiceBusMQManager;component/Images/Error.selected.png", UriKind.Relative));


    private HwndSource _hwndSource;


    public ContentWindow() {
      InitializeComponent();

      SourceInitialized += ContentWindow_SourceInitialized;



      this.Icon = BitmapFrame.Create(this.GetImageResourceStream("main.ico"));
    }

    void ContentWindow_SourceInitialized(object sender, EventArgs e) {
 
      this.HideFromProgramSwitcher();
    }




    private static FlowDocument GetFlowDocument(string xml) {
      StringReader stringReader = new StringReader(xml);

      XmlReader xmlReader = XmlReader.Create(stringReader);

      Section sec = XamlReader.Load(xmlReader) as Section;

      FlowDocument doc = new FlowDocument();

      while( sec.Blocks.Count > 0 )
        doc.Blocks.Add(sec.Blocks.FirstBlock);

      return doc;
    }



    public void SetContent(string content, MessageContentFormat contentType, QueueItemError errorMsg = null) {    



      switch(contentType) {
        case MessageContentFormat.Xml:
          tbContent.CodeLanguage = NServiceBus.Profiler.Common.CodeParser.CodeLanguage.Xml;
          tbContent.Text = content;
          break;

        case MessageContentFormat.Json:
          tbContent.CodeLanguage = NServiceBus.Profiler.Common.CodeParser.CodeLanguage.Json;
          tbContent.Text = content;
          break;
      }

      if( errorMsg != null ) {
        theGrid.RowDefinitions[1].Height = new GridLength(61);
        lbError.Text = errorMsg.Message;
        lbRetries.Text = errorMsg.Retries.ToString();

        if( errorMsg.State == QueueItemErrorState.Retry ) {
          imgError.Source = BMP_WARNING;

          lbError.Background = BACKGROUND_WARNING;
          lbRetries.Foreground = BACKGROUND_WARNING;
        
        } else {
          imgError.Source = BMP_ERROR;
          lbError.Background = BACKGROUND_ERROR;
          lbRetries.Foreground = BACKGROUND_ERROR;
        }


      } else { 
        theGrid.RowDefinitions[1].Height = new GridLength(0);
      }

    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      this.MoveOrResizeWindow(e);
    }
    private void HandleCloseClick(Object sender, RoutedEventArgs e) {
      Close();
    }
    private void Window_MouseMove(object sender, MouseEventArgs e) {
      Cursor = this.GetBorderCursor();
    }


    internal void SetTitle(string str) {
      lbTitle.Content = str;
      this.Title = str;
    }


    private void frmContent_Activated_1(object sender, EventArgs e) {
      App.Current.MainWindow.EnsureVisibility();
    }
  }
}
