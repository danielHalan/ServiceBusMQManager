Service Bus MQ Manager - v4.xx
======================================================================
An application to view and manage Service Bus messages.

## Overview
Service Bus MQ Manager works by monitoring a Queue and parsing the Messages presenting them in a list sorted by Commands, Events, Messages and Errors. Currently MSMQ and Azure Service Bus queues are supported, and message parsing for [MassTransit](https://github.com/jjchiw/ServiceBusMQManager)  and [NServiceBus](https://github.com/danielHalan/ServiceBusMQManager) is supported, its also possible to [create a new Service Bus Adapter](https://github.com/danielHalan/ServiceBusMQManager/wiki/Building-a-Service-Bus-Adapter) for other Service Bus messages & Queue providers.

The application is most useful when used in a [CQRS architecture](http://cqrsinfo.com), where its common to send Commands and Event messages using a Service Bus.

- **Monitor Queues**, Monitor queues for incoming Commands, Events and Messages envelops, also possible to monitor Error Queue
- **Purge messages**, remove messages from queues
- **Retry failed messages**, Move messages from Error queue to its original queue
- **View Processed Messages**, Load messages from the Journal queues. For use cases such as, show messages that got processed before Service Bus MQ Manager was started, or messages that where added and processed before the Service Bus monitor process discovered them.
- **Send Command**, Build and send commands in an user friendly interface The commands can later be sent directly from command prompt
- **View Subscriptions**, View what Events are being subscribed and who is publishing them
- **Service Bus Integration**, Supports NServiceBus with MSMQ XML and JSON Transportation, with possibility to extend to other service buses or new transportation methods.

More information can be found at http://blog.halan.se/page/Service-Bus-MQ-Manager.aspx


## License

The source code is released under the RPL License, 
http://opensource.org/licenses/rpl1.5.txt

