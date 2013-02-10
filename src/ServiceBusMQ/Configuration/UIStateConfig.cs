#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    StateConfig.cs
  Created: 2012-09-22

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace ServiceBusMQ {

  public class UIStateConfig {
    
    private static readonly string KEY_ALWAYSONTOP = "_ALWAYSONTOP";
    private static readonly string KEY_ISMINIMIZED = "_ISMINIMIZED";
    private static readonly string KEY_CONTROL = "_CTL";


    public enum WindowPositionType { Custom, TopLeft, BottomLeft, TopRight, BottomRight }

    public class UIWindowState {

      public WindowPositionType WindowPosition { get; set; }
      
      public double Left { get; set; }
      public double Top { get; set; }
      public double Height { get; set; }
      public double Width { get; set; }

      [JsonIgnore]
      public bool IsEmpty { get { return Left + Top + Height + Width == 0; } }

      public UIWindowState() {
        WindowPosition = WindowPositionType.Custom;
      }

      public UIWindowState(WindowPositionType winPos, double width, double height) {
        WindowPosition = winPos;
        
        Width = width;
        Height = height;
      }

      public UIWindowState(double left, double top, double width, double height) {
        WindowPosition = WindowPositionType.Custom;

        Left = left;
        Top = top;
        Width = width;
        Height = height;
      }

    }

    public class UIStateData {
      public Dictionary<string, UIWindowState> WindowStates { get; set; }
      public Dictionary<string, object> Values { get; set; }

      public UIStateData() {
        WindowStates = new Dictionary<string, UIWindowState>();
        Values = new Dictionary<string, object>();
      }
    }


    string _fileName;

    UIStateData _data;

    public UIStateConfig() {

      _fileName = SbmqSystem.AppDataPath + "\\ui.state.dat";

      Load();
    }


    public void StoreWindowState(Window window) {
      WindowPositionType winPos = GetWindowPosition(window);


      var r = winPos == WindowPositionType.Custom ? 
        new UIWindowState(window.Left, window.Top, window.Width, window.Height) : 
        new UIWindowState(winPos, window.Width, window.Height);

      if( !_data.WindowStates.ContainsKey(window.Name) )
        _data.WindowStates.Add(window.Name, r);
      else _data.WindowStates[window.Name] = r;

      Save();
    }
    public bool RestoreWindowState(Window window) {

      if( _data.WindowStates.ContainsKey(window.Name) ) {
        var r = _data.WindowStates[window.Name];

        window.Width = r.Width;
        window.Height = r.Height;

        if( r.WindowPosition == WindowPositionType.Custom ) {

          window.Left = r.Left;
          window.Top = r.Top;

        } else SetWindowPosition(window, r.WindowPosition);

        MakeSureVisibility(window);

        return true;

      } else return false;

    }

    private WindowPositionType GetWindowPosition(Window w) {
      var s = WpfScreen.GetScreenFrom(w).WorkingArea;
      
      if( (w.Left + w.Width) == s.Width ) {

        if( w.Top == 0 )
          return WindowPositionType.TopRight;

        else if( w.Top + w.Height == s.Height )
          return WindowPositionType.BottomRight;

      } else if( w.Left == 0 ) {
      
        if( w.Top == 0 )
          return WindowPositionType.TopLeft;

        else if( w.Top + w.Height == s.Height )
          return WindowPositionType.BottomLeft;
      
      }

      return WindowPositionType.Custom;
    }
    private void SetWindowPosition(Window w, WindowPositionType winPos) {
      var s = WpfScreen.GetScreenFrom(w).WorkingArea;

      switch(winPos) {

        case WindowPositionType.BottomRight:
          w.Top = s.Height - w.Height;
          w.Left = s.Width - w.Width;
          break;

        case WindowPositionType.TopLeft:
          w.Top = 0;
          w.Left = 0;
          break;

        case WindowPositionType.BottomLeft:
          w.Top = s.Height - w.Height;
          w.Left = 0;
          break;

        case WindowPositionType.TopRight:
          w.Top = 0;
          w.Left = s.Width - w.Width;
          break;

      }
    }
    private void MakeSureVisibility(Window w) {
      var s = WpfScreen.GetScreenFrom(w).WorkingArea;
      
      if( w.Left + w.Width > s.Width )
        w.Left = s.Width - w.Width;

      if( w.Top + w.Height > s.Height )
        w.Top = s.Height - w.Height;

    }



    public void StoreControlState(Control control) {
      object value = null;

      if( control is CheckBox ) {
        value = ( control as CheckBox ).IsChecked;

      } else if( control is ToggleButton ) {
        value = ( control as ToggleButton ).IsChecked;

      } else if( control is TextBox ) {
        value = ( control as TextBox ).Text;

      } else if( control is ComboBox ) {
        value = ( control as ComboBox ).SelectedValue;

      } else return;

      UpdateValue(KEY_CONTROL + control.Name, value);
    }
    public void RestoreControlState(Control control, object def) {
      object value = null;

      if( !GetValue(KEY_CONTROL + control.Name, out value) )
        value = def;

      if( control is CheckBox ) {
        ( control as CheckBox ).IsChecked = (bool?)value;
      
      } else if( control is ToggleButton ) {
        ( control as ToggleButton ).IsChecked = (bool?)value;

      } else if( control is TextBox ) {
        ( control as TextBox ).Text = (string)value;

      } else if( control is ComboBox ) {
        ( control as ComboBox ).SelectedValue = value;

      } else return;

    }

    private T GetValue<T>(string key, T @default) {
      if( _data.Values.ContainsKey(key) ) {
        return  (T)_data.Values[key];

      } else return @default;
    }


    private bool GetValue(string key, out object value) {
      if( _data.Values.ContainsKey(key) ) {
        value = _data.Values[key];
        return true;

      } else {
        value = null;
        return false;
      }
    }
    private void UpdateValue(string key, object value) {
      if( _data.Values.ContainsKey(key) )
        _data.Values[key] = value;
      else _data.Values.Add(key, value);
    }


    public Boolean AlwaysOnTop { get { return GetValue<bool>(KEY_ALWAYSONTOP, true); } set { UpdateValue(KEY_ALWAYSONTOP, value); } }
    public Boolean IsMinimized { get { return GetValue<bool>(KEY_ISMINIMIZED, false); } set { UpdateValue(KEY_ISMINIMIZED, value); } }

    public void Save() {
      JsonFile.Write(_fileName, _data);
    }
    private void Load() {

      if( File.Exists(_fileName) )
        _data = JsonFile.Read<UIStateData>(_fileName);
      else _data = new UIStateData();

    }

  }
}
