#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    QueueListControl.xaml.cs
  Created: 2013-02-12

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ServiceBusMQManager.Controls {
  /// <summary>
  /// Interaction logic for QueueListControl.xaml
  /// </summary>
  public class QueueListItemRoutedEventArgs : RoutedEventArgs {

    public QueueListControl.QueueListItem Item { get; set; }

    public QueueListItemRoutedEventArgs(RoutedEvent e)
      : base(e) {
    }
  }


  /// <summary>
  /// Interaction logic for QueueListControl.xaml
  /// </summary>
  public partial class QueueListControl : UserControl {


    public class QueueListItem {
      public Color Color { get; set; }
      public string Name { get; set; }

      public QueueListItem(string name, Color color) {
        Name = name;
        Color = color;
      }
      public QueueListItem(string name, int color) {
        Name = name;
        Color = Color.FromArgb(color);
      }

    }

    ColorPickerControl _colorPicker = null;

    int _lastId = 0;

    Dictionary<int, QueueListItem> _items = new Dictionary<int, QueueListItem>();

    public QueueListControl() {
      InitializeComponent();

      _colorPicker = colorPicker;
    }

    void c_SelectedColorChanged(object sender, RoutedEventArgs e) {

    }

    public void BindItems(IEnumerable<QueueListItem> items) {
      theStack.Children.Clear();
      _items.Clear();

      if( items != null ) {
        foreach( var itm in items )
          AddListItem(itm);
      }

      UpdateEmptyLabel();
    }

    private void UpdateEmptyLabel() {
      lbEmpty.Visibility = _items.Count == 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
    }

    public QueueListItem[] GetItems() {
      return _items.Select(i => i.Value).ToArray();
    }


    private void AddItem_Click(object sender, RoutedEventArgs e) {

      var e2 = new QueueListItemRoutedEventArgs(AddItemEvent);

      RaiseEvent(e2);

      if( e2.Handled ) {
        AddListItem(e2.Item);

        var e3 = new QueueListItemRoutedEventArgs(AddedItemEvent);
        e3.Item = e2.Item;
        RaiseEvent(e3);

      }

    }

    private void AddListItem(QueueListItem item) {

      var id = ++_lastId;

      _items.Add(id, item);

      // Visuals
      var g = new QueueListItemControl(item, id);
      g.RemovedItem += btnDelete_Click;
      g.SelectColor += g_SelectColor;

      theStack.Children.Add(g);

      RecalcControlSize();
      UpdateEmptyLabel();
    }

    void g_SelectColor(object sender, SelectColorRoutedEventArgs e) {
      var c = sender as QueueListItemControl;

      var offset = c.TranslatePoint(new System.Windows.Point(0, 0), theGrid);
      _colorPicker.Margin = new Thickness(10, (offset.Y+1) - theGrid.RowDefinitions[0].ActualHeight, 0, 0);
      _colorPicker.Tag = c;
      _colorPicker.Show(e.Color);
      
    }

    private void RecalcControlSize() {
      this.Height = 70 + ( 40 * _items.Count );
    }

    void btnDelete_Click(object sender, DeleteStringListItemRoutedEventArgs e) {
      var itm = sender as QueueListItemControl;

      var e2 = new QueueListItemRoutedEventArgs(RemovedItemEvent);

      e2.Item = _items[(int)e.Id];

      RemoveListItem(itm, e.Id);

      RaiseEvent(e2);
    }


    void RemoveListItem(QueueListItemControl itm, int id) {
      _items.Remove(id);

      theStack.Children.Remove(itm);

      RecalcControlSize();
      UpdateEmptyLabel();
    }


    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
             "Title", typeof(string), typeof(QueueListControl), new PropertyMetadata(string.Empty));


    public string Title {
      get { return (string)GetValue(TitleProperty); }
      set { SetValue(TitleProperty, value); }
    }

    public static readonly RoutedEvent AddItemEvent = EventManager.RegisterRoutedEvent("AddItem",
      RoutingStrategy.Direct, typeof(EventHandler<QueueListItemRoutedEventArgs>), typeof(QueueListControl));

    public event EventHandler<QueueListItemRoutedEventArgs> AddItem {
      add { AddHandler(AddItemEvent, value); }
      remove { RemoveHandler(AddItemEvent, value); }
    }


    public static readonly RoutedEvent AddedItemEvent = EventManager.RegisterRoutedEvent("AddedItem",
      RoutingStrategy.Direct, typeof(EventHandler<QueueListItemRoutedEventArgs>), typeof(QueueListControl));

    public event EventHandler<QueueListItemRoutedEventArgs> AddedItem {
      add { AddHandler(AddedItemEvent, value); }
      remove { RemoveHandler(AddedItemEvent, value); }
    }


    public static readonly RoutedEvent RemovedItemEvent = EventManager.RegisterRoutedEvent("RemovedItem",
       RoutingStrategy.Direct, typeof(EventHandler<QueueListItemRoutedEventArgs>), typeof(QueueListControl));

    public event EventHandler<QueueListItemRoutedEventArgs> RemovedItem {
      add { AddHandler(RemovedItemEvent, value); }
      remove { RemoveHandler(RemovedItemEvent, value); }
    }


    public int ItemsCount { get { return _items.Count; } }

    private void colorPicker_SelectedColorChanged(object sender, RoutedEventArgs e) {
      var itm = _colorPicker.Tag as QueueListItemControl;
      
      itm.UpdateColor(_colorPicker.SelectedColor);

    }
  }

}
