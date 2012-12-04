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
using System.Xml.Linq;

namespace ServiceBusMQ {

  public class UIStateConfig {

    string _fileName;

    Dictionary<string, string> _config = new Dictionary<string,string>();


    public UIStateConfig() {

      _fileName = SbmqSystem.AppDataPath + "\\ui.state";

      // Set defaults
      AlwaysOnTop = true;
      ContentWindowRect = Rect.Empty;
      MainWindowRect = Rect.Empty;
      SelectedQueues = "commands;events";
    }

    public void UpdateButtonState(bool? commands, bool? events, bool? messages, bool? errors) {

      // Save pressed buttons
      List<string> q = new List<string>();
      if( (bool)commands )
        q.Add("commands");

      if( (bool)events )
        q.Add("events");

      if( (bool)messages )
        q.Add("messages");

      if( (bool)errors )
        q.Add("errors");

      SelectedQueues = string.Join(";", q.ToArray());
    }
    public void UpdateMainWindowState(Window window) {
      MainWindowRect = new Rect(new Point(window.Left, window.Top), new Size(window.Width, window.Height));
    }
    public void UpdateContentWindowState(Window window) {
      ContentWindowRect = new Rect(new Point(window.Left, window.Top), new Size(window.Width, window.Height));
    }

    public void UpdateAlwaysOnTop(bool value) {
      AlwaysOnTop = value;
    }

    public string SelectedQueues { get; private set; }
    public Rect MainWindowRect { get; private set; }
    public Rect ContentWindowRect { get; private set; }
    public Boolean AlwaysOnTop { get;  set; }

    public void Save() {
      XDocument doc = File.Exists(_fileName) ? XDocument.Load(_fileName) : new XDocument( new XElement("settings") );
    
      foreach( var prop in this.GetType().GetProperties() ) {
        SetConfigValue(doc, prop.Name, GetStringValue(prop.GetValue(this, null)) );
      }

      //foreach( var k in _config)
      //  SetConfigValue(doc, k.Key, k.Value);
    
      doc.Save(_fileName);
    }

    public void Load() {

      XDocument doc = File.Exists(_fileName) ? XDocument.Load(_fileName) : new XDocument(new XElement("settings"));

      var props = this.GetType().GetProperties().ToArray();
      foreach( XElement e in doc.Root.Elements() ) {
        var pr = props.Single( p => p.Name == e.Name );
        
        pr.SetValue(this, GetObjectValue(e.Value, pr.PropertyType), null);
      }
      

      //foreach( var k in _config)
      //  SetConfigValue(doc, k.Key, k.Value);

      doc.Save(_fileName);
    
    }

    private object GetObjectValue(string str, Type type) {
      if( type == typeof(Rect) ) {
        int[] arr = str.Split(';').Select( i => Convert.ToInt32(i) ).ToArray();

        return new Rect(arr[0], arr[1], arr[2], arr[3]);


      } else if( type == typeof(bool) )
        return Convert.ToBoolean(str);
        
      else if( type == typeof(string) ) 
        return str;

      else throw new NotSupportedException("Not supported config type = " + type.ToString());

    }

    private string GetStringValue(object obj) {
      if( obj is Rect ) {
        var r = (Rect)obj;
        return string.Concat(Convert.ToInt32(r.Left), ';',  
                             Convert.ToInt32(r.Top), ';', 
                             Convert.ToInt32(r.Width), ';', 
                             Convert.ToInt32(r.Height) );
      
      } else if( obj is string )
        return (string)obj;
      
      else if( obj != null ) 
        return obj.ToString();

      else return string.Empty;
    }

    void SetConfigValue(string name, string value) {
      if( _config.ContainsKey(name) )
        _config[name] = value;
      else _config.Add(name, value);
    }

    void SetConfigValue(XDocument doc, string name, string value) {
      if( doc.Root.Element(name) == null )
        doc.Root.Add(new XElement(name));

      doc.Root.Element(name).Value = value;
    }
  }
}
