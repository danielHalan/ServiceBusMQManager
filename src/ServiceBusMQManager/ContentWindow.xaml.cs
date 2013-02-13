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


    readonly SolidColorBrush ERROR_BACKGROUND = new SolidColorBrush(Color.FromRgb(173, 28, 59));
    readonly SolidColorBrush ERROR_SPLITHANDLE = new SolidColorBrush(Color.FromRgb(227, 152, 168)); // #E398A8
    readonly SolidColorBrush WARNING_BACKGROUND = new SolidColorBrush(Color.FromRgb(130, 128, 62)); // #82803E
    readonly SolidColorBrush WARNING_SPLITHANDLE = new SolidColorBrush(Color.FromRgb(176, 175, 134)); // #B0AF86

    readonly BitmapImage WARNING_BMP = new BitmapImage(new Uri(@"/ServiceBusMQManager;component/Images/warning-white.png", UriKind.Relative));
    readonly BitmapImage ERROR_BMP = new BitmapImage(new Uri(@"/ServiceBusMQManager;component/Images/Error.selected.png", UriKind.Relative));


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
        imgStackTrace.ToolTip = errorMsg.StackTrace;
        lbRetries.Text = errorMsg.Retries.ToString();

        if( errorMsg.State == QueueItemErrorState.Retry ) {
          imgError.Source = WARNING_BMP;
          lbError.Background = WARNING_BACKGROUND;
          gsError.Background = WARNING_SPLITHANDLE;
          lbRetries.Foreground = WARNING_BACKGROUND;
        
        } else {
          imgError.Source = ERROR_BMP;
          lbError.Background = ERROR_BACKGROUND;
          gsError.Background = ERROR_SPLITHANDLE;
          lbRetries.Foreground = ERROR_BACKGROUND;
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

    private void CopyCallStack_Click(object sender, RoutedEventArgs e) {
      Clipboard.SetData(DataFormats.Text, imgStackTrace.ToolTip);
    }
    private void CopyMessage_Click(object sender, RoutedEventArgs e) {
      Clipboard.SetData(DataFormats.Text, lbError.Text);
    }
  }
}
