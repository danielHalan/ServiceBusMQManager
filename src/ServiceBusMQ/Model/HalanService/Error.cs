#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    Error.cs
  Created: 2013-03-02

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
  public class Error {

    string _Message;
    Exception _Exception;

    public Error(Exception e) {
      _Message = e.Message;
      _Exception = e;
    }
    public Error(string msg, Exception e) {
      _Message = msg;
      _Exception = e;
    }
    public Error(string msg) {
      _Message = msg;
    }

    public string Message {
      get { return _Message; }
    }

    public Exception Exception {
      get { return _Exception; }
    }
  }

}
