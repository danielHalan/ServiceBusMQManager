#region File Information
/********************************************************************
  Project: ServiceBusMQ.MassTransit
  File:    MessageSerializer.cs
  Created: 2013-10-13

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Newtonsoft.Json;

namespace ServiceBusMQ.MassTransit {
  public static class MessageSerializer {


    private static string SerializeMessage_JSON(object cmd) {
      return JsonConvert.SerializeObject(cmd);
    }
    private static string SerializeMessage_XML(object cmd) {

      using( StringWriter stringWriter = new StringWriter() )
      using( XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter) ) {
        System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(cmd.GetType());
        x.Serialize(xmlTextWriter, cmd);
        return stringWriter.ToString();
      }
    }
    private static object DeserializeMessage_XML(string cmd, Type cmdType) {
      using( StringReader stringReader = new StringReader(cmd) )
      using( XmlTextReader xmlTextReader = new XmlTextReader(stringReader) ) {
        System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(cmdType);
        return x.Deserialize(xmlTextReader);
      }
    }
    private static object DeserializeMessage_JSON(string cmd, Type cmdType) {
      return JsonConvert.DeserializeObject(cmd, cmdType);
    }

    public static string SerializeMessage(object cmd, string commandContentFormat) {
      if( commandContentFormat == "XML" )
        return SerializeMessage_XML(cmd);

      else if( commandContentFormat == "JSON" )
        return SerializeMessage_JSON(cmd);

      else throw new Exception("Unknown Command Content Format, " + commandContentFormat);
    }
    public static object DeserializeMessage(string cmd, Type cmdType, string commandContentFormat) {
      if( commandContentFormat == "XML" )
        return DeserializeMessage_XML(cmd, cmdType);

      else if( commandContentFormat == "JSON" )
        return DeserializeMessage_JSON(cmd, cmdType);

      else throw new Exception("Unknown Command Content Format, " + commandContentFormat);
    }

  }
}
