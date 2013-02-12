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

  public class SelectColorRoutedEventArgs : RoutedEventArgs {

    public System.Drawing.Color Color  { get; set; }

    public SelectColorRoutedEventArgs(RoutedEvent e)
      : base(e) {
    }
  }


  /// <summary>
  /// Interaction logic for QueueListItemControl.xaml
  /// </summary>
  public partial class QueueListItemControl : UserControl {
    
    QueueListControl.QueueListItem _item;


    public QueueListItemControl(QueueListControl.QueueListItem item, int id) {
      InitializeComponent();

      _item = item;

      tb.Text = item.Name;
      brColor.Background = new SolidColorBrush(Color.FromRgb(item.Color.R, item.Color.G, item.Color.B));
      btn.Tag = id;
    }

    public void UpdateColor(System.Drawing.Color color) {
    
      _item.Color = color;
      brColor.Background = new SolidColorBrush(Color.FromRgb(_item.Color.R, _item.Color.G, _item.Color.B));

    }


    private void btnDelete_Click(object sender, RoutedEventArgs e) {
      var btn = sender as RoundMetroButton;

      var e2 = new DeleteStringListItemRoutedEventArgs(RemovedItemEvent);

      e2.Id = (int)btn.Tag;

      RaiseEvent(e2);
    }



    public static readonly RoutedEvent RemovedItemEvent = EventManager.RegisterRoutedEvent("RemovedItem",
       RoutingStrategy.Direct, typeof(EventHandler<DeleteStringListItemRoutedEventArgs>), typeof(QueueListItemControl));

    public event EventHandler<DeleteStringListItemRoutedEventArgs> RemovedItem {
      add { AddHandler(RemovedItemEvent, value); }
      remove { RemoveHandler(RemovedItemEvent, value); }
    }

    public static readonly RoutedEvent SelectColorEvent = EventManager.RegisterRoutedEvent("SelectColor",
       RoutingStrategy.Direct, typeof(EventHandler<SelectColorRoutedEventArgs>), typeof(QueueListItemControl));

    public event EventHandler<SelectColorRoutedEventArgs> SelectColor {
      add { AddHandler(SelectColorEvent, value); }
      remove { RemoveHandler(SelectColorEvent, value); }
    }

    private void brColor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      var evn = new SelectColorRoutedEventArgs(SelectColorEvent);

      evn.Color = _item.Color;

      RaiseEvent(evn);

      //if( evn.Handled ) {
      //  _item.Color = evn.Color;
        
      //  brColor.Background = new SolidColorBrush(Color.FromRgb(_item.Color.R, _item.Color.G, _item.Color.B));
      //}
    }


  }
}
