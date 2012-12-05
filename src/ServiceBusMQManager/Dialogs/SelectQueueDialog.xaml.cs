using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using ServiceBusMQ;

namespace ServiceBusMQManager.Dialogs {
  /// <summary>
  /// Interaction logic for SelectQueueDialog.xaml
  /// </summary>
  public partial class SelectQueueDialog : Window {
    
    public SelectQueueDialog(string[] queueNames) {
      InitializeComponent();

      Topmost = SbmqSystem.Instance.UIState.AlwaysOnTop;

      lbQueues.ItemsSource = queueNames;
    }

    public string SelectedQueueName { get; set; }

    private void btnOK_Click(object sender, RoutedEventArgs e) {

      SelectedQueueName = lbQueues.SelectedItem as string;
      DialogResult = true;
    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      this.MoveOrResizeWindow(e);
    }


    private void Window_MouseMove(object sender, MouseEventArgs e) {
      Cursor = this.GetBorderCursor();
    }
    
    private void HandleMaximizeClick(object sender, RoutedEventArgs e) {
      var s = WpfScreen.GetScreenFrom(this);

      this.Top = s.WorkingArea.Top;
      this.Height = s.WorkingArea.Height;
    }
    private void HandleCloseClick(Object sender, RoutedEventArgs e) {
      Close();
    }

    private void lbQueues_MouseDoubleClick(object sender, MouseButtonEventArgs e) {

      if( lbQueues.SelectedIndex > -1 ) {
        SelectedQueueName = lbQueues.SelectedItem as string;
        DialogResult = true;
      }

    }

    private void lbQueues_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      btnOK.IsEnabled = lbQueues.SelectedItem != null;
    }


  }
}
