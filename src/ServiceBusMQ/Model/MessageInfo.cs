#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    MessageInfo.cs
  Created: 2013-02-18

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

namespace ServiceBusMQ.Model {
  public class MessageInfo {

    public string Name { get; private set; }
    public string AssemblyQualifiedName { get; private set; }
  
    public MessageInfo(string name, string asmName = null) { 
      Name = name;
      AssemblyQualifiedName = asmName;
    }

  }
}
