#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    ClockControl.xaml.cs
  Created: 2012-12-24

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

  public enum TimeArm { Hour = 0, Minute, Second }


  /// <summary>
  /// Interaction logic for ClockControl.xaml
  /// </summary>
  public partial class ClockControl : UserControl {


    static readonly Brush CIRCLE_BRUSH = new SolidColorBrush(Color.FromRgb(0x02, 0xAF, 0xFF)); // 02AFFF
    static readonly Brush CIRCLE_SHADOW_BRUSH = new SolidColorBrush(Color.FromRgb(0x00, 0x9E, 0xE8)); // 009FE8
    

    static readonly Pen PEN_WHITE = new Pen(Brushes.White, 2);
    static readonly Pen PEN_GRAY = new Pen(Brushes.LightGray, 2);
    static readonly Pen PEN_BLACK = new Pen(Brushes.Black, 2);

    private Pen _pen;
    private double _r;
    private Point _center;

    public ClockControl() {
      InitializeComponent();

      _pen = new Pen(Brushes.Black, 2);


      SelectArm(HourArm);
    }

    private void SelectArm(Rectangle arm) {
      if( _selectedArmCtl != null )
        _selectedArmCtl.Stroke = Brushes.Transparent;

      _selectedArm = (TimeArm)Convert.ToInt16(arm.Tag);

      _selectedArmCtl = arm;
      _selectedArmCtl.Stroke = Brushes.Yellow;
    }
    private void SelectArm(TimeArm arm) {
      _selectedArm = arm;

      switch( arm ) {
        case TimeArm.Hour: SelectArm(HourArm); break;
        case TimeArm.Minute: SelectArm(MinArm); break;
        //case TimeArm.Second: SelectArm(HourArm); break;
      }

    }

    List<double> _hours = new List<double>();
    List<double> _mins = new List<double>();
    private Rectangle _selectedArmCtl;
    private TimeArm _selectedArm;


    private void UpdateControlRegion() {
      _r = ActualWidth / 2;

      HourArm.Height = ( _r * 0.55 );
      HourArm.Margin = new Thickness(_r - ( HourArm.ActualWidth / 2 ), _r, 0, 0);

      MinArm.Height = ( _r * 0.70 );
      MinArm.Margin = new Thickness(_r - ( MinArm.ActualWidth / 2 ), _r, 0, 0);

      _center = new Point(_r, _r);

      _hours.Clear();
      // calc hour positions
      int inc = 360 / 12;
      for( int i = 0; i < 12; i++ )
        _hours.Add(inc * i);

      _mins.Clear();
      inc = 360 / 60;
      for( int i = 0; i < 60; i++ )
        _mins.Add(inc * i);


      InvalidateVisual();
    }


    public System.Windows.Point PointOnCircle(double radius, double angleInDegrees, Point origin) {

      // Convert from degrees to radians via multiplication by PI/180        
      double x = (double)( radius * Math.Cos(angleInDegrees * Math.PI / 180F) ) + origin.X;
      double y = (double)( radius * Math.Sin(angleInDegrees * Math.PI / 180F) ) + origin.Y;

      return new System.Windows.Point(x, y);
    }


    void DrawArc(DrawingContext drawingContext) {
      // setup the geometry object
      PathGeometry geometry = new PathGeometry();
      PathFigure figure = new PathFigure();
      figure.IsClosed = true;

      geometry.Figures.Add(figure);
      figure.StartPoint = new Point(_r, ( _r * 2 ));

      // add the arc to the geometry
      figure.Segments.Add(new ArcSegment(new Point(0, _r), new Size(_r, _r), 0, false, SweepDirection.Clockwise, false));

      // draw the arc
      drawingContext.DrawGeometry(CIRCLE_SHADOW_BRUSH, null, geometry);
    }

    protected override void OnRender(System.Windows.Media.DrawingContext g) {
      base.OnRender(g);

      if( _r == 0 )
        return;

      // Draw clock 
      g.DrawEllipse(CIRCLE_BRUSH, null, _center, _r, _r);

      DrawArc(g);
      
      g.DrawEllipse(Brushes.White, PEN_WHITE, _center, 2, 2);

      // Draw Hour marks
      int inc = 360 / 12;
      for( int i = 0; i < 12; i++ ) {
        if( i % 3 == 0 ) {
          var centerPoint = PointOnCircle(_r - 13, inc * i, _center);

          g.DrawEllipse(Brushes.White, PEN_WHITE, centerPoint, 4, 4);

        } else {
          var topPoint = PointOnCircle(_r - 12, inc * i, _center);
          var bottomPoint = PointOnCircle(_r - 21, inc * i, _center);

          g.DrawLine(PEN_WHITE, topPoint, bottomPoint);
        }
      }



    }

    private void UserControl_SizeChanged_1(object sender, SizeChangedEventArgs e) {
      UpdateControlRegion();
    }
    private void UserControl_SourceUpdated_1(object sender, DataTransferEventArgs e) {
    }
    private void UserControl_Loaded_1(object sender, RoutedEventArgs e) {
      UpdateControlRegion();
    }
    private void UserControl_MouseMove_1(object sender, MouseEventArgs e) {


      if( e.LeftButton == MouseButtonState.Pressed ) {

        var x = e.GetPosition((IInputElement)this).X;
        var y = e.GetPosition((IInputElement)this).Y;

        HandleMousePress(x, y);
      }

    }


    private double CalcPosition(double x, double y) {

      var y1 = _r - y;
      var x1 = _r - x;
      var angle = ( ( Math.Atan2(y1, x1) * 180F / Math.PI ) + 360 ) % 360;

      return ( angle + 90 ) % 360;


    }

    private void HandleMousePress(double x, double y) {
      var angle = CalcPosition(x, y);

      var rt = _selectedArmCtl.RenderTransform as RotateTransform;


      List<double> list = null;
      int timeValue = 0;

      if( _selectedArm == TimeArm.Hour )
        list = _hours;

      else if( _selectedArm == TimeArm.Minute )
        list = _mins;


      var index = GetMatchingIndex(list, angle);

      if( index == -1 )
        index = list.Count - 1;


      if( index != 0 )
        timeValue = index;
      else timeValue = list.Count;

      if( _selectedArm == TimeArm.Minute ) {
        if( timeValue == 60 )
          timeValue = 0;
      }

      angle = list[index];

      rt.Angle = angle + 180;

      if( _selectedArmCtl == HourArm ) {
        Hour = timeValue;
      } else {
        Minute = timeValue;
      }

      OnValueChanged();
    }

    private void OnValueChanged() {
      RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
    }

    public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged",
      RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(ClockControl));

    public event RoutedEventHandler ValueChanged {
      add { AddHandler(ValueChangedEvent, value); }
      remove { RemoveHandler(ValueChangedEvent, value); }
    }



    private int GetMatchingIndex(List<double> list, double angle) {
      int index = -1;
      double prevH = list[list.Count - 1];
      double timeAngle = ( angle + 180 ) % 360;

      double treshold = ( 360 / list.Count ) / 2;
      for( int i = 0; i < list.Count; i++ ) {
        var h = list[i];

        if( ( prevH + treshold <= timeAngle && timeAngle < h ) || // before the hour w/ treshold

           ( timeAngle >= h && timeAngle <= ( h + treshold ) ) ) { // after the hour w/ treshold
          angle = h;
          index = i;
          break;
        }

        prevH = h;
      }

      return index;
    }

    public void SetValue(int hour, int minute, int second) {

      SetHour(hour);
      SetMinute(minute);
      SetSecond(second);
    }
    public void SetHour(int hour) {
      Hour = hour % 12;

      if( _hours.Count > 0 ) {
        var angle = _hours[Hour];

        var rt = HourArm.RenderTransform as RotateTransform;
        rt.Angle = angle + 180;
      }
    }


    public void SetMinute(int minute) {
      Minute = minute;

      if( _mins.Count > 0 ) {
        var angle = _mins[minute % 60];

        var rt = MinArm.RenderTransform as RotateTransform;
        rt.Angle = angle + 180;
      }
    }
    private void SetSecond(int second) {
      Second = second;
    }


    public int Hour { get; private set; }
    public int Minute { get; private set; }
    public int Second { get; private set; }

    public TimeArm SelectedArm {
      get { return _selectedArm; }
      set { SelectArm(value); }
    }

    private void Arm_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      SelectArm(sender as Rectangle);
    }

  }
}
