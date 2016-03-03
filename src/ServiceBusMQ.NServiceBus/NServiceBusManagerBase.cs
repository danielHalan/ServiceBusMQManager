#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    NServiceBusMessageManager.cs
  Created: 2012-08-24

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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
//using NServiceBus;
//using NServiceBus.Tools.Management.Errors.ReturnToSourceQueue;
using ServiceBusMQ.Manager;
using ServiceBusMQ.Model;

namespace ServiceBusMQ.NServiceBus {

  public abstract class NServiceBusManagerBase<T> : IServiceBusManager where T : IMessageQueue {

    private static readonly string[] IGNORE_DLL = new string[] { "\\Autofac.dll", "\\AutoMapper.dll", "\\log4net.dll", 
                                                                  "\\MongoDB.Driver.dll", "\\MongoDB.Bson.dll", 
                                                                  "\\NServiceBus.dll" };

    static readonly string JSON_START = "\"$type\":\"";
    static readonly string JSON_END = ",";


    public abstract string ServiceBusName { get; }
    public abstract string ServiceBusVersion { get; }
    public abstract string MessageQueueType { get; }


    public string CommandContentFormat { get; set; }


    protected Dictionary<string, object> _connectionSettings;
    protected SbmqmMonitorState _monitorState;
    protected CommandDefinition _commandDef;

    protected List<T> _monitorQueues = new List<T>();


    public string[] AvailableMessageContentTypes {
      get { return new string[] { "XML", "JSON" }; }
    }


    public NServiceBusManagerBase() {
    }
    public virtual void Initialize(Dictionary<string, object> connectionSettings, Queue[] monitorQueues, SbmqmMonitorState monitorState) {
      _connectionSettings = connectionSettings;

      MonitorQueues = monitorQueues;
      _monitorState = monitorState;
    }


    public bool IsIgnoredQueue(string queueName) {
      return ( queueName.EndsWith(".subscriptions") ); // || queueName.EndsWith(".retries") || queueName.EndsWith(".timeouts") );
    }

    public abstract void MoveErrorMessageToOriginQueue(QueueItem itm);
    public abstract void MoveAllErrorMessagesToOriginQueue(string errorQueue);

    protected string ReadMessageStream(Stream s) {
      using( StreamReader r = new StreamReader(s, Encoding.Default) )
        return r.ReadToEnd().Replace("\0", "");
    }
    protected string GetSubscriptionType(string xml) {
      List<string> r = new List<string>();
      try {
        XDocument doc = XDocument.Parse(xml);

        var e = doc.Root as XElement;
        return e.Value;


      } catch { }

      return string.Empty;
    }
    public abstract MessageSubscription[] GetMessageSubscriptions(Dictionary<string, object> connectionSettings, IEnumerable<string> queues);


    public abstract string LoadMessageContent(QueueItem itm);

    protected IEnumerable<T> GetQueueListByType(QueueType type) {
      return _monitorQueues.Where(q => q.Queue.Type == type);
    }

    protected MessageInfo[] GetMessageNames(string content, bool includeNamespace) {

      if( content.StartsWith("<?xml version=\"1.0\"") )
        return GetXmlMessageNames(content, includeNamespace);
      else return GetJsonMessageNames(content, includeNamespace);

    }
    private MessageInfo[] GetJsonMessageNames(string content, bool includeNamespace) {
      List<MessageInfo> r = new List<MessageInfo>();
      try {
        foreach( var msg in GetAllRootCurlyBrackers(content) ) {

          int iStart = msg.IndexOf(JSON_START) + JSON_START.Length;
          int iEnd = msg.IndexOf(JSON_END, iStart);

          if( !includeNamespace ) {
            iStart = msg.LastIndexOf(".", iEnd) + 1;
          }

          r.Add( new MessageInfo(msg.Substring(iStart, iEnd - iStart)));
        }
      } catch { }

      return r.ToArray();
    }
    private MessageInfo[] GetXmlMessageNames(string content, bool includeNamespace) {
      List<MessageInfo> r = new List<MessageInfo>();
      try {
        XDocument doc = XDocument.Parse(content);
        string ns = string.Empty;

        if( includeNamespace ) {
          ns = doc.Root.Attribute("xmlns").Value.Remove(0, 19) + ".";
        }

        foreach( XElement e in doc.Root.Elements() ) {
          r.Add( new MessageInfo( ns + e.Name.LocalName ));
        }

      } catch { }

      return r.ToArray();
    }
    
    private IEnumerable<string> GetAllRootCurlyBrackers(string content) {
      int start = -1;
      int stack = 0;
      List<string> r = new List<string>();

      int i = 0;
      do {
        if( content[i] == '{' ) {
          if( stack == 0 )
            start = i;

          stack++;
        }

        if( content[i] == '}' ) {
          stack--;

          if( stack == 0 ) {
            r.Add(content.Substring(start, i - start));
          }
        }

      } while( ++i < content.Length );

      return r;
    }
    protected string MergeStringArray(MessageInfo[] arr) {
      StringBuilder sb = new StringBuilder();
      foreach( var msg in arr ) {
        if( sb.Length > 0 ) sb.Append(", ");

        sb.Append(msg.Name);
      }

      return sb.ToString();
    }



    protected MessageInfo[] ExtractEnclosedMessageTypeNames(string content, bool includeNamespace = false) {
      string[] types = content.Split(';');
      List<MessageInfo> r = new List<MessageInfo>(types.Length);

      foreach( string type in types ) {

        int start = 0;
        int end = type.IndexOf(',', start);
        if( end < 0 ) {
          r.Add(new MessageInfo(type));
        }
        else {
          if( !includeNamespace ) {
            start = type.LastIndexOf('.', end) + 1;
          }
          r.Add(new MessageInfo(type.Substring(start, end - start), type));
        }
      }

      return r.ToArray();
    }


    public abstract object DeserializeCommand(string cmd, Type cmdType);
    public abstract string SerializeCommand(object cmd);


    public bool MessagesHasMilliSecondPrecision { get { return false; } }


    public abstract QueueFetchResult GetUnprocessedMessages(QueueFetchUnprocessedMessagesRequest req);
    public abstract QueueFetchResult GetProcessedMessages(QueueType type, DateTime since, IEnumerable<QueueItem> currentItems);

    public abstract void PurgeMessage(QueueItem itm);
    public abstract void PurgeAllMessages();

    public abstract void PurgeErrorMessages(string queueName);
    public abstract void PurgeErrorAllMessages();


    public Queue[] MonitorQueues { get; private set; }


    public event EventHandler<ErrorArgs> ErrorOccured;
    public event EventHandler<WarningArgs> WarningOccured;

    protected void OnError(string message, Exception exception = null, bool fatal = false) {
      if( ErrorOccured != null )
        ErrorOccured(this, new ErrorArgs(message, exception, fatal));
    }
    protected void OnWarning(string message, string content, WarningType type = WarningType.Other) {
      if( WarningOccured != null )
        WarningOccured(this, new WarningArgs(message, content, type));
    }


    protected EventHandler _itemsChanged;
    public event EventHandler ItemsChanged {
      [MethodImpl(MethodImplOptions.Synchronized)]
      add {
        _itemsChanged = (EventHandler)Delegate.Combine(_itemsChanged, value);
      }
      [MethodImpl(MethodImplOptions.Synchronized)]
      remove {
        _itemsChanged = (EventHandler)Delegate.Remove(_itemsChanged, value);
      }
    }

    protected void OnItemsChanged() {
      if( _itemsChanged != null )
        _itemsChanged(this, EventArgs.Empty);
    }

    public abstract void Terminate();


    // ISendCommand
    public Type[] GetAvailableCommands(string[] asmPaths, CommandDefinition commandDef, bool suppressErrors) {
      List<Type> arr = new List<Type>();


      List<string> nonExistingPaths = new List<string>();


      foreach( var path in asmPaths ) {

        if( Directory.Exists(path) ) {

          foreach( var dll in Directory.GetFiles(path, "*.dll") ) {

            if( IGNORE_DLL.Any(a => dll.EndsWith(a)) )
              continue;

            try {
              var asm = Assembly.LoadFrom(dll);
              //var asm = Assembly.ReflectionOnlyLoadFrom(dll);

              foreach( Type t in asm.GetTypes() ) {

                if( commandDef.IsCommand(t) )
                  arr.Add(t);

              }

            } catch( ReflectionTypeLoadException fte ) {

              if( suppressErrors )
                continue;

              StringBuilder sb = new StringBuilder();
              if( fte.LoaderExceptions != null ) {

                if( fte.LoaderExceptions.All(a => a.Message.EndsWith("does not have an implementation.")) )
                  continue;

                string lastMsg = null;
                foreach( var ex in fte.LoaderExceptions ) {
                  if( ex.Message != lastMsg )
                    sb.AppendFormat(" - {0}\n\n", lastMsg = ex.Message);
                }

                sb.Append("Try adding these libraries to your Assembly Folder");
              }

              OnWarning("Could not search for Commands in Assembly '{0}'".With(Path.GetFileName(dll)), sb.ToString());

            } catch { }

          }
        } else nonExistingPaths.Add(path);
      }

      if( nonExistingPaths.Count > 0 )
        OnError("The paths '{0}' doesn't exist, could not search for commands.".With(nonExistingPaths.Concat()));


      return arr.ToArray();
    }

  }

}
