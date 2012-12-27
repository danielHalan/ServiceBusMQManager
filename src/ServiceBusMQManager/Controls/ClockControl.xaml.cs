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
using Microsoft.Expression.Shapes;
using ServiceBusMQ;

namespace ServiceBusMQManager.Controls {

  public enum TimeArm { Hour = 0, Minute, Second }


  /// <summary>
  /// Interaction logic for ClockControl.xaml
  /// </summary>
  public partial class ClockControl : UserControl {


    static readonly Brush BRUSH_CIRCLE = new SolidColorBrush(Color.FromRgb(0x02, 0xAF, 0xFF)); // 02AFFF
    static readonly Brush BRUSH_CIRCLE_SHADOW = new SolidColorBrush(Color.FromRgb(0x00, 0xA0, 0xEB)); // 00A0EB

    static readonly Brush BRUSH_CIRCLE_HOUR = new SolidColorBrush(Color.FromRgb(0xDB, 0xF4, 0xFF)); // DBF4FF

    

    static readonly Pen PEN_WHITE = new Pen(Brushes.White, 2);
    static readonly Pen PEN_GRAY = new Pen(Brushes.LightGray, 2);
    static readonly Pen PEN_BLACK = new Pen(Brushes.Black, 2);
    static readonly Pen PEN_MINUTE = new Pen(BRUSH_CIRCLE_HOUR, 2);
    
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
      HourArmTreshold.Height = MinArm.Height + 16;
      
      HourArm.Margin = new Thickness(_r - ( HourArm.ActualWidth / 2 ), _r, 0, 0);
      HourArmTreshold.Margin = new Thickness(HourArm.Margin.Left - 8, HourArm.Margin.Top, 0, 0);

      MinArm.Height = ( _r * 0.70 );
      MinArmTreshold.Height = MinArm.Height + 16;

      MinArm.Margin = new Thickness(_r - ( MinArm.ActualWidth / 2 ), _r, 0, 0);
      MinArmTreshold.Margin = new Thickness(MinArm.Margin.Left - 10, MinArm.Margin.Top, 0, 0);

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



    StreamGeometry DrawArc(DrawingContext drawingContext) {
      StreamGeometry geometry = new StreamGeometry();
      geometry.FillRule = FillRule.EvenOdd;

      using( StreamGeometryContext context = geometry.Open() ) {
        _DrawArcToStream(context);
      }

      // Freeze the geometry for performance benefits
      geometry.Freeze();

      drawingContext.DrawGeometry(BRUSH_CIRCLE_SHADOW, null, geometry);

      return geometry;

    }

    void _DrawArcToStream(StreamGeometryContext context) {

      var rotationAngle = 65F;
      var innerRadius = 0F;
      var centerX = _r;
      var centreY = _r;
      var wedgeAngle = 150F;

      Point startPoint = new Point(centerX, centreY);

      Point innerArcStartPoint = Tools.ComputeCartesianCoordinate(rotationAngle, innerRadius);
      innerArcStartPoint.Offset(centerX, centreY);

      Point innerArcEndPoint = Tools.ComputeCartesianCoordinate(rotationAngle + wedgeAngle, innerRadius);
      innerArcEndPoint.Offset(centerX, centreY);

      Point outerArcStartPoint = Tools.ComputeCartesianCoordinate(rotationAngle, _r);
      outerArcStartPoint.Offset(centerX, centreY);

      Point outerArcEndPoint = Tools.ComputeCartesianCoordinate(rotationAngle + wedgeAngle, _r);
      outerArcEndPoint.Offset(centerX, centreY);

      bool largeArc = wedgeAngle > 180.0;

      Size outerArcSize = new Size(_r, _r);
      Size innerArcSize = new Size(innerRadius, innerRadius);

      context.BeginFigure(innerArcStartPoint, true, true);
      
      context.LineTo(outerArcStartPoint, true, true);
      context.ArcTo(outerArcEndPoint, outerArcSize, 0, largeArc, SweepDirection.Clockwise, true, true);
      
      context.LineTo(innerArcEndPoint, true, true);
      context.ArcTo(innerArcStartPoint, innerArcSize, 0, largeArc, SweepDirection.Counterclockwise, true, true);
    }

    protected override void OnRender(System.Windows.Media.DrawingContext g) {
      base.OnRender(g);

      if( _r == 0 )
        return;

      // Draw clock 
      g.DrawEllipse(BRUSH_CIRCLE, null, _center, _r, _r);

      DrawArc(g);

      g.DrawEllipse(Brushes.White, PEN_WHITE, _center, 2, 2);

      // Draw Hour marks
      int inc = 360 / 12;
      for( int i = 0; i < 12; i++ ) {
        if( i % 3 == 0 ) {
          var centerPoint = PointOnCircle(_r - 13, inc * i, _center);

          g.DrawEllipse(BRUSH_CIRCLE_HOUR, null, centerPoint, 4, 4);

        } else {
          var topPoint = PointOnCircle(_r - 12, inc * i, _center);
          var bottomPoint = PointOnCircle(_r - 21, inc * i, _center);

          g.DrawLine(PEN_MINUTE, topPoint, bottomPoint);
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

        var point = e.GetPosition((IInputElement)this);

        HandleMousePress(point.X, point.Y);
      }

    }

    List<DependencyObject> hitResultsList = new List<DependencyObject>();
    private void UserControl_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e) {
      var pt = e.GetPosition((IInputElement)this);

      // Clear the contents of the list used for hit test results.
      hitResultsList.Clear();

      // Set up a callback to receive the hit test result enumeration.
      for( int x = 1; x < 10; x++ ) {

        VisualTreeHelper.HitTest(this, new HitTestFilterCallback(MyHitTestFilter),
            new HitTestResultCallback(MyHitTestResult),
            new PointHitTestParameters(new Point(pt.X + x, pt.Y)));

        // Perform actions on the hit test results list. 
        if( hitResultsList.Count > 0 ) {
          Console.WriteLine("Number of Visuals Hit: " + hitResultsList.Count);
          break;
        }
      }


      HandleMousePress(pt.X, pt.Y);
    }

    // Return the result of the hit test to the callback. 
    public HitTestResultBehavior MyHitTestResult(HitTestResult result) {
      // Add the hit test result to the list that will be processed after the enumeration.
      hitResultsList.Add(result.VisualHit);

      // Set the behavior to return visuals at all z-order levels. 
      return HitTestResultBehavior.Continue;
    }

    public HitTestFilterBehavior MyHitTestFilter(DependencyObject o) {
      // Test for the object value you want to filter. 
      if( o.GetType() != typeof(Rectangle) )
        // Visual object and descendants are NOT part of hit test results enumeration. 
        return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
      else return HitTestFilterBehavior.Continue;

    }


    private double CalcPosition(double x, double y) {

      var y1 = _r - y;
      var x1 = _r - x;
      var angle = ( ( Math.Atan2(y1, x1) * 180F / Math.PI ) + 360 ) % 360;

      return ( angle + 90 ) % 360;


    }

    private void HandleMousePress(double x, double y) {
      var angle = CalcPosition(x, y);

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

      SetArmAngle(_selectedArmCtl, angle + 180);

      if( _selectedArmCtl == HourArm ) {
        Hour = timeValue;
      } else {
        Minute = timeValue;
      }

      OnValueChanged();
    }

    private void SetArmAngle(Rectangle arm, double angle) {
      var rt = arm.RenderTransform as RotateTransform;
      rt.Angle = angle;

      rt = (RotateTransform)( ( arm == HourArm ) ? HourArmTreshold.RenderTransform : MinArmTreshold.RenderTransform );
      if( rt != null )
        rt.Angle = angle;

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

      if( _hours.Count > 0 ) 
        SetArmAngle(HourArm, _hours[Hour] + 180);
      
    }


    public void SetMinute(int minute) {
      Minute = minute;

      if( _mins.Count > 0 ) 
        SetArmAngle(MinArm, _mins[minute % 60] + 180);
      
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

    private void MinArmTreshold_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      SelectArm(MinArm);
    }

    private void HourArmTreshold_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e) {
      SelectArm(HourArm);
    }


  }
}
