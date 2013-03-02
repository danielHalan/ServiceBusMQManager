#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    AssemblyCache.cs
  Created: 2013-02-14

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
using System.Reflection;
using ServiceBusMQ.Manager;

namespace ServiceBusMQ.Configuration {
  
  public class AssemblyCache {

    static Type INTERFACE = typeof(IServiceBus);

    public class SbmqmServiceBusType {
      public string ServiceBusName { get; set; }
      public string TransportationName { get; set; }
      
      public string[] Interfaces { get; set; }

      public string TypeName { get; set; }
    }

    public class SbmqmAssembly {

      public string AssemblyName { get; set; }
      public string AssemblyFile { get; set; }
      public List<SbmqmServiceBusType> Types { get; set; }

    }

    public class AssemblyCacheFile {
      public string Path { get; set; }
      public int PathHash { get; set; }
      
      public List<SbmqmAssembly> Assemblies { get; set; }
    }

    string _cacheFile;

    public string Path { get; private set; }

    public List<SbmqmAssembly> Assemblies { get; set; }

    private AssemblyCache(string path) {
      Path = path;

      _cacheFile = SbmqSystem.AppDataPath + "asmCache.dat";

      if( File.Exists(_cacheFile) ) {
        AssemblyCacheFile f = LoadFile();

        if( f != null && f.Path == path && f.PathHash == GetPathHash(path) ) {
          
          Assemblies = f.Assemblies;
        
        } else Rescan();
      
      } else Rescan();
    }

    public static AssemblyCache Create(string path) {
      return new AssemblyCache(path);
    }

    private AssemblyCacheFile LoadFile() {
      try {
        return JsonFile.Read<AssemblyCacheFile>(_cacheFile);
      } catch { 
        return null;
      }
    }

    private void Rescan() {
      Assemblies = new List<SbmqmAssembly>();

      FindAssemblies();

      SaveFile();
    }

    private void SaveFile() {
      try {
        AssemblyCacheFile f = new AssemblyCacheFile();
        f.Path = Path;
        f.PathHash = GetPathHash(Path);
        f.Assemblies = Assemblies;

        JsonFile.Write(_cacheFile, f);
      } catch {
      }
    }

    private int GetPathHash(string path) {
      var files = Directory.GetFiles(path);

      int hash = files.Length;

      foreach( var file in files )
        hash += file.Length;

      return hash;
    }


    private void FindAssemblies() {
    
      foreach( Assembly asm in GetAllAssemblies() ) {
        var a = new SbmqmAssembly() { 
          AssemblyFile = asm.Location,
          AssemblyName = asm.FullName,
          Types = new List<SbmqmServiceBusType>() 
        };

        foreach( var type in asm.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface) ) {

          if( INTERFACE.IsAssignableFrom(type) ) {
            SbmqmServiceBusType t = new SbmqmServiceBusType();

            IServiceBus mgr = (IServiceBus)Activator.CreateInstance(type);
            t.ServiceBusName = mgr.ServiceBusName; 
            t.TransportationName = mgr.TransportationName; 
            t.Interfaces = type.GetInterfaces().Select( i => i.Name ).ToArray();
            t.TypeName = type.FullName;

            a.Types.Add(t);
          }
        }

        if( a.Types.Count > 0 )
          Assemblies.Add(a);
      }
    
    }


    Assembly[] GetAllAssemblies() {

      List<Assembly> result = new List<Assembly>();
      string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

      foreach( string asm in Directory.GetFiles(path, "ServiceBusMQ.*.dll") )
        result.Add(Assembly.LoadFile(asm));

      return result.ToArray();
    }


  
  }
}
