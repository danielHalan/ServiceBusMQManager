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

    public static T Read<T>(string fileName) {

      if( File.Exists(fileName) ) {
        var s = new JsonSerializerSettings {
          TypeNameHandling = TypeNameHandling.All,
          TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple

        };

        return (T)JsonConvert.DeserializeObject(File.ReadAllText(fileName), typeof(T), s);
      }

      return default(T);
    }


  }
}
