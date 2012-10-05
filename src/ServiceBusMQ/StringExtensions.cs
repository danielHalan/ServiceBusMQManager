#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    StringExtensions.cs
  Created: 2012-09-10

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
using System.Threading.Tasks;

namespace ServiceBusMQ {
  public static class StringExtensions {

    public static string CutBeginning(this string str, int length) {
      length -= 3;
      if( str.Length > length ) {
        return "..." + str.Substring(str.Length - length, length);
      } else return str;
    }
    public static string CutEnd(this string str, int length) {
      length -= 3;
      if( str.Length > length ) {
        return str.Substring(0, length) + "...";
      } else return str;
    }
  
  }
}
