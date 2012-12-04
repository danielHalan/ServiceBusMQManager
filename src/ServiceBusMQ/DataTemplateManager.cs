#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    DataTemplateManager.cs
  Created: 2012-11-26

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
using System.Runtime.Serialization.Formatters;
using System.Text;
using Newtonsoft.Json;

namespace ServiceBusMQ {
  public class DataTemplateManager {

    string _templateFile;

    public class DataTemplate {

      public string Name { get; set; }
      public string TypeName { get; set; }

      public object Object { get; set; }
    }

    List<DataTemplate> _templates = new List<DataTemplate>();

    public List<DataTemplate> Templates { get { return _templates; } }

    public DataTemplateManager() {

      _templateFile =  SbmqSystem.AppDataPath + @"\templates.dat";

      LoadFromDisk();
    }


    void WriteToDisk() {
      JsonFile.Write(_templateFile, _templates);
    }

    void LoadFromDisk() {

      _templates = JsonFile.Read<List<DataTemplate>>(_templateFile);

      if( _templates == null )
        _templates = new List<DataTemplate>();
    }


    public void Store(string name, Type type, object obj) {

      DataTemplate temp = _templates.SingleOrDefault(t => t.TypeName == type.FullName && t.Name == name);
      if( temp == null ) {
        temp = new DataTemplate();
        temp.Name = name;
        temp.TypeName = type.FullName;

        _templates.Add(temp);
      }

      temp.Object = obj;

      WriteToDisk();
    }

    public object Get(string name, Type type) {

      DataTemplate temp = _templates.FirstOrDefault(t => t.TypeName == type.FullName && t.Name == name);

      if( temp != null )
        return temp.Object;
      else return null;
    }

    public void Delete(string name, Type type) {

      var item = _templates.FirstOrDefault(t => t.Name == name && t.TypeName == type.FullName);

      if( item != null )
        _templates.Remove(item);
 
      WriteToDisk();
    }
  }
}
