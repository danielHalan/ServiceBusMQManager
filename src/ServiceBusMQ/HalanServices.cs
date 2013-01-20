#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    HalanServices.cs
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
using ServiceBusMQ.HalanService;


namespace ServiceBusMQ {

  public class HalanServices {

    public static HalanVersionInfo GetVersionInfo(string productName, Version currentVersion) {

      BasicHttpBinding_IProductManager p = new BasicHttpBinding_IProductManager();


      LatestVersionRequest req = new LatestVersionRequest();
      req.ProductName = productName;
      req.CurrentProductVersion = currentVersion.ToString(4);



      LatestVersionResponse resp = p.GetLatestVersion(req);

      if( resp.ProductVersion.IsValid() ) {
        HalanVersionInfo r = new HalanVersionInfo();

        Version larestVer = new Version(resp.ProductVersion);
        r.Product = productName;
        r.ReleaseDate = resp.ReleaseDate;
        r.Status = ( larestVer <= currentVersion ) ? VersionStatus.Latest : VersionStatus.Old;
        r.LatestVersion = larestVer;
        r.Features = resp.Features;
        r.Url = resp.Url.Default("http://blog.halan.se/page/Service-Bus-MQ-Manager.aspx?update=true&v=" + currentVersion.ToString());
        return r;
      }

      return null;
    }


  }
}
