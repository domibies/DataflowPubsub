[![.NET](https://github.com/domibies/DataflowPubsub/actions/workflows/mydotnet.yml/badge.svg)](https://github.com/domibies/DataflowPubsub/actions/workflows/mydotnet.yml)

### DataflowPubsub 

A class library for an in memory, asynchronous message based publish subscribe pattern to communicate between task based/multithreaded components. 

The implementation uses Dataflow (Task Parallel Library) components.

### What's new

Version 1.1.0, 9 dec 2021 : removed superfluous BufferBlock in subscriber

#### How To Use

Add a reference to the nuget package 
```
dotnet add package DataflowPubsub
```

Create subscribers trough an instance of `MessageBus`. You subscribe to any `BaseMessage` derived type (a few basic types like `TextMessage` and `BinaryMessage` are defined in the library). Any message can have a 'topic' that can be used in filtering. (Any public message properties can be filtered upon)

```csharp
var messageBus = new MessageBus();
// optional predicate is used as a filter for the subscription
var subscriber = messageBus.CreateSubscriber<TextMessage>(m => m.Topic=="MyTopic"); 
```

You can publish any `BaseMessage` to the bus through the public property `Sender` (`ITargetBlock<BaseMessage>`) of the bus, like this:
```csharp
await messageBus.Sender.SendAsync(new TextMessage("My Message","MyTopic"));
```

Subscribers instances can then be used to read asynchronously from the filtered message subscription, via the public property `Receiver` (`ISourceBlock<TextMessage>` in this case), like this:

```csharp
var message = await subscriber.Receiver.ReceiveAsync();
```

![name-of-you-image](https://github.com/domibies/DataflowPubsub/blob/31f09e60c322372bf453e86e91eae32809131586/Images/mermaid-pubsubdataflow.png)
