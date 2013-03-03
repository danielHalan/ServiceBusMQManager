#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    ApplicationInfo.cs
  Created: 2013-03-03

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ServiceBusMQ {
  
  public class ApplicationInfo {

    Assembly _Assembly;

    public ApplicationInfo(string id, Assembly asm) {
      Id = id;
      _Assembly = asm;
    }


    T GetCustomAttribute<T>() where T : Attribute {
      object[] customAttributes = _Assembly.GetCustomAttributes(typeof(T), false);
      if( customAttributes.Length == 0 )
        return default(T);
      else return (T)customAttributes[0];
    }

    public string Company {
      get {
        AssemblyCompanyAttribute a = GetCustomAttribute<AssemblyCompanyAttribute>();
        return a != null ? a.Company : string.Empty;
      }
    }


    public string Product {
      get {
        AssemblyProductAttribute a = GetCustomAttribute<AssemblyProductAttribute>();
        return a != null ? a.Product : string.Empty;
      }
    }

    public string Title {
      get {
        AssemblyTitleAttribute a = GetCustomAttribute<AssemblyTitleAttribute>();
        return a != null ? a.Title : string.Empty;
      }
    }

    public string Description {
      get {
        AssemblyDescriptionAttribute a = GetCustomAttribute<AssemblyDescriptionAttribute>();
        return a != null ? a.Description : string.Empty;
      }
    }

    public string Copyright {
      get {
        AssemblyCopyrightAttribute a = GetCustomAttribute<AssemblyCopyrightAttribute>();
        return a != null ? a.Copyright : string.Empty;
      }
    }

    public Version Version {
      get {
        return _Assembly.GetName().Version;
      }
    }

    public HalanVersionInfo GetLatestVersionInfo() {
      return HalanServices.GetVersionInfo(Product, Version);
    }

    public string Id { get; set; }
  }


}
