#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    AppDomainBinder.cs
  Created: 2013-01-08

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json.Utilities;

namespace ServiceBusMQ {

  public class AppDomainBinder : SerializationBinder {

    //private readonly ThreadSafeStore<TypeNameKey, Type> _typeCache = new ThreadSafeStore<TypeNameKey, Type>(GetTypeFromTypeNameKey);
    private AppDomain _appDomain;

    public AppDomainBinder(AppDomain appDomain) {
      _appDomain = appDomain;   
    }

    private Type GetTypeFromTypeNameKey(TypeNameKey typeNameKey) {
      string assemblyName = typeNameKey.AssemblyName;
      string typeName = typeNameKey.TypeName;

      if( assemblyName != null ) {
        Assembly assembly;

//#pragma warning disable 618,612
        Console.WriteLine(_appDomain.FriendlyName);
        
        assembly = _appDomain.Load( new AssemblyName() { Name = assemblyName } );
//#pragma warning restore 618,612

        if( assembly == null )
          throw new SerializationException("Could not load assembly '{0}'.".With(CultureInfo.InvariantCulture, assemblyName));

        Type type = assembly.GetType(typeName);
        if( type == null )
          throw new SerializationException("Could not find type '{0}' in assembly '{1}'.".With(CultureInfo.InvariantCulture, typeName, assembly.FullName));

        return type;
      } else {
        return Type.GetType(typeName);
      }
    }

    internal struct TypeNameKey : IEquatable<TypeNameKey> {
      internal readonly string AssemblyName;
      internal readonly string TypeName;

      public TypeNameKey(string assemblyName, string typeName) {
        AssemblyName = assemblyName;
        TypeName = typeName;
      }

      public override int GetHashCode() {
        return ( ( AssemblyName != null ) ? AssemblyName.GetHashCode() : 0 ) ^ ( ( TypeName != null ) ? TypeName.GetHashCode() : 0 );
      }

      public override bool Equals(object obj) {
        if( !( obj is TypeNameKey ) )
          return false;

        return Equals((TypeNameKey)obj);
      }

      public bool Equals(TypeNameKey other) {
        return ( AssemblyName == other.AssemblyName && TypeName == other.TypeName );
      }
    }

    /// <summary>
    /// When overridden in a derived class, controls the binding of a serialized object to a type.
    /// </summary>
    /// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly"/> name of the serialized object.</param>
    /// <param name="typeName">Specifies the <see cref="T:System.Type"/> name of the serialized object.</param>
    /// <returns>
    /// The type of the object the formatter creates a new instance of.
    /// </returns>
    public override Type BindToType(string assemblyName, string typeName) {
      return GetTypeFromTypeNameKey(new TypeNameKey(assemblyName, typeName));
    }

    /// <summary>
    /// When overridden in a derived class, controls the binding of a serialized object to a type.
    /// </summary>
    /// <param name="serializedType">The type of the object the formatter creates a new instance of.</param>
    /// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly"/> name of the serialized object. </param>
    /// <param name="typeName">Specifies the <see cref="T:System.Type"/> name of the serialized object. </param>
    public override void BindToName(Type serializedType, out string assemblyName, out string typeName) {
      assemblyName = serializedType.Assembly.FullName;
      typeName = serializedType.FullName;
    }


  }
}
