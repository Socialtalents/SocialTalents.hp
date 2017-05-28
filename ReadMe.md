# SocialTalents HP Toolkit
HP = means 'HelPer'
## Repository structure

Samples.Console - Sample implementation

SocialTalents.Hp.Events.net45 - Inversion of Events handing (for .net Framework 4.5+)

SocialTalents.Hp.Events.nettandard - Copy for .netStandard (copis all files in pre-build step)

SocialTalents.Hp.MongoDb - Helper classes for mongodb (TBD)

SocialTalents.Hp.UnitTests - Tests for all SocialTalents.Hp.* projects

## Usage

### Publish events

```EventBus.Publish<TEvent>(eventInstance, sender);```

TEvent - identify workflow queue

eventInstance - event to handle (may be child class of TEvent)

sender - (ussually this) Any class which declares ICanPublish< TEvent >  interface

### Subscribing to events

Using lambda functions:
```
EventBus.Subscribe<TEvent>((eventInstance) => { do something with eventInstance });
```

using handler function:
```
void eventHandler(TEvent eventInstance) { }
...
EventBus.Subscribe<TEvent>(eventHandler);
```
using interface:
```
class ServiceToHandleEvents: ICanHandle<TEvent> {
   public void Handle(TEvent eventInstance) { ... }
}
...
EventBus.Subscribe<TEvent>(serviceToHandleEvents);
```
### Modifying handler calling strategy

Multiple handlers are ok:
```
EventBus.Subscribe<TEvent>(handler1);
EventBus.Subscribe<TEvent>(handler2);
EventBus.Subscribe<TEvent>(handler3);
```
Async handling:
```
EventBus.Subscribe(handler.Async());
```
Fail strategy:
```
EventBus.Strategy(handler.AddOnFail<TEvent, TException>(onFailHandler));
```
Strategies can be combined, for example OnFail with Async:
```
EventBus.Strategy(handler.AddOnFail<TEvent, TException>(onFailHandler).Async());
```

## Requirements

Minimal dotnet framework required: 4.5.1 (.NET Standard 1.2) 
[Understanding .NET Standart vs .NET Framework](https://blogs.msdn.microsoft.com/dotnet/2016/09/26/introducing-net-standard/)

Works with .NET Core and .NET Framework