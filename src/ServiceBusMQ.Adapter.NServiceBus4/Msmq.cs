#region File Information
/********************************************************************
  Project: ServiceBusMQ.NServiceBus
  File:    Msmq.cs
  Created: 2013-02-14

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System.Messaging;

namespace ServiceBusMQ.NServiceBus4 {
  public static class Msmq {
    
    public static MessageQueue Create(string serverName, string queueName, QueueAccessMode accessMode) {
      if( !queueName.StartsWith("private$\\") )
        queueName = "private$\\" + queueName;

      queueName = string.Format("FormatName:DIRECT=OS:{0}\\{1}", !Tools.IsLocalHost(serverName) ? serverName : ".", queueName);

      return new MessageQueue(queueName, false, true, accessMode);
    }
    
    public static MessageQueue Create(string queueFormatName, QueueAccessMode accessMode) {
      return new MessageQueue(queueFormatName, false, true, accessMode);
    }
  
  }
}
