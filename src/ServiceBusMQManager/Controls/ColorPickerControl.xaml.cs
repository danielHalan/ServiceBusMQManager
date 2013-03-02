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

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ServiceBusMQ;

namespace ServiceBusMQManager.Controls {
  /// <summary>
  /// Interaction logic for ColorPickerControl.xaml
  /// </summary>
  public partial class ColorPickerControl : UserControl {
    public ColorPickerControl() {
      InitializeComponent();

      this.Visibility = System.Windows.Visibility.Hidden;

      BindColors();
    }

    private void BindColors() {

      foreach( var color in QueueColorManager.COLORS.Select(c => System.Drawing.Color.FromArgb(c)) ) {
        var b = new Border();
        b.Background = new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
        b.Width = b.Height = 20;
        b.Margin = new Thickness(5, 8, 0, 0);
        b.BorderBrush = new SolidColorBrush(Colors.White);
        b.MouseLeftButtonDown += Color_MouseLeftButtonDown;
        b.Cursor = Cursors.Hand;
        b.BorderThickness = new Thickness(1);

        thePanel.Children.Add(b);
      }

    }


    private static SolidColorBrush BORDER_SELECTED = new SolidColorBrush(Colors.Yellow);
    private static SolidColorBrush BORDER_UNSELECTED = new SolidColorBrush(Colors.White);
    private static Thickness BORDER_THICKNESS_SELECTED = new Thickness(2);
    private static Thickness BORDER_THICKNESS_UNSELECTED = new Thickness(1);

    public void Show(System.Drawing.Color color) {

      SelectedColor = color;

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
