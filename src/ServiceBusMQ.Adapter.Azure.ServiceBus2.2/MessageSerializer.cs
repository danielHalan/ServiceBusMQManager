#region File Information
/********************************************************************
  Project: ServiceBusMQ.NServiceBus4.Azure
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
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace ServiceBusMQ.Adapter.Azure.ServiceBus22 {

  public static class MessageSerializer {


    private static string SerializeMessage_XML(object cmd) {
      var types = new List<Type> { cmd.GetType() };

      var serializr = new XmlSerializer(cmd.GetType());

      using( Stream stream = new MemoryStream() ) {
        serializr.Serialize(stream, cmd);
        stream.Position = 0;

        return new StreamReader(stream).ReadToEnd();
      }

    }
    private static object DeserializeMessage_XML(string cmd, Type cmdType) {
        var serializr = new XmlSerializer(cmd.GetType());

        using( Stream stream = new MemoryStream(Encoding.Unicode.GetBytes(cmd)) ) {
          return serializr.Deserialize(stream);
        }

    }

    private static string SerializeMessage_JSON(object cmd) {
      return JsonConvert.SerializeObject(cmd);
    }
    private static object DeserializeMessage_JSON(string cmd, Type cmdType) {
      return JsonConvert.DeserializeObject(cmd, cmdType);
    }

    public static object DeserializeMessage(string cmd, Type cmdType, string commandContentFormat) {

      if( commandContentFormat == "XML" )
        return DeserializeMessage_XML(cmd, cmdType);

      else if( commandContentFormat == "JSON" )
        return DeserializeMessage_JSON(cmd, cmdType);

      else throw new Exception("Unknown Command Content Format, " + commandContentFormat);
    }
    public static string SerializeMessage(object cmd, string commandContentFormat) {

      if( commandContentFormat == "XML" )
        return SerializeMessage_XML(cmd);

      else if( commandContentFormat == "JSON" )
        return SerializeMessage_JSON(cmd);

      else throw new Exception("Unknown Command Content Format, " + commandContentFormat);

    }


  }
}
