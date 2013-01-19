#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    JsonFile.cs
  Created: 2012-12-04

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
using System.Runtime.Serialization.Formatters;
using System.Text;
using Newtonsoft.Json;

namespace ServiceBusMQ {
  public static class JsonFile {


    public static void Write(string fileName, object obj) {

      var s = new JsonSerializerSettings {
        TypeNameHandling = TypeNameHandling.All,
        TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
      };

      File.WriteAllText(fileName, JsonConvert.SerializeObject(obj, Formatting.Indented, s));
    }

    public static T Read<T>(string fileName, System.Runtime.Serialization.SerializationBinder binder = null) {

      if( File.Exists(fileName) ) {
        var s = new JsonSerializerSettings {
          TypeNameHandling = TypeNameHandling.All,
          TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
          ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
          ConstructorHandlingFallback = CtorFallback,
          Binder = binder
        };


        return (T)JsonConvert.DeserializeObject(File.ReadAllText(fileName), typeof(T), s);
      }

      return default(T);
    }

    private static void CtorFallback(object sender, ConstructorHandlingFallbackEventArgs e) {

      Dictionary<string, object> props = new Dictionary<string, object>();
      foreach( var p in e.ObjectContract.Properties ) {
        props.Add(p.PropertyName, Tools.GetDefault(p.PropertyType));
      }

      e.Object = Tools.CreateInstance(e.ObjectContract.UnderlyingType, props);

      e.Handled = true;

    }



  }
}
