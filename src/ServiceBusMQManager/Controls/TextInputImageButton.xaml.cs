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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ServiceBusMQManager.Controls {
  /// <summary>
  /// Interaction logic for TextInputImageButton.xaml
  /// </summary>
  public partial class TextInputImageButton : UserControl {
    public TextInputImageButton() {
      InitializeComponent();
    }



    private void btn_Click(object sender, RoutedEventArgs e) {
      RaiseEvent(new RoutedEventArgs(ClickEvent));
    }

    public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click",
      RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(TextInputImageButton));

    public event RoutedEventHandler Click {
      add { AddHandler(ClickEvent, value); }
      remove { RemoveHandler(ClickEvent, value); }
    }



    public static readonly DependencyProperty SourceProperty =
      DependencyProperty.Register("Source", typeof(string), typeof(TextInputImageButton), new UIPropertyMetadata(string.Empty));

    public string Source {
      get { return (string)GetValue(SourceProperty); }
      set { SetValue(SourceProperty, value); }
    }



  }
}
