#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    HalanVersionInfo.cs
  Created: 2012-12-09

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceBusMQ {

  public enum VersionStatus { Unknown, NoConnection, Latest, Old }

  public class HalanVersionInfo {
    public string Product;
    public DateTime ReleaseDate;
    public VersionStatus Status;
    public Version LatestVersion;
    public string[] Features;
    public string Url;
  }

}
