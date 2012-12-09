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
