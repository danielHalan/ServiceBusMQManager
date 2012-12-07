#region File Information
/********************************************************************
  Project: Newtonsoft.Json
  File:    FormatterAssemblyStyle.cs
  Created: 2012-11-20

  Author(s):
    James Newton-King

 (C) Copyright 2007 James Newton-King

********************************************************************/
#endregion

#if SILVERLIGHT || PocketPC || NETFX_CORE || PORTABLE
namespace System.Runtime.Serialization.Formatters
{
  /// <summary>
  /// Indicates the method that will be used during deserialization for locating and loading assemblies.
  /// </summary>
  public enum FormatterAssemblyStyle
  {
    /// <summary>
    /// In simple mode, the assembly used during deserialization need not match exactly the assembly used during serialization. Specifically, the version numbers need not match as the LoadWithPartialName method is used to load the assembly.
    /// </summary>
    Simple,
    /// <summary>
    /// In full mode, the assembly used during deserialization must match exactly the assembly used during serialization. The Load method of the Assembly class is used to load the assembly.
    /// </summary>
    Full
  }
}
#endif