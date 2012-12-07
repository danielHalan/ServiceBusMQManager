#region File Information
/********************************************************************
  Project: Newtonsoft.Json
  File:    TraceLevel.cs
  Created: 2012-11-20

  Author(s):
    James Newton-King

 (C) Copyright 2007 James Newton-King

********************************************************************/
#endregion

#if (NETFX_CORE || SILVERLIGHT || PORTABLE)
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json
{
  /// <summary>
  /// Specifies what messages to output for the <see cref="ITraceWriter"/> class.
  /// </summary>
  public enum TraceLevel
  {
    /// <summary>
    /// Output no tracing and debugging messages.
    /// </summary>
    Off,
    /// <summary>
    /// Output error-handling messages.
    /// </summary>
    Error,
    /// <summary>
    /// Output warnings and error-handling messages.
    /// </summary>
    Warning,
    /// <summary>
    /// Output informational messages, warnings, and error-handling messages.
    /// </summary>
    Info,
    /// <summary>
    /// Output all debugging and tracing messages.
    /// </summary>
    Verbose
  }
}
#endif