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

    /// <summary>
    /// A string that should be in the commands namespace
    /// </summary>
    public string NamespaceContains;

    /// <summary>
    /// A class or interface that the command should inherit/implement.
    /// </summary>
    public string InheritsType;

    /// <summary>
    /// Function that checks if the type is a command
    /// </summary>
    /// <param name="commandType">Type to check</param>
    /// <returns></returns>
    public bool IsCommand(Type commandType) {
      Type t = GetInheritsType();

      if( t != null && t.IsAssignableFrom(commandType) ) {
        return true;

      } else if( NamespaceContains.IsValid() && commandType.Namespace.IsValid() && 
                        commandType.Namespace.Contains(NamespaceContains) ) {
        return true;
      }

      return false;
    }

    private Type GetInheritsType() {
      if( !string.IsNullOrEmpty(InheritsType) ) {
        return Type.GetType(InheritsType);
      } else return null;
    }

  }
}
