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
using System.Windows;
using System.Windows.Controls;

namespace ServiceBusMQManager.Controls {
  
  public class StringListItemRoutedEventArgs : RoutedEventArgs {

    public string Item { get; set; }

    public StringListItemRoutedEventArgs(RoutedEvent e)
      : base(e) {
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

      theStack.Children.Add(g);

      RecalcControlSize();
      UpdateEmptyLabel();
    }

    private void RecalcControlSize() {
      this.Height = 70 + (40 * _items.Count);
    }

    void btnDelete_Click(object sender, DeleteStringListItemRoutedEventArgs e) {
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
