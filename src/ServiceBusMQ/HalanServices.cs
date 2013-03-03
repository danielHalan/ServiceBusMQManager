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
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using ServiceBusMQ.HalanService;
using ServiceBusMQ.Manager;


namespace ServiceBusMQ {

  public class HalanServices {

    public static HalanVersionInfo GetVersionInfo(string productName, Version currentVersion) {
      LatestVersionRequest req = new LatestVersionRequest();
      req.ProductName = productName;
      req.CurrentProductVersion = currentVersion.ToString(4);

      LatestVersionResponse resp = null;
      var client = HalanServices.CreateProductManager();
      try {
        resp = client.GetLatestVersion(req);
      } finally {
        client.Close();
      }

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

      /*
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
      */
    }


    public static ProductManagerClient CreateProductManager() {

      BasicHttpBinding wsd = new BasicHttpBinding();
      wsd.Name = "WSHttpBinding_IProductManager";

      wsd.SendTimeout = new TimeSpan(0,5,0);
      wsd.MessageEncoding = WSMessageEncoding.Mtom;
      wsd.AllowCookies = false;
      wsd.BypassProxyOnLocal = false;
      wsd.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
      wsd.TextEncoding = Encoding.UTF8;
      wsd.TransferMode = TransferMode.Buffered;
      wsd.UseDefaultWebProxy = true;
      wsd.MaxReceivedMessageSize = 1048576 * 30; // 30 mb
      wsd.MaxBufferSize = 1048576 * 30;
      wsd.Security.Mode = BasicHttpSecurityMode.None;
      wsd.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
      wsd.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.Default;
      wsd.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
      wsd.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
      wsd.Security.Transport.Realm = string.Empty;

      Uri baseAddress = new Uri("http://www.halan.se/service/ProductManager.svc/mtom");
      EndpointAddress ea = new EndpointAddress(baseAddress); // EndpointIdentity.CreateDnsIdentity("localhost"));


      return new ProductManagerClient(wsd, ea); //new ProductManagerClient("BasicHttpBinding_IProductManager");
    }


  }
}
