#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    CheckVersionThread.cs
  Created: 2012-12-09

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ServiceBusMQ {

  public class CheckVersionObject {
    public string ProductName;
    public Version CurrentVersion;
  }

  /// <summary>
  /// Retrieves Version information from Halan Server
  /// </summary>
  public class CheckVersionThread : BackgroundWorker {

    public CheckVersionThread()
      : base() {
    }

    protected override void OnDoWork(DoWorkEventArgs e) {
      List<HalanVersionInfo> r = new List<HalanVersionInfo>();

      List<CheckVersionObject> check = e.Argument as List<CheckVersionObject>;
      foreach( CheckVersionObject c in check ) {
        HalanVersionInfo inf = HalanServices.GetVersionInfo(c.ProductName, c.CurrentVersion);
        if( inf != null )
          r.Add(inf);
      }

      e.Result = r;
    }

  }
}
