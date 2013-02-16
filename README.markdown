Service Bus MQ Manager - v2.xx
======================================================================
An application to view and manage Service Bus messages.

## Overview
Service Bus MQ Manager works by monitoring a Queue (currently MSMQ) and parsing the Messages presenting them in a list sorted by Commands, Events, Messages and Errors. Currently NServiceBus is supported, but its possible to [create a new Service Bus Adapter](https://github.com/danielHalan/ServiceBusMQManager/wiki/Building-a-Service-Bus-Adapter) for other Service Bus enviroments.

The application is most useful when used in a [CQRS architecture](http://cqrsinfo.com), where its common to send Commands and Event messages using a Service Bus.

- **Monitor Queues**, Monitor queues for incoming Commands, Events and Messages envelops, also possible to monitor Error Queue
- **Remove messages**, remove messages from queues
- **Retry failed messages**, Move messages from Error queue to its original queue
- **Send Command**, Build and send commands in an user friendly interface The commands can later be sent directly from command prompt
- **View Subscriptions**, View what Events are being subscribed and who is publishing them
- **Service Bus Integration**, Supports NServiceBus with MSMQ XML and JSON Transportation.
 
More information can be found at http://blog.halan.se/page/Service-Bus-MQ-Manager.aspx
