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

<<<<<<< HEAD
namespace ServiceBusMQ.Adapter.NServiceBus4.Azure.SB22 {
=======
namespace ServiceBusMQ.NServiceBus4.Azure {
>>>>>>> 3dd34e76b2bd5c60a3431e8f5fa66de0154cca6c
  public static class MessageSerializer {


    private static string SerializeMessage_XML(object cmd) {
      var types = new List<Type> { cmd.GetType() };

      var mapper = new global::NServiceBus.MessageInterfaces.MessageMapper.Reflection.MessageMapper();
      mapper.Initialize(types);

      var serializr = new global::NServiceBus.Serializers.XML.XmlMessageSerializer(mapper);
      serializr.Initialize(types);

      using( Stream stream = new MemoryStream() ) {
        serializr.Serialize(new[] { cmd }, stream);
        stream.Position = 0;

        return new StreamReader(stream).ReadToEnd();
      }

    }
    private static object DeserializeMessage_XML(string cmd, Type cmdType) {
        var types = new List<Type> { cmdType };

        var mapper = new global::NServiceBus.MessageInterfaces.MessageMapper.Reflection.MessageMapper();
        mapper.Initialize(types);

        var serializr = new global::NServiceBus.Serializers.XML.XmlMessageSerializer(mapper);
        serializr.Initialize(types);

        using( Stream stream = new MemoryStream(Encoding.Unicode.GetBytes(cmd)) ) {
          var obj = serializr.Deserialize(stream);

          return obj[0];
        }

    }

    private static string SerializeMessage_JSON(object cmd) {

      var types = new List<Type> { cmd.GetType() };

      var mapper = new global::NServiceBus.MessageInterfaces.MessageMapper.Reflection.MessageMapper();
      mapper.Initialize(types);

      var serializr = new global::NServiceBus.Serializers.Json.JsonMessageSerializer(mapper);

      using( Stream stream = new MemoryStream() ) {
        serializr.Serialize(new[] { cmd }, stream);
        stream.Position = 0;

        return new StreamReader(stream).ReadToEnd();
      }

    }
    private static object DeserializeMessage_JSON(string cmd, Type cmdType) {
      var types = new List<Type> { cmd.GetType() };

      var mapper = new global::NServiceBus.MessageInterfaces.MessageMapper.Reflection.MessageMapper();
      mapper.Initialize(types);

      var serializr = new global::NServiceBus.Serializers.Json.JsonMessageSerializer(mapper);

      using( Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(cmd)) ) {
        var obj = serializr.Deserialize(stream);

        return obj[0];
      }

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
