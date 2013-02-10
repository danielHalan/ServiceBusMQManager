#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    CommandDefinition.cs
  Created: 2012-12-05

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

namespace ServiceBusMQ {
  
  [Serializable]
  public class CommandDefinition {

    public string NamespaceContains;
    public string InheritsType;

    public bool IsCommand(Type commandType) {
      Type t = GetBaseType();

      if( t != null && t.IsAssignableFrom(commandType) ) {
        return true;

      } else if( NamespaceContains.IsValid() && commandType.Namespace.IsValid() && 
                        commandType.Namespace.Contains(NamespaceContains) ) {
        return true;
      }

      return false;
    }

    private Type GetBaseType() {
      if( !string.IsNullOrEmpty(InheritsType) ) {
        return Type.GetType(InheritsType);
      } else return null;
    }

  }
}
