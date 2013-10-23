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
      public string Version { get; private set; }
      public string QueueType { get; private set; }
      public string[] MessageContentTypes { get; set; }

      public ServiceBusManagerType(string name, string version, string queueType, string[] msgContentTypes) {
        Name = name;
        Version = version;
        QueueType = queueType;
        MessageContentTypes = msgContentTypes;
      }

      public string DisplayName { 
        get { return ServerConfig3.GetFullMessageBusName(Name, Version); }
      }

    }


    public static ServiceBusManagerType[] AvailableServiceBusManagers() {
      List<ServiceBusManagerType> r = new List<ServiceBusManagerType>();

      foreach( var asm in AsmCache.Assemblies ) {
        foreach( var type in asm.Types.Where(t => t.Interfaces.Any(i => i.EndsWith("IServiceBusManager"))) ) {

          ServiceBusManagerType t = r.SingleOrDefault(sb => sb.Name == type.ServiceBusName && sb.Version == type.ServiceBusVersion && sb.QueueType == type.MessageQueueType);

          if( t == null ) {
            r.Add(new ServiceBusManagerType(type.ServiceBusName,
                        type.ServiceBusVersion,
                        type.MessageQueueType,
                        type.AvailableMessageContentTypes));
          }
        }
      }

      return r.ToArray();
    }


    //static Dictionary<string, AppDomain> _domains = new Dictionary<string, AppDomain>();

    internal static IServiceBusManager CreateManager(string name, string version, string queueType) {
      // DH 2013-10-15: Using Separate AppDomain per Manager (version) is To Slow due all Marheling
      /* 
      var domainName = name + queueType;
      var domain = _domains.GetValue(domainName, null);
      if( domain == null ) {
        domain = AppDomain.CreateDomain(domainName);
        domain.AssemblyResolve += domain_AssemblyResolve;
        _domains.Add(domainName, domain);
      }
      */

      try {
        foreach( var asm in AsmCache.Assemblies ) {
          var type = asm.Types.SingleOrDefault(t =>
                                  t.ServiceBusName == name &&
                                  t.ServiceBusVersion == version &&
                                  t.MessageQueueType == queueType &&
                                  t.Interfaces.Any(i => i.EndsWith("IServiceBusManager")));

          if( type != null )
            return (IServiceBusManager)Activator.CreateInstance(asm.AssemblyName, type.TypeName).Unwrap();
          //return (IServiceBusManager)domain.CreateInstanceAndUnwrap(asm.AssemblyName, type.TypeName);
        }

      } catch( TypeLoadException ) {
        AsmCache.Rescan();

        CreateManager(name, version, queueType);
      }


      throw new NoMessageBusManagerFound(name, queueType);
    }

    internal static string GetManagerFilePath(string name, string version, string queueType) {
      try {
        foreach( var asm in AsmCache.Assemblies ) {
          var type = asm.Types.SingleOrDefault(t =>
                                  t.ServiceBusName == name &&
                                  t.ServiceBusVersion == version &&
                                  t.MessageQueueType == queueType &&
                                  t.Interfaces.Any(i => i.EndsWith("IServiceBusManager")));

          if( type != null )
            return asm.AssemblyFile;
        }

      } catch( TypeLoadException ) {
        AsmCache.Rescan();

        return GetManagerFilePath(name, version, queueType);
      }


      throw new NoMessageBusManagerFound(name, queueType);    
    }

    /*
    static Assembly domain_AssemblyResolve(object sender, ResolveEventArgs args) {
      string asmName = args.Name.Split(',')[0];
      var root = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
      string adapterPath = root + "\\Adapters\\";

      foreach( var dir in Directory.GetDirectories(adapterPath) ) {
        var fn = Path.Combine(dir, asmName + ".dll");
        if( File.Exists(fn) && AssemblyName.GetAssemblyName(fn).FullName == args.Name )
          return Assembly.LoadFrom(fn);
      }

      return null;
    }
    */ 

    internal static IServiceBusDiscovery CreateDiscovery(string name, string version, string transportation) {

      try {
        foreach( var asm in AsmCache.Assemblies ) {
          var type = asm.Types.SingleOrDefault(t =>
                                  t.ServiceBusName == name &&
                                  t.ServiceBusVersion == version &&
                                  t.MessageQueueType == transportation &&
                                  t.Interfaces.Any(i => i.EndsWith("IServiceBusDiscovery")));

          if( type != null )
            return (IServiceBusDiscovery)Activator.CreateInstance(asm.AssemblyName, type.TypeName).Unwrap();

        }

      } catch( TypeLoadException ) {
        AsmCache.Rescan();

        CreateDiscovery(name, version, transportation);
      }


      throw new NoMessageBusManagerFound(name, transportation);
    }


    public static bool CanSendCommand(string name, string version, string queueType) {

      foreach( var asm in AsmCache.Assemblies ) {
        var type = asm.Types.SingleOrDefault(t =>
                                t.ServiceBusName == name &&
                                t.ServiceBusVersion == version &&
                                t.MessageQueueType == queueType &&
                                t.Interfaces.Any(i => i.EndsWith("ISendCommand")));

        if( type != null )
          return true;
      }

      return false;
    }


    static AssemblyCache _asmCache = null;
    static AssemblyCache AsmCache {
      get {
        if( _asmCache == null )
          _asmCache = AssemblyCache.Create(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

        return _asmCache;
      }
    }



  }
}
