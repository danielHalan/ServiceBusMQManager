#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    ObjectExtensions.cs
  Created: 2013-10-27

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ServiceBusMQ {
  public static class ObjectExtensions {
  
    public static T As<T>(this object obj) {
      return (T)obj;
    }

    public static string GetFormatted(this JObject document) {
      if( document == null ) {
        return string.Empty;
      }

      var sb = new StringBuilder();
      using( var writer = new JsonTextWriter(new StringWriter(sb)) ) {
        writer.Formatting = Formatting.Indented;
        document.WriteTo(writer);
      }

      return sb.ToString();
    }


  }
}
