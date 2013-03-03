#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    FailedToSendErrorReportException.cs
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
using System.Text;

namespace ServiceBusMQ.Model.HalanService {
  public class FailedToSendErrorReportException : Exception {
    public FailedToSendErrorReportException(string reason): base(reason) {
    }
  }
}
