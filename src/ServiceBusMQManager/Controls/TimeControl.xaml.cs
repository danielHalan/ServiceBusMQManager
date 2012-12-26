#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    TimeControl.xaml.cs
  Created: 2012-12-23

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
using ServiceBusMQ;

namespace ServiceBusMQManager.Controls {
  public enum TimeOfDay { AM, PM }

  /// <summary>
  /// Interaction logic for TimeControl.xaml
  /// </summary>
  public partial class TimeControl : UserControl {

    bool _capturing = false;
    
    private TimeOfDay _timeOfDay;


    public TimeControl() {
      InitializeComponent();

      this.Visibility = System.Windows.Visibility.Hidden;
    }

    public void Show(DateTime time) {

      if( time.Hour > 12 ) {
        SetTimeOfDay(TimeOfDay.PM);

      } else SetTimeOfDay(TimeOfDay.AM);


      clock.SetValue(time.Hour, time.Minute, time.Second);
      clock.SelectedArm = TimeArm.Hour;

      SetTextValue(tbHour, time.Hour % 12);
      SetTextValue(tbMin, time.Minute);
      SetTextValue(tbSec, time.Second);

      this.Visibility = System.Windows.Visibility.Visible;

      tbHour.SelectAll();
      tbHour.Focus();
    }
    private void SetTimeOfDay(TimeOfDay timeOfDay) {
      _timeOfDay = timeOfDay;

      btnTimeOfDay.Content = _timeOfDay.ToString();
    }

    private void Ellipse_MouseDown_1(object sender, MouseButtonEventArgs e) {
      _capturing = true;
    }

    private void Ellipse_MouseUp_1(object sender, MouseButtonEventArgs e) {
      _capturing = false;
    }

    public static readonly DependencyProperty SelectedTimeProperty =
      DependencyProperty.Register("SelectedTime", typeof(DateTime), typeof(TimeControl), new UIPropertyMetadata(DateTime.Now));

    public DateTime SelectedTime {
      get { return (DateTime)GetValue(SelectedTimeProperty); }
      set { SetValue(SelectedTimeProperty, value); }
    }

    private void Ellipse_MouseMove_1(object sender, MouseEventArgs e) {
    }


    //private double CalcPosition(double x, double y) {
    //  var rt = _selectedPointer.RenderTransform as RotateTransform;
    //  var radius = Clock.ActualHeight / 2;

    //  rt.Angle = (( Math.Atan2(y - radius, x - radius) * 180 / Math.PI ) + 90 );

    //  Console.WriteLine(" x: " + x + " y: " + y + ", angle: " + rt.Angle);
    //  return rt.Angle;

    //}

    private void clockGrid_MouseMove(object sender, MouseEventArgs e) {

      if( e.LeftButton == MouseButtonState.Pressed ) {

        //var x = e.GetPosition((IInputElement)Clock).X;
        //var y = e.GetPosition((IInputElement)Clock).Y;

        //HandleMousePress(x, y);
      }

    }

    private void HandleMousePress(double x, double y) {
      //var angle = CalcPosition(x, y);

      //if( angle < 0 ) 
      //  angle += 360;


      //var hour = (Math.Round((angle / 360) * 12))+1;
      //var min = ( Math.Round(( angle / 360 ) * 12) ) + 1;

      //if( _selectedPointer == MinArm ) {

      //  tbHour.Text = (Math.Round((angle / 360) * 12, 0)).ToString();
      //}

    }


    bool _updating = false;

    private void ClockControl_ValueChanged_1(object sender, RoutedEventArgs e) {
      _updating = true;
      try {
        if( clock.SelectedArm == TimeArm.Hour ) {

          SetTextValue(tbHour, clock.Hour);

        } else if( clock.SelectedArm == TimeArm.Minute ) {

          SetTextValue(tbMin, clock.Minute);
        }

      } finally {
        _updating = false;
      }

    }

    private void SetTextValue(TextBox tb, int value) {
      if( !tb.IsFocused )
        tb.Focus();
      tb.Text = value.ToString();
      tb.SelectAll();
    }

    private void tbHour_GotFocus(object sender, RoutedEventArgs e) {
      clock.SelectedArm = TimeArm.Hour;
    }

    private void tbMin_GotFocus(object sender, RoutedEventArgs e) {
      clock.SelectedArm = TimeArm.Minute;
    }


    public void SetValue(int hour, int minute, int second) {
      clock.SetValue(hour, minute, second);

      tbHour.Text = hour.ToString();
      tbMin.Text = minute.ToString();
      tbSec.Text = second.ToString();
    }


    private void tbHour_TextChanged(object sender, TextChangedEventArgs e) {
      if( !_updating )
        clock.SetHour(tbHour.Text.Convert(12));
    }
    private void tbMin_TextChanged(object sender, TextChangedEventArgs e) {
      if( !_updating )
        clock.SetMinute(tbMin.Text.Convert(15));
    }

    private void TextInputLabelButton_Click_1(object sender, RoutedEventArgs e) {
      HideControl();
    }

    private void UserControl_LostFocus_1(object sender, RoutedEventArgs e) {
      //
    }

    private void UserControl_LostKeyboardFocus_1(object sender, KeyboardFocusChangedEventArgs e) {

      if( e.NewFocus is ScrollViewer )
        return; // ignore ScrollViewer control, as it always selected when moving time-arms

      DependencyObject o = (DependencyObject)e.NewFocus;
      bool isParent = e.NewFocus == this;
      while( !isParent && ( o = VisualTreeHelper.GetParent(o) ) != null ) {
        if( o == this )
          isParent = true;
      }

      if( !isParent )
        HideControl();

    }

    private void HideControl() {
      var hour = _timeOfDay == TimeOfDay.AM ? clock.Hour : clock.Hour + 12;

      SelectedTime = new DateTime(1979, 01, 03, hour, clock.Minute, clock.Second);
      
      OnSelectedTimeChanged();
      
      this.Visibility = System.Windows.Visibility.Hidden;
    }


    private void OnSelectedTimeChanged() {
      RaiseEvent(new RoutedEventArgs(SelectedTimeChangedEvent));
    }

    public static readonly RoutedEvent SelectedTimeChangedEvent = EventManager.RegisterRoutedEvent("SelectedTimeChanged",
      RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(TimeControl));

    public event RoutedEventHandler SelectedTimeChanged {
      add { AddHandler(SelectedTimeChangedEvent, value); }
      remove { RemoveHandler(SelectedTimeChangedEvent, value); }
    }

    private void TimeOfDay_Click(object sender, RoutedEventArgs e) {
      if( _timeOfDay == TimeOfDay.PM )
        SetTimeOfDay(TimeOfDay.AM);
      else
        SetTimeOfDay(TimeOfDay.PM);
    }


  }
}
