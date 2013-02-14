#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    MessageContentFormat.cs
  Created: 2013-01-26

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

namespace ServiceBusMQ.Manager {
  public enum MessageContentFormat  { Xml, Json, Other=0xFF }
}
