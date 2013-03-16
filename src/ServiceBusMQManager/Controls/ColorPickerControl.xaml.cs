#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    ColorPickerControl.xaml.cs
  Created: 2013-02-12

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ServiceBusMQ;

namespace ServiceBusMQManager.Controls {

  internal class ColorItem {

    private static SolidColorBrush BORDER_SELECTED = new SolidColorBrush(Colors.Yellow);
    private static SolidColorBrush BORDER_UNSELECTED = new SolidColorBrush(Colors.White);
    private static Thickness BORDER_THICKNESS_SELECTED = new Thickness(2);
    private static Thickness BORDER_THICKNESS_UNSELECTED = new Thickness(1);

    public bool Selected { get; set; }

    public SolidColorBrush Color { get; set; }
    public Thickness BorderThickness { get { return Selected ? BORDER_THICKNESS_SELECTED : BORDER_THICKNESS_UNSELECTED; }  }
    public SolidColorBrush BorderBrush { get { return Selected ? BORDER_SELECTED : BORDER_UNSELECTED; } }
    
    public ColorItem(SolidColorBrush c, bool selected) {
      Color = c;
      Selected = selected;
    }
  }


  /// <summary>
  /// Interaction logic for ColorPickerControl.xaml
  /// </summary>
  public partial class ColorPickerControl : UserControl {

    public ColorPickerControl() {
      InitializeComponent();

      this.Visibility = System.Windows.Visibility.Hidden;

      //BindColors();
    }

    private void BindColors(System.Drawing.Color selectedColor) {
      List<ColorItem> list = new List<ColorItem>(QueueColorManager.COLORS.Length);

      foreach( var color in QueueColorManager.COLORS.Select(c => System.Drawing.Color.FromArgb(c |  0xFF << 24 )) ) {
        list.Add(new ColorItem(new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B)), selectedColor == color));
      }

      theList.ItemsSource = list;
    }



    public void Show(System.Drawing.Color color) {

      BindColors(color);

      SelectedColor = color;

      /*
      Border selected = null;
      foreach( Border b in thePanel.Children ) {
        var c = ( b.Background as SolidColorBrush ).Color;

        if( c.R == color.R && c.G == color.G && c.B == color.B ) {
          b.BorderThickness = BORDER_THICKNESS_SELECTED;
          b.BorderBrush = BORDER_SELECTED;
          selected = b;
        } else {
          b.BorderThickness = BORDER_THICKNESS_UNSELECTED;
          b.BorderBrush = BORDER_UNSELECTED;
        }
      }

      if( selected != null ) {
        thePanel.Children.Remove(selected);
        thePanel.Children.Insert(0, selected);
      }
      */

      this.Visibility = System.Windows.Visibility.Visible;
    }

    private void UserControl_GotFocus_1(object sender, RoutedEventArgs e) {
      //this.Focus();
    }

    private void UserControl_LostFocus_1(object sender, RoutedEventArgs e) {
    }

    private void UserControl_LostKeyboardFocus_1(object sender, KeyboardFocusChangedEventArgs e) {


      if( e.NewFocus is ScrollViewer ) {
        var pt = Mouse.GetPosition(this);

        if( ( pt.X > 0 && pt.X < this.Width ) &&
            ( pt.Y > 0 && pt.Y < this.Height ) ) {

          //if( clock.SelectedArm == TimeArm.Hour )
          //  tbHour.Focus();
          //else tbMin.Focus();

          return; // ignore ScrollViewer control, as it always selected when moving time-arms
        }
      }

      bool isParent = this.IsChildControl((DependencyObject)e.NewFocus);

      if( !isParent )
        HideControl();

    }

    private void HideControl() {

      OnSelectedColorChanged();

      this.Visibility = System.Windows.Visibility.Hidden;
    }
    private void ColorSelected(System.Drawing.Color c) {

      SelectedColor = c;

      OnSelectedColorChanged();

      this.Visibility = System.Windows.Visibility.Hidden;
    }


    private void OnSelectedColorChanged() {
      RaiseEvent(new RoutedEventArgs(SelectedColorChangedEvent));
    }

    public static readonly RoutedEvent SelectedColorChangedEvent = EventManager.RegisterRoutedEvent("SelectedColorChanged",
      RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(ColorPickerControl));

    public event RoutedEventHandler SelectedColorChanged {
      add { AddHandler(SelectedColorChangedEvent, value); }
      remove { RemoveHandler(SelectedColorChangedEvent, value); }
    }


    private void UserControl_PreviewLostKeyboardFocus_1(object sender, KeyboardFocusChangedEventArgs e) {

      //if( e.OldFocus == btnTimeOfDay && !this.IsChildControl((DependencyObject)e.NewFocus) ) {

      //  if( clock.SelectedArm == TimeArm.Hour )
      //    tbHour.Focus();
      //  else tbMin.Focus();

      //  e.Handled = true;
      //}
    }

    private void TextInputLabelButton_Click_1(object sender, RoutedEventArgs e) {
      HideControl();
    }

    private void Color_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if( e.AddedItems.Count > 0 ) {
        var c = ( e.AddedItems[0] as ColorItem ).Color;

        QueueColorManager.ReturnColor(SelectedColor);

        SelectedColor = System.Drawing.Color.FromArgb(c.Color.R, c.Color.G, c.Color.B);

        QueueColorManager.UseColor(SelectedColor);

        HideControl();
      }
    }

    private void Color_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      var b = sender as Border;

      var c = ( b.Background as SolidColorBrush ).Color;

      QueueColorManager.ReturnColor(SelectedColor);

      SelectedColor = System.Drawing.Color.FromArgb(c.R, c.G, c.B);

      QueueColorManager.UseColor(SelectedColor);

      HideControl();
    }




    public static readonly DependencyProperty SelectedColorProperty =
      DependencyProperty.Register("SelectedColor", typeof(System.Drawing.Color), typeof(ColorPickerControl), new UIPropertyMetadata(System.Drawing.Color.Azure));

    public System.Drawing.Color SelectedColor {
      get { return (System.Drawing.Color)GetValue(SelectedColorProperty); }
      set { SetValue(SelectedColorProperty, value); }
    }

    private void btn_Click_1(object sender, RoutedEventArgs e) {
      HideControl();
    }



  }
}
