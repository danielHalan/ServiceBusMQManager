#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    ComplexDataViewControl.xaml.cs
  Created: 2012-11-20

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ServiceBusMQ;

namespace ServiceBusMQManager.Controls {



  /// <summary>
  /// Interaction logic for ComplexDataTypeControl.xaml
  /// </summary>
  public partial class ComplexDataViewControl : UserControl {

    const int CONTROL_WIDTH = 610;
    const int SCROLLTO_CONTROL_SPEED = 400; // ms

    DataTemplateManager _tempMgr;

    Stack _panels = new Stack();

    StackPanel _mainPanel;

    private bool _isListItem;

    class PanelInfo {
      public Type DataType;
      public string AttributeName;
      public StackPanel ChildControl;
    }


    public bool IsValid {
      get { return _panels.Count > 0 && _mainPanel != null; }
    }

    public ComplexDataViewControl() {
      InitializeComponent();

    }


    private void CreateTitlePart(StackPanel p, Type type, string attribute, object value) {

      // Add Title
      var titleControl = new ComplexDataTitleControl(type.Name, _panels.Count > 0);
      titleControl.BackClick += titleControl_BackClick;

      p.Children.Add(titleControl);

      // Add Template bar
      if( _panels.Count > 0 ) {
        var tempControl = new ComplexDataTemplateControl(type, GetTempManager());
        tempControl.CreateTemplate += tempControl_CreateTemplate;
        tempControl.DeleteTemplate += tempControl_DeleteTemplate;
        tempControl.TemplateSelected += tempControl_TemplateSelected;
        tempControl.SelectTemplate(value);
        p.Children.Add(tempControl);

        var img = new Image();
        img.VerticalAlignment = System.Windows.VerticalAlignment.Top;
        img.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
        img.Height = 20;
        img.Width = 10;
        img.Margin = new Thickness(0, -15, 0, 0);
        img.Source = BitmapFrame.Create(_GetImageResourceStream("template-fold.png"));

        p.Children.Add(img);
      }

    }

    private Stream _GetImageResourceStream(string name) {
      return this.GetType().Assembly.GetManifestResourceStream("ServiceBusMQManager.Images." + name);
    }  


    private DataTemplateManager GetTempManager() {
      if( _tempMgr == null ) {
        _tempMgr = new DataTemplateManager();
      }

      return _tempMgr;
    }

    private StackPanel CreateDataPanel(Type type, string attribute, object value) {

      StackPanel mainPanel = new StackPanel();
      mainPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
      mainPanel.Margin = new Thickness(0, 0, 0, 0);
      mainPanel.Background = Brushes.Transparent;
      mainPanel.Width = CONTROL_WIDTH; // 480;
      
      var panelInfo = new PanelInfo() { DataType = type, AttributeName = attribute, ChildControl = mainPanel };
      mainPanel.Tag = panelInfo;

      CreateTitlePart(mainPanel, type, attribute, value);

      double h = 0;
      foreach( UserControl c in mainPanel.Children.OfType<UserControl>() )
        h += c.Height;

      ScrollViewer scroller = new ScrollViewer();
      scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
      scroller.CanContentScroll = false;
      scroller.Width = CONTROL_WIDTH; // 500
      scroller.Margin = new Thickness(0, 0, 0, 0);
      scroller.Height = this.ActualHeight - h;
      scroller.Style = FindResource("FavsScrollViewer") as Style;

      // Stack Panel for Input Controls
      StackPanel p = new StackPanel();
      p.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
      //p.Margin = new Thickness(0,0,0,0);
      p.Background = Brushes.Transparent;
      p.Tag = panelInfo;
      p.Width = CONTROL_WIDTH - 30; // remove space for the scrollbar;

      scroller.Content = p;

      mainPanel.Children.Add(scroller);

      _panels.Push(p);

      theStack.Children.Add(mainPanel);

      this.Width = theStack.Children.Count * 1000;

      return p;
    }

    void tempControl_DeleteTemplate(object sender, TemplateEventArgs e) {
      var p = _panels.Peek() as StackPanel;
      PanelInfo pi = p.Tag as PanelInfo;

      object instance = CreateTypeInstance(p);
      _tempMgr.Delete(e.Name, pi.DataType);

    }
    void tempControl_TemplateSelected(object sender, TemplateSelectedEventArgs e) {
      var p = _panels.Peek() as StackPanel;

      UpdateDataPanel(p, e.Type, e.Data);
    }
    void tempControl_CreateTemplate(object sender, TemplateEventArgs e) {

      var p = _panels.Peek() as StackPanel;
      PanelInfo pi = p.Tag as PanelInfo;

      object instance = CreateTypeInstance(p);
      _tempMgr.Store(e.Name, pi.DataType, instance, e.IsDefault);

      e.Value = instance;
    }


    public void SetDataType(Type t, object value) {

      if( !ScrollToMainPanel( () => _SetDataType(t, value), 0) )
        _SetDataType(t, value);

    }

    public void _SetDataType(Type t, object value) {

      theStack.Children.Clear();
      _panels.Clear();

      var p = CreateDataPanel(t, null, value);

      _mainPanel = p;

      BindDataPanel(p, t, value);
    }

    internal void Clear() {
      theStack.Children.Clear();
      _panels.Clear();

      _mainPanel = null;
    }

    private void BindDataPanel(StackPanel p, Type t, object value) {
      foreach( var prop in t.GetProperties().OrderBy(pr => pr.Name) ) {

        object v = value != null ? prop.GetValue(value, null) : null;

        if( v == null ) {
          var tmp = GetTempManager().GetDefault(prop.PropertyType.FullName);
          
          if( tmp != null )
            v = tmp.Object;
        }
          
        var ctl = new AttributeControl(prop.Name, prop.PropertyType, v);
        ctl.DefineComplextType += ctl_DefineComplextType;

        p.Children.Add(ctl);
      }
    }
    private void UpdateDataPanel(StackPanel p, Type t, object value) {
      var ctrls = p.Children.OfType<AttributeControl>();

      if( value != null ) {

        // Use Value.GetType() if its a different version
        foreach( var prop in value.GetType().GetProperties() ) {
          var ctrl = ctrls.Where(c => c.DisplayName == prop.Name).FirstOrDefault();

          if( ctrl != null )
            ctrl.Value = prop.GetValue(value, null);
        }

      } else { // Its null, clear all values

        foreach( AttributeControl ctrl in ctrls ) {
          ctrl.Value = Tools.GetDefault(t);
        }

      }

    }


    void ctl_DefineComplextType(object sender, ComplexTypeEventArgs e) {
      var p = CreateDataPanel(e.Type, e.AttributeName, e.Value);

      BindDataPanel(p, e.Type, e.Value);

      ScrollToNextControl(sender as ComplexDataInputControl);
    }

    void titleControl_BackClick(object sender, EventArgs e) {
      var p = _panels.Pop() as StackPanel;
      PanelInfo pi = p.Tag as PanelInfo;

      object instance = CreateTypeInstance(p);

      var currPanel = _panels.Peek() as StackPanel;

      SetAttributeValue(currPanel, pi.AttributeName, instance);

      ScrollBackToPreviousControl(pi);
    }

    private void ScrollToNextControl(ComplexDataInputControl btn) {
      btn.IsIgnoringClicks = true;

      // Show next Control
      DoubleAnimation anim = new DoubleAnimation();
      anim.From = ( _panels.Count - 2 ) * -CONTROL_WIDTH;
      anim.To = anim.From - CONTROL_WIDTH;
      anim.Duration = new Duration(new TimeSpan(0, 0, 0, 0, SCROLLTO_CONTROL_SPEED));
      anim.RepeatBehavior = new RepeatBehavior(1);
      anim.Completed += (s2, e2) => { btn.IsIgnoringClicks = false; };
      anim.AccelerationRatio = 1;

      TranslateTransform trans = new TranslateTransform();

      theStack.RenderTransform = trans;
      trans.BeginAnimation(TranslateTransform.XProperty, anim);
    }

    private void ScrollBackToPreviousControl(PanelInfo pi) {

      DoubleAnimation anim = new DoubleAnimation();
      anim.From = ( _panels.Count ) * -CONTROL_WIDTH;
      anim.To = anim.From + CONTROL_WIDTH;
      anim.Duration = new Duration(new TimeSpan(0, 0, 0, 0, SCROLLTO_CONTROL_SPEED));
      anim.RepeatBehavior = new RepeatBehavior(1);
      anim.Completed += (s2, e2) => { theStack.Children.Remove(pi.ChildControl); };
      anim.AccelerationRatio = 0.5;
      anim.DecelerationRatio = 0.5;
      TranslateTransform trans = new TranslateTransform();

      theStack.RenderTransform = trans;
      trans.BeginAnimation(TranslateTransform.XProperty, anim);
    }

    private bool ScrollToMainPanel(Action onCompleted, int scrollTime) {

      if( _panels.Count > 1 ) {

        DoubleAnimation anim = new DoubleAnimation();
        anim.From = ( _panels.Count ) * -CONTROL_WIDTH;
        anim.To = 0;
        anim.Duration = new Duration(new TimeSpan(0, 0, 0, 0, scrollTime));
        anim.RepeatBehavior = new RepeatBehavior(1);
        anim.Completed += (s2, e2) => { onCompleted();  };
        anim.AccelerationRatio = 0.5;
        TranslateTransform trans = new TranslateTransform();

        theStack.RenderTransform = trans;
        trans.BeginAnimation(TranslateTransform.XProperty, anim);

        return true;
      
      
      } else return false;
    }


    private void SetAttributeValue(StackPanel panel, string name, object value) {
      PanelInfo pi = panel.Tag as PanelInfo;

      AttributeControl ac = panel.Children.OfType<AttributeControl>().Where(c => c.DisplayName == name).FirstOrDefault();
      if( ac != null )
        ac.Value = value;


    }


    public object CreateObject() {
      if( IsValid ) {
      
        if( _panels.Count > 1 ) { // Scroll back to Main Panel
          var scrollTime = _panels.Count * 120;
          ScrollToMainPanel(() => { theStack.Children.OfType<StackPanel>().
            Where(s => s.Tag != _mainPanel.Tag).
            ForEach(s => s.Visibility = Visibility.Hidden ); }, scrollTime);
          WindowTools.Sleep(scrollTime);
        }

        while( _panels.Count > 1 ) { // Save all sub objects
          var p = _panels.Pop() as StackPanel;
          PanelInfo pi = p.Tag as PanelInfo;

          object instance = CreateTypeInstance(p);

          var currPanel = _panels.Peek() as StackPanel;

          SetAttributeValue(currPanel, pi.AttributeName, instance);
        }

        return CreateTypeInstance(_mainPanel);
      } else return null;
    
    }
    private object CreateTypeInstance(StackPanel panel) {
      Dictionary<string, object> values = new Dictionary<string, object>();

      foreach( AttributeControl atr in panel.Children.OfType<AttributeControl>() ) {
        values.Add(atr.DisplayName, atr.Value);
      }

      Type type = ( panel.Tag as PanelInfo ).DataType;

      return Tools.CreateInstance(type, values);
    }


    public bool IsListItem { get; set; }

  }
}
