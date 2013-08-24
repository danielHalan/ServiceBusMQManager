#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    MessageBusFactory.cs
  Created: 2012-09-23

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ServiceBusMQ.Configuration;

namespace ServiceBusMQ.Manager {

  public class ServiceBusFactory {

    public class ServiceBusManagerType {
      public string Name { get; private set; }
      public List<string> QueueTypes { get; private set; }
      public string[] MessageContentTypes { get; set; }

      public ServiceBusManagerType(string name, string[] queueTypes, string[] msgContentTypes) {
        Name = name;
        QueueTypes = new List<string>(queueTypes);
        MessageContentTypes = msgContentTypes;
      }

    }


    public static ServiceBusManagerType[] AvailableServiceBusManagers() {
      List<ServiceBusManagerType> r = new List<ServiceBusManagerType>();


      foreach( var asm in AsmCache.Assemblies ) {
        foreach( var type in asm.Types.Where(t => t.Interfaces.Any(i => i.EndsWith("IServiceBusManager"))) ) {

          ServiceBusManagerType t = r.SingleOrDefault(sb => sb.Name == type.ServiceBusName);

          if( t == null )
            r.Add(new ServiceBusManagerType(type.ServiceBusName, type.AvailableMessageQueueTypes, type.AvailableMessageContentTypes));

        }
      }

      return r.ToArray();
    }


    internal static IServiceBusManager CreateManager(string name, string queueType) {

      foreach( var asm in AsmCache.Assemblies ) {
        var type = asm.Types.SingleOrDefault( t => 
                                t.ServiceBusName == name && 
                                t.AvailableMessageQueueTypes.Contains(queueType) && 
                                t.Interfaces.Any( i => i.EndsWith("IServiceBusManager") ) );

        if( type != null ) 
          return (IServiceBusManager)Activator.CreateInstance(asm.AssemblyName, type.TypeName).Unwrap();
      
      }

      throw new NoMessageBusManagerFound(name, queueType);
    }
    internal static IServiceBusDiscovery CreateDiscovery(string name, string transportation) {
      foreach( var asm in AsmCache.Assemblies ) {
        var type = asm.Types.SingleOrDefault(t =>
                                t.ServiceBusName == name && 
                                t.AvailableMessageQueueTypes.Contains(transportation) && 
                                t.Interfaces.Any(i => i.EndsWith("IServiceBusDiscovery")));

        if( type != null )
          return (IServiceBusDiscovery)Activator.CreateInstance(asm.AssemblyName, type.TypeName).Unwrap();

      }

      throw new NoMessageBusManagerFound(name, transportation);
    }


    static AssemblyCache _asmCache = null;
    static AssemblyCache AsmCache {
      get {
        if( _asmCache == null )
          _asmCache = AssemblyCache.Create(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

        return _asmCache;
      }
    }



  }
}
