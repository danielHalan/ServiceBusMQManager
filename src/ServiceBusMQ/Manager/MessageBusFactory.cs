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
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusMQ.Manager {
 
  public class MessageBusFactory {
 
    public class ServiceBusManagerType { 
      public string Name { get; private set; }
      public List<string> QueueTypes { get; private set; }

      public ServiceBusManagerType(string name, string queueType) {
        Name = name;
        QueueTypes = new List<string>();
        QueueTypes.Add(queueType);
      }

    }
 

    static Assembly[] GetAssemblies() {

      List<Assembly> result = new List<Assembly>();
      string path = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );

      foreach( string asm in Directory.GetFiles(path, "ServiceBusMQ.*.dll") )
        result.Add(Assembly.LoadFile(asm));
    
      return result.ToArray();
    }

    public static ServiceBusManagerType[] AvailableServiceBusManagers() {
      Type mgrInterface = typeof(IMessageManager);

      List<ServiceBusManagerType> r = new List<ServiceBusManagerType>();

      foreach( Assembly asm in GetAssemblies() )
        foreach( var tMgr in asm.GetTypes().Where(t => mgrInterface.IsAssignableFrom(t) && !t.IsAbstract) ) {
          IMessageManager mgr = (IMessageManager)Activator.CreateInstance(tMgr);

          ServiceBusManagerType t = r.SingleOrDefault( sb => sb.Name == mgr.BusName );

          if( t == null )
            r.Add( new ServiceBusManagerType(mgr.BusName, mgr.BusQueueType) );
          else t.QueueTypes.Add( mgr.BusQueueType);
        }

      return r.ToArray();
    }



    public static IMessageManager Create(string name, string queueType) {
      Type mgrInterface = typeof(IMessageManager);
      
      foreach( Assembly asm in GetAssemblies() ) 
        foreach( var tMgr in asm.GetTypes().Where( t => mgrInterface.IsAssignableFrom(t) && !t.IsAbstract ) ) {
          IMessageManager mgr = (IMessageManager)Activator.CreateInstance(tMgr);
      
          if( string.Compare(mgr.BusName, name) == 0 && string.Compare(mgr.BusQueueType, queueType) == 0 ) 
            return mgr;
        }

      throw new NoMessageBusManagerFound(name, queueType);
    }
  
  }
}
