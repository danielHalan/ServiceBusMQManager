#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    Bit.cs
  Created: 2013-03-16

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

namespace ServiceBusMQ {
  public static class Bit {
    public static int HiWord(int iValue) {
      return ( ( iValue >> 16 ) & 0xFFFF );
    }
    public static int LoWord(int iValue) {
      return ( iValue & 0xFFFF );
    }

  }
}
