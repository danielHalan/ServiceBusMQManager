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

    int CONTROL_WIDTH = 500;

    DataTemplateManager _tempMgr = new DataTemplateManager();

    //Type _dataType;

    Stack _panels = new Stack();

    StackPanel _mainPanel;
    
    private bool _isListItem;

    class PanelInfo {
      public Type DataType;
      public string AttributeName;
      public StackPanel ChildControl;
    }


    public ComplexDataViewControl() {
      InitializeComponent();

      CONTROL_WIDTH = 610;
    }


    private void CreateTitlePart(StackPanel p, Type type, string attribute, object value) {    
      
      // Add Title
      var titleControl = new ComplexDataTitleControl(type.Name, _panels.Count > 0);
      titleControl.BackClick += titleControl_BackClick;

      p.Children.Add(titleControl);

      // Add Template bar
      if( _panels.Count > 0 ) {
        var tempControl = new ComplexDataTemplateControl(type, _tempMgr);
        tempControl.CreateTemplate += tempControl_CreateTemplate;
        tempControl.DeleteTemplate += tempControl_DeleteTemplate;
        tempControl.TemplateSelected += tempControl_TemplateSelected;
        tempControl.SelectTemplate(value);
        p.Children.Add(tempControl);
      }

    }

    private StackPanel CreateDataPanel(Type type, string attribute, object value) {

      StackPanel mainPanel = new StackPanel();
      mainPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
      mainPanel.Margin = new Thickness(0, 0, 0, 0);
      mainPanel.Background = Brushes.Transparent;
      mainPanel.Width = CONTROL_WIDTH; // 480;
      //mainPanel.Tag = new PanelInfo() { DataType = type, AttributeName = attribute, ChildControl = mainPanel };

      CreateTitlePart(mainPanel, type, attribute, value);

      double h = 0;
      foreach(UserControl c in mainPanel.Children )
        h+= c.Height;

      ScrollViewer scroller = new ScrollViewer();
      scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
      scroller.CanContentScroll = true;
      scroller.Width = CONTROL_WIDTH; // 500
      scroller.Margin = new Thickness(0,0,0,0);
      scroller.Height = this.ActualHeight - h;

      // Stack Panel for Input Controls
      StackPanel p = new StackPanel();
      p.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
      //p.Margin = new Thickness(0,0,0,0);
      p.Background = Brushes.Transparent;
      p.Tag = new PanelInfo() { DataType = type, AttributeName = attribute, ChildControl = mainPanel };
      p.Width = CONTROL_WIDTH - 30; // 480;

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

      object instance = GetTypeInstance(p);
      _tempMgr.Delete(e.Name, pi.DataType);

    }
    void tempControl_TemplateSelected(object sender, TemplateSelectedEventArgs e) {
      var p = _panels.Peek() as StackPanel;

      UpdateDataPanel(p, e.Type, e.Data);
    }
    void tempControl_CreateTemplate(object sender, TemplateEventArgs e) {

      var p = _panels.Peek() as StackPanel;
      PanelInfo pi = p.Tag as PanelInfo;

      object instance = GetTypeInstance(p);
      _tempMgr.Store(e.Name, pi.DataType, instance);

      e.Value = instance;
    }


    public void SetDataType(Type t, object value) {
      theStack.Children.Clear();
      _panels.Clear();

      var p = CreateDataPanel(t, null, value);

      _mainPanel = p;

      BindDataPanel(p, t, value);
    }

    private void BindDataPanel(StackPanel p, Type t, object value) {
      foreach( var prop in t.GetProperties().OrderBy( pr => pr.Name ) ) {

        var ctl = new AttributeControl(prop.Name, prop.PropertyType, value != null ? prop.GetValue(value, null) : null);
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

      ComplexDataInputControl btn = sender as ComplexDataInputControl;
      btn.IsListItem = true;

      var p = CreateDataPanel(e.Type, e.AttributeName, e.Value);

      BindDataPanel(p, e.Type, e.Value);

      //this.Width = _panels.Count+1 * CONTROL_WIDTH;

      //Vector offset = VisualTreeHelper.GetOffset(theGrid);

      // Show next Control
      DoubleAnimation anim = new DoubleAnimation();
      anim.From = ( _panels.Count - 2 ) * -CONTROL_WIDTH;
      anim.To = anim.From - CONTROL_WIDTH;
      anim.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 400));
      anim.RepeatBehavior = new RepeatBehavior(1);
      anim.Completed += (s2, e2) => { btn.IsListItem = false; };

      TranslateTransform trans = new TranslateTransform();

      theStack.RenderTransform = trans;
      trans.BeginAnimation(TranslateTransform.XProperty, anim);

    }

    void titleControl_BackClick(object sender, EventArgs e) {
      // Setup new AttirbPane;l
      var p = _panels.Pop() as StackPanel;
      PanelInfo pi = p.Tag as PanelInfo;

      object instance = GetTypeInstance(p);

      var currPanel = _panels.Peek() as StackPanel;


      SetAttributeValue(currPanel, pi.AttributeName, instance);

      Vector offset = VisualTreeHelper.GetOffset(theGrid);

      // Show next Control
      DoubleAnimation anim = new DoubleAnimation();
      anim.From = ( _panels.Count ) * -CONTROL_WIDTH;
      anim.To = anim.From + CONTROL_WIDTH;
      anim.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 400));
      anim.RepeatBehavior = new RepeatBehavior(1);
      anim.Completed += (s2, e2) => { theStack.Children.Remove(pi.ChildControl); };
      TranslateTransform trans = new TranslateTransform();

      theStack.RenderTransform = trans;
      trans.BeginAnimation(TranslateTransform.XProperty, anim);

      //this.Width = _panels.Count * CONTROL_WIDTH;
    }

    private void SetAttributeValue(StackPanel panel, string name, object value) {
      PanelInfo pi = panel.Tag as PanelInfo;

      AttributeControl ac = panel.Children.OfType<AttributeControl>().Where(c => c.DisplayName == name).FirstOrDefault();
      if( ac != null )      
        ac.Value = value;


    }


    public object CreateObject() { 
      return GetTypeInstance(_mainPanel);
    }
    private object GetTypeInstance(StackPanel panel) {
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
