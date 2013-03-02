using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using ServiceBusMQ;

namespace ServiceBusMQManager.Dialogs {

  public enum MessageType { Error }

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

      Topmost = SbmqSystem.UIState.AlwaysOnTop;

      lbTitle.Content = string.Format("{0} - Service Bus MQ Manager", type);

      UpdateStackTraceButton();

      BindMessage();
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
