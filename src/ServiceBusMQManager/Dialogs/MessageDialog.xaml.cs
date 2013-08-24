#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    MessageDialog.xaml.cs
  Created: 2013-03-02

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ServiceBusMQ;
using ServiceBusMQ.HalanService;
using ServiceBusMQ.Model.HalanService;

namespace ServiceBusMQManager.Dialogs {

  public enum MessageType { Error, Warn, Info }

  /// <summary>
  /// Interaction logic for MessageDialog.xaml
  /// </summary>
  public partial class MessageDialog : Window {

    string _text;
    string _content;
    Exception _e;
    private MessageType _type;

    public MessageDialog(MessageType type, string text, string content) {
      InitializeComponent();

      _type = type;
      _text = text;
      _content = content;

      InitializeDialogUI();
    }


    public MessageDialog(MessageType type, string text, Exception e) {
      InitializeComponent();

      _type = type;
      _text = text;
      _e = e;

      InitializeDialogUI();

    }


    void InitializeDialogUI() {

      switch( _type ) {
        case MessageType.Info:
          ImageSource = "/ServiceBusMQManager;component/Images/Msg.Info.png";
          img.Margin = new Thickness(15, 50, 0, 0);
          img.Width = 40;
          btnCancel.Visibility = System.Windows.Visibility.Collapsed;
          lbSendReportText.Visibility = System.Windows.Visibility.Collapsed;
          btnOK.Content = "CLOSE";
          break;

        case MessageType.Warn:
          ImageSource = "/ServiceBusMQManager;component/Images/Msg.Warn.png";
          btnCancel.Visibility = System.Windows.Visibility.Collapsed;
          lbSendReportText.Visibility = System.Windows.Visibility.Collapsed;
          btnOK.Content = "CLOSE";
          break;

        case MessageType.Error:
          ImageSource = "/ServiceBusMQManager;component/Images/Msg.Error.png";
          break;
      }

      Topmost = SbmqSystem.UIState.AlwaysOnTop;

      lbTitle.Content = Title = string.Format("{0} - Service Bus MQ Manager", _type);

      UpdateStackTraceButton();

      BindMessage();
    }


    public static readonly DependencyProperty ImageSourceProperty =
      DependencyProperty.Register("ImageSource", typeof(string), typeof(MessageDialog), new UIPropertyMetadata(string.Empty));

    public string ImageSource {
      get { return (string)GetValue(ImageSourceProperty); }
      set { SetValue(ImageSourceProperty, value); }
    }


    private void UpdateStackTraceButton() {
      if( _type == MessageType.Error && _e != null ) {
        imgStackTrace.Visibility = System.Windows.Visibility.Visible;
        imgStackTrace.ToolTip = _e.StackTrace;
      } else {
        imgStackTrace.Visibility = System.Windows.Visibility.Collapsed;
      }
    }

    private void BindMessage() {
      Paragraph para = new Paragraph();

      if( _text != null )
        para.Inlines.Add(( new Run(_text) { FontSize = 19 } ));
      else if( _e != null )
        para.Inlines.Add(( new Run(_e.Message) { FontSize = 19 } ));


      tbMessage.Document.Blocks.Add(para);

      if( _type == MessageType.Error ) {

        if( _e != null && _text != null ) {
          para = new Paragraph();
          para.Inlines.Add(new Run(_e.Message) { FontSize = 15 });
          tbMessage.Document.Blocks.Add(para);
        }

      } else if( _type == MessageType.Warn ) {

        if( _content.IsValid() ) {

          para = new Paragraph();
          para.Inlines.Add(new Run(_content) { FontSize = 15 });
          tbMessage.Document.Blocks.Add(para);

        }

      
      
      }

      //if( !_url.IsValid() )
      //  _url = inf.Url;
    }

    public static void Show(MessageType type, string text, Exception e = null) {
      var dlg = new MessageDialog(type, text, e);

      dlg.ShowDialog();
    }

    public static void Show(MessageType type, string text, string content) {
      var dlg = new MessageDialog(type, text, content);

      dlg.ShowDialog();
    }


    private void btnOK_Click(object sender, RoutedEventArgs e) {
      if( _type == MessageType.Error && _text.IsValid() ) {

        BackgroundWorker bw = new BackgroundWorker();
        bw.DoWork += (object s, DoWorkEventArgs ev) => {
          ErrorReport rep = new ErrorReport(SbmqSystem.AppInfo, new Error(_text, _e), new string[0]);
          rep.Send();
        };

        bw.RunWorkerCompleted += (object s, RunWorkerCompletedEventArgs ev) => {
          if( ev.Error is FailedToSendErrorReportException )
            MessageBox.Show("Error occured when trying to send report, " + ev.Error.Message, "Error Sendng Report", MessageBoxButton.OK, MessageBoxImage.Error);

        };

        bw.RunWorkerAsync();
      }

      Close();
    }




    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      this.MoveOrResizeWindow(e);
    }
    private void Window_MouseMove(object sender, MouseEventArgs e) {
      Cursor = this.GetBorderCursor();
    }
    private void HandleCloseClick(Object sender, RoutedEventArgs e) {
      Close();
    }

    private void CopyCallStack_Click(object sender, RoutedEventArgs e) {
      Clipboard.SetData(DataFormats.Text, imgStackTrace.ToolTip);
    }

  }
}
