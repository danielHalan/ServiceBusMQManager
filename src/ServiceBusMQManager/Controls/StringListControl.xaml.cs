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
  
  public class AddItemRoutedEventArgs : RoutedEventArgs {

    public string Item { get; set; }

    public AddItemRoutedEventArgs(RoutedEvent e): base(e) {
    }
  }
  
  /// <summary>
  /// Interaction logic for StringListControl.xaml
  /// </summary>
  public partial class StringListControl : UserControl {
    
    int _lastId = 0;

    Dictionary<int, string> _items = new Dictionary<int, string>();

    public StringListControl() {
      InitializeComponent();

    }

    public void BindItems(string[] items) {
      
      foreach(var itm in items) 
        AddListItem(itm);
    }

    public string[] GetItems() {
      return _items.Select( i => i.Value ).ToArray();
    }


    private void AddItem_Click(object sender, RoutedEventArgs e) {

      var e2 = new AddItemRoutedEventArgs(AddItemEvent);

      RaiseEvent(e2);
      
      if( e2.Handled ) {
        AddListItem(e2.Item);
      }
    
    }

    private void AddListItem(string str) {
    
            //<Grid>
            //    <TextBlock Text="sdsdsdsd" FontSize="18" FontFamily="Calibri" Margin="5,0,43,0" VerticalAlignment="Center" />
            //    <local:RoundMetroButton Source="/ServiceBusMQManager;component/Images/delete-item.png" Height="32" HorizontalAlignment="Right" Margin="0,0,4,4" />
            //</Grid>

      var id = ++_lastId;

      _items.Add(id, str);

      // Visuals

      Grid g = new Grid();
      g.Background = Brushes.Gray;

      TextBlock tb = new TextBlock();
      tb.FontSize = 18;
      tb.Foreground = Brushes.White;
      tb.FontFamily =  new FontFamily("Calibri");
      tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
      tb.Margin = new Thickness(5,0,43,0);
      tb.Text = str;
      g.Children.Add(tb);

      RoundMetroButton btn = new RoundMetroButton();
      btn.Source = "/ServiceBusMQManager;component/Images/delete-item-white.png";
      btn.Height = 32;
      btn.Margin = new Thickness(0, 2, 4, 2);
      btn.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
      btn.Tag = id;
      btn.Click += btnDelete_Click;
      g.Children.Add(btn);

      theStack.Children.Add(g);

      RecalcControlSize();
    }

    private void RecalcControlSize() {
      this.Height = 50 + (40 * _items.Count) + 10;
    }

    void btnDelete_Click(object sender, RoutedEventArgs e) {
      var btn = sender as RoundMetroButton;
      _items.Remove( Convert.ToInt32(btn.Tag));

      theStack.Children.Remove( btn.Parent as UIElement );
      
      RecalcControlSize();
    }


    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
             "Title", typeof(string), typeof(StringListControl), new PropertyMetadata(string.Empty));


    public string Title {
      get { return (string)GetValue(TitleProperty); }
      set { SetValue(TitleProperty, value); }
    }

    public static readonly RoutedEvent AddItemEvent = EventManager.RegisterRoutedEvent("AddItem",
      RoutingStrategy.Direct, typeof(EventHandler<AddItemRoutedEventArgs>), typeof(StringListControl));

    public event EventHandler<AddItemRoutedEventArgs> AddItem {
      add { AddHandler(AddItemEvent, value); }
      remove { RemoveHandler(AddItemEvent, value); }
    }



  }
}
