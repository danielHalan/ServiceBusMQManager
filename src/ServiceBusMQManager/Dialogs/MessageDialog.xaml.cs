using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ServiceBusMQ;

namespace ServiceBusMQManager.Dialogs {

  public enum MessageType { Error, Info }

  /// <summary>
  /// Interaction logic for MessageDialog.xaml
  /// </summary>
  public partial class MessageDialog : Window {

    string _text;
    Exception _e;
    private MessageType _type;

    public MessageDialog(MessageType type, string text, Exception e) {
      InitializeComponent();

      _type = type;
      _text = text;
      _e = e;

      switch(_type) {
        case MessageType.Info:
          ImageSource = "/ServiceBusMQManager;component/Images/info.circle.png";
          img.Margin = new Thickness(15, 50, 0, 0);
          img.Width = 40;
          btnCancel.Visibility = System.Windows.Visibility.Collapsed;
          lbSendReportText.Visibility = System.Windows.Visibility.Collapsed;
          btnOK.Content = "CLOSE";
          break;

        case MessageType.Error:
          ImageSource = "/ServiceBusMQManager;component/Images/Error.png";
          break;
      }

      Topmost = SbmqSystem.UIState.AlwaysOnTop;

      lbTitle.Content = string.Format("{0} - Service Bus MQ Manager", type);

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
        para.Inlines.Add((new Run(_text) { FontSize = 19 }));
      else if( _e != null )
        para.Inlines.Add((new Run(_e.Message) { FontSize = 19 }));


      tbMessage.Document.Blocks.Add(para);

      if( _e != null && _text != null ) {
        para = new Paragraph();
        para.Inlines.Add(new Run(_e.Message) { FontSize = 15 });
        tbMessage.Document.Blocks.Add(para);
      }

      //if( !_url.IsValid() )
      //  _url = inf.Url;
    }

    public static void Show(MessageType type, string text, Exception e = null) {

      var dlg = new MessageDialog(type, text, e);

      dlg.ShowDialog();

    }



    private void btnOK_Click(object sender, RoutedEventArgs e) {
      //System.Diagnostics.Process.Start(_url);
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
