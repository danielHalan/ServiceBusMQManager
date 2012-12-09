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
        r.Url = resp.Url.Default("http://blog.halan.se/page/Service-Bus-MQ-Manager.aspx");
        return r;
      }

      return null;
    }


  }
}
