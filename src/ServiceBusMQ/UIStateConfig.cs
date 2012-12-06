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
    private static readonly string KEY_CONTROL = "_CTL";

    public class UIWindowState {
      public double Left { get; set; }
      public double Top { get; set; }
      public double Height { get; set; }
      public double Width { get; set; }

      [JsonIgnore]
      public bool IsEmpty { get { return Left + Top + Height + Width == 0; } }

      public UIWindowState() {
      }

      public UIWindowState(double left, double top, double width, double height) {
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

    //Dictionary<string, string> _config = new Dictionary<string, string>();

    UIStateData _data;

    public UIStateConfig() {

      _fileName = SbmqSystem.AppDataPath + "\\ui.state.dat";

      Load();
    }


    public void StoreWindowState(Window window) {
      var r = new UIWindowState(window.Left, window.Top, window.Width, window.Height);

      if( !_data.WindowStates.ContainsKey(window.Name) )
        _data.WindowStates.Add(window.Name, r);
      else _data.WindowStates[window.Name] = r;

      Save();
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

    public bool RestoreWindowState(Window window) {

      if( _data.WindowStates.ContainsKey(window.Name) ) {
        var r = _data.WindowStates[window.Name];

        window.Left = r.Left;
        window.Top = r.Top;
        window.Width = r.Width;
        window.Height = r.Height;

        return true;

      } else return false;

    }

    private void UpdateAlwaysOnTop(bool value) {
      UpdateValue(KEY_ALWAYSONTOP, value);
    }
    private bool GetAlwaysOnTop() {
      object v;
      if( !GetValue(KEY_ALWAYSONTOP, out v) )
        return true; // default true

      return (bool)v;
    }

    //public string SelectedQueues { get; private set; }
    public Boolean AlwaysOnTop { get { return GetAlwaysOnTop(); } set { UpdateAlwaysOnTop(value); } }

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
