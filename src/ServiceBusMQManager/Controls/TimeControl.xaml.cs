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

    private TimeOfDay _timeOfDay;


    public TimeControl() {
      InitializeComponent();

      this.Visibility = System.Windows.Visibility.Hidden;
    }

    public void Show(DateTime time) {

      if( time.Hour >= 12 ) {
        SetTimeOfDay(TimeOfDay.PM);

      } else SetTimeOfDay(TimeOfDay.AM);


      clock.SetValue(time.Hour, time.Minute, time.Second);
      clock.SelectedArm = TimeArm.Hour;

      SetTextValue(tbSec, time.Second);
      SetTextValue(tbMin, time.Minute);

      this.Visibility = System.Windows.Visibility.Visible;
      SetTextValue(tbHour, time.Hour % 12);

    }
    private void SetTimeOfDay(TimeOfDay timeOfDay) {
      _timeOfDay = timeOfDay;

      btnTimeOfDay.Content = _timeOfDay.ToString();
    }

    public static readonly DependencyProperty SelectedTimeProperty =
      DependencyProperty.Register("SelectedTime", typeof(DateTime), typeof(TimeControl), new UIPropertyMetadata(DateTime.Now));

    public DateTime SelectedTime {
      get { return (DateTime)GetValue(SelectedTimeProperty); }
      set { SetValue(SelectedTimeProperty, value); }
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
      var hour = _timeOfDay == TimeOfDay.AM ? clock.Hour : ( clock.Hour + 12 ) % 24;

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

    private void UserControl_Loaded_1(object sender, RoutedEventArgs e) {
      //this.Focus();
    }

    private void UserControl_GotFocus_1(object sender, RoutedEventArgs e) {
      //this.Focus();
    }

    private void UserControl_LostFocus_1(object sender, RoutedEventArgs e) {
    }

    private void UserControl_PreviewLostKeyboardFocus_1(object sender, KeyboardFocusChangedEventArgs e) {

      if( e.OldFocus == btnTimeOfDay && !this.IsChildControl((DependencyObject)e.NewFocus) ) {

        if( clock.SelectedArm == TimeArm.Hour )
          tbHour.Focus();
        else tbMin.Focus();

        e.Handled = true;
      }
    }


  }
}
