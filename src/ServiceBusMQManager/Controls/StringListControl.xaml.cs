#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    StringListControl.xaml.cs
  Created: 2012-12-05

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

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
  
  public class StringListItemRoutedEventArgs : RoutedEventArgs {

    public string Item { get; set; }

    public StringListItemRoutedEventArgs(RoutedEvent e): base(e) {
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

    public void BindItems(IEnumerable<string> items) {
      theStack.Children.Clear();
      _items.Clear();

      if( items != null ) {
        foreach(var itm in items) 
          AddListItem(itm);
      }

      UpdateEmptyLabel();
    }

    private void UpdateEmptyLabel() {
      lbEmpty.Visibility = _items.Count == 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
    }

    public string[] GetItems() {
      return _items.Select( i => i.Value ).ToArray();
    }


    private void AddItem_Click(object sender, RoutedEventArgs e) {

      var e2 = new StringListItemRoutedEventArgs(AddItemEvent);

      RaiseEvent(e2);
      
      if( e2.Handled ) {
        AddListItem(e2.Item);

        var e3 = new StringListItemRoutedEventArgs(AddedItemEvent);
        e3.Item = e2.Item;
        RaiseEvent(e3);

      }
    
    }

    private void AddListItem(string str) {
    
      var id = ++_lastId;

      _items.Add(id, str);

      // Visuals
      var g = new StringListItemControl(str, id);
      g.RemovedItem += btnDelete_Click;
      /*
      Grid g = new Grid();
      g.Background = Brushes.Gray;
      g.Margin = new Thickness(10,1,0,1);


      TextBlock tb = new TextBlock();
      tb.FontSize = 18;
      tb.Foreground = Brushes.White;
      tb.FontFamily =  new FontFamily("Calibri");
      tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
      tb.Margin = new Thickness(10,0,43,0);
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
      */
      theStack.Children.Add(g);

      RecalcControlSize();
      UpdateEmptyLabel();
    }

    private void RecalcControlSize() {
      this.Height = 70 + (40 * _items.Count);
    }

    void btnDelete_Click(object sender, DeleteStringListItemDRoutedEventArgs e) {
      var itm = sender as StringListItemControl;

      var e2 = new StringListItemRoutedEventArgs(RemovedItemEvent);

      e2.Item = _items[(int)e.Id];

      RemoveListItem(itm, e.Id);

      RaiseEvent(e2);
    }


    void RemoveListItem(StringListItemControl itm, int id) {
      _items.Remove(id);
      
      theStack.Children.Remove( itm );

      RecalcControlSize();
      UpdateEmptyLabel();
    }
    

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
             "Title", typeof(string), typeof(StringListControl), new PropertyMetadata(string.Empty));


    public string Title {
      get { return (string)GetValue(TitleProperty); }
      set { SetValue(TitleProperty, value); }
    }

    public static readonly RoutedEvent AddItemEvent = EventManager.RegisterRoutedEvent("AddItem",
      RoutingStrategy.Direct, typeof(EventHandler<StringListItemRoutedEventArgs>), typeof(StringListControl));

    public event EventHandler<StringListItemRoutedEventArgs> AddItem {
      add { AddHandler(AddItemEvent, value); }
      remove { RemoveHandler(AddItemEvent, value); }
    }


    public static readonly RoutedEvent AddedItemEvent = EventManager.RegisterRoutedEvent("AddedItem",
      RoutingStrategy.Direct, typeof(EventHandler<StringListItemRoutedEventArgs>), typeof(StringListControl));

    public event EventHandler<StringListItemRoutedEventArgs> AddedItem {
      add { AddHandler(AddedItemEvent, value); }
      remove { RemoveHandler(AddedItemEvent, value); }
    }


    public static readonly RoutedEvent RemovedItemEvent = EventManager.RegisterRoutedEvent("RemovedItem",
       RoutingStrategy.Direct, typeof(EventHandler<StringListItemRoutedEventArgs>), typeof(StringListControl));

    public event EventHandler<StringListItemRoutedEventArgs> RemovedItem {
      add { AddHandler(RemovedItemEvent, value); }
      remove { RemoveHandler(RemovedItemEvent, value); }
    }


    public int ItemsCount { get { return _items.Count; } }
  }
}
