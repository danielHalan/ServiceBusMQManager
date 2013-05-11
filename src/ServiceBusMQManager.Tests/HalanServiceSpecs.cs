#region File Information
/********************************************************************
  Project: ServiceBusMQManager.Tests
  File:    HalanServiceSpecs.cs
  Created: 2013-05-11

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
using System.Threading.Tasks;
using Machine.Specifications;
using ServiceBusMQ;

namespace ServiceBusMQManager.Tests {

  [Subject("halan service")]
  public abstract class with_halan_service {

    protected static ApplicationInfo AppInfo;

    Establish context = () => {
      AppInfo = new ApplicationInfo(null, Assembly.GetExecutingAssembly());
    };

  }

  [Subject("halan service")]
  public class when_getting_latest_version : with_halan_service {
    
    private static HalanVersionInfo response;

    Because of = () => response = HalanServices.GetVersionInfo("ServiceBusMQManager", AppInfo.Version);

    It shoud_return_response = () => response.ShouldNotBeNull();
    It shoud_return_correct_version = () => response.LatestVersion.ShouldBeGreaterThanOrEqualTo(CURRENT_VERSION); 
  }


}
