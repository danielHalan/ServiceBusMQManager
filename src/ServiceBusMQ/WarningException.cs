#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    WarningException.cs
  Created: 2013-08-24

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
using System.Threading.Tasks;

namespace ServiceBusMQ {
  public class WarningException : Exception {
  

    public WarningException(string msg, string content): base(msg) {
    
      Content = content;
    
    }


    public string Content { get; set; }
  }
}
