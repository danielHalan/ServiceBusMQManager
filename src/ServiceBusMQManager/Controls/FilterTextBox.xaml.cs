using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ServiceBusMQManager.Controls {

  public class FilterChangedRoutedEventArgs : RoutedEventArgs {

    public string Filter { get; set; }

    public FilterChangedRoutedEventArgs(RoutedEvent e)
      : base(e) {
    }
  }


  /// <summary>
  /// Interaction logic for FilterTextbox.xaml
  /// </summary>
  public partial class FilterTextBox : UserControl {

    StringBuilder _searchString = new StringBuilder(100);
    
    public FilterTextBox() {
      InitializeComponent();

      EventManager.RegisterClassHandler(typeof(MainWindow),
           Keyboard.PreviewKeyDownEvent, new KeyEventHandler(frmMain_PreviewKeyDown), true);
    }


    
    public void frmMain_PreviewKeyDown(object sender, KeyEventArgs e) {

      if( e.Key == Key.Back && _searchString.Length > 0 ) {

        _searchString.Remove(_searchString.Length - 1, 1);
        SearchStringChanged();

        e.Handled = true;

      } else if( IsCharKey(e.Key) ) {

        _searchString.Append(e.Key.ToString().ToLower());
        SearchStringChanged();

        e.Handled = true;
      }
    }

    private bool IsCharKey(Key key) {
      int v = (int)key;

      return ( v >= 44 && v <= 69 );
    }


    private void SearchStringChanged() {

      if( _searchString.Length > 0 ) {
        var str = _searchString.ToString();
        
        tb.Text = str;

        OnFilterChanged(str);

      } else {
        Clear();
      }
    }



    private void btnClear_Click(object sender, RoutedEventArgs e) {
      Clear();
    }

    private void Clear() {
      _searchString.Clear();

      tb.Text = string.Empty;

      OnFilterCleared();
    }

    private void OnFilterChanged(string str) {
      RaiseEvent(new FilterChangedRoutedEventArgs(FilterChangedEvent) { Filter = str });
    }
    private void OnFilterCleared() {
      RaiseEvent(new RoutedEventArgs(FilterClearedEvent));
    }

    public static readonly RoutedEvent FilterChangedEvent = EventManager.RegisterRoutedEvent("FilterChanged",
       RoutingStrategy.Direct, typeof(EventHandler<FilterChangedRoutedEventArgs>), typeof(FilterTextBox));

    public event EventHandler<FilterChangedRoutedEventArgs> FilterChanged {
      add { AddHandler(FilterChangedEvent, value); }
      remove { RemoveHandler(FilterChangedEvent, value); }
    }


    public static readonly RoutedEvent FilterClearedEvent = EventManager.RegisterRoutedEvent("FilterCleared",
       RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(FilterTextBox));

    public event EventHandler<RoutedEventArgs> FilterCleared {
      add { AddHandler(FilterClearedEvent, value); }
      remove { RemoveHandler(FilterClearedEvent, value); }
    }


  }
}
