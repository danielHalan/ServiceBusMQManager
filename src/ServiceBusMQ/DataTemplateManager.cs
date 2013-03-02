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
using Newtonsoft.Json;

namespace ServiceBusMQ {
  public class DataTemplateManager {

    string _templateFile;

    public class DataTemplate {

      public string Name { get; set; }
      public string TypeName { get; set; }

      public object Object { get; set; }
    
      [JsonIgnore]
      public string FileName { get; set; }
    }

    List<DataTemplate> _templates = new List<DataTemplate>();
    private string _templateFolder;
    private string _defaultsFile;

    public List<DataTemplate> Templates { get { return _templates; } }

    public Dictionary<string, string> _defaults;


    public DataTemplateManager() {

      _templateFolder = SbmqSystem.AppDataPath + @"\templates\";
      _defaultsFile = _templateFolder + "template.def";

      if( !Directory.Exists(_templateFolder) )
        Directory.CreateDirectory(_templateFolder);

      Load();
    }


    private void Load() {

      foreach( var file in Directory.GetFiles(_templateFolder, "*.tmp") ) {
        try {
          var tmp = JsonFile.Read<DataTemplate>(file);
          tmp.FileName = file;

          _templates.Add(tmp);
        } catch { }
      }
      
      _defaults = JsonFile.Read<Dictionary<string,string>>(_defaultsFile);
    
      if( _defaults == null )
        _defaults = new Dictionary<string,string>();
    }

    public void Save() {

      foreach( var tmp in _templates ) 
        WriteToDisk(tmp);

      SaveDefaults();
    }

    private void SaveDefaults() {
      JsonFile.Write(_defaultsFile, _defaults);
    }

    void WriteToDisk(DataTemplate tmp) {
      if( !tmp.FileName.IsValid() )
        tmp.FileName = GetAvailableFileName();

      JsonFile.Write(tmp.FileName, tmp);
    }

    private string GetAvailableFileName() {
      string fileName;

      int i = 0;
      do {
        fileName = string.Format("{0}{1}.tmp", _templateFolder, ++i);
      } while( File.Exists(fileName) );

      return fileName;
    }

    public DataTemplate GetDefault(string typeName) {
      
      if( _defaults.ContainsKey(typeName)  )
        return _templates.FirstOrDefault(t => t.TypeName == typeName && t.Name == _defaults[typeName] );
      
      else return null;
    }
    private void SetDefault(string typeName, string name) {
      
      if( _defaults.ContainsKey(typeName) )
        _defaults[typeName] = name;
      else _defaults.Add(typeName, name);

      SaveDefaults();
    }
    public bool IsDefault(string typeName, string name) {
      
      if( _defaults.ContainsKey(typeName) )
        return _defaults[typeName] == name;

      return false;
    }


    public void Store(string name, Type type, object obj, bool @default = false) {

      DataTemplate temp = _templates.SingleOrDefault(t => t.TypeName == type.FullName && t.Name == name);
      if( temp == null ) {
        temp = new DataTemplate();
        temp.Name = name;
        temp.TypeName = type.FullName;

        _templates.Add(temp);
      }

      temp.Object = obj;

      if( @default )
        SetDefault(temp.TypeName, temp.Name);

      WriteToDisk(temp);
    }

    public object Get(string name, Type type) {

      DataTemplate temp = _templates.FirstOrDefault(t => t.TypeName == type.FullName && t.Name == name);

      if( temp != null )
        return temp.Object;
      else return null;
    }

    public void Delete(string name, Type type) {

      var item = _templates.FirstOrDefault(t => t.Name == name && t.TypeName == type.FullName);

      if( item != null ) {
        
        if( item.FileName.IsValid() )
          File.Delete(item.FileName);

        if( _defaults.Any( d => d.Key == item.TypeName && d.Value == item.Name ) ) {
          _defaults.Remove( item.TypeName );
          SaveDefaults();        
        }
        
        _templates.Remove(item);
      }
    }

  }
}
