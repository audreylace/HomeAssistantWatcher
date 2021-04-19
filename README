# HomeAssistantWatcher

## What is it?

A sample program to show how to use [SharpHomeAssistant](https://github.com/audreylace/SharpHomeAssistant). It connects to a Home Assistant server via the [WebSocket API](https://developers.home-assistant.io/docs/api/websocket/) and subscribes to all events. When an event is received it writes it out to the command line via standard out.

## How do I build it?

This program is written in [.Net 5.0](https://dotnet.microsoft.com/download/dotnet/5.0). Make sure you have the .Net runtime and SDK installed.

- Checkout this repository via git along with the [SharpHomeAssistant repo](https://github.com/audreylace/SharpHomeAssistant) to the same folder.
- Rename SharpHomeAssistant folder to SharpHomeAssistant if it is not already named that.
- Run dotnet build in the HomeAssistantWatcher directory.
- The program will be placed in **HomeAssistantWatcher/bin/Debug/net5.0** and named **HomeAssistantWatcher** (**HomeAssistantWatcher.exe** on windows).

## How do I use it?

The program takes 2 command line arguments:

- **--uri** - This is the websocket path to home assistant server.
- **--accessToken** - The long lived access token that is used to authenticate with the Home Assistant server.

> ### Example
>
> HomeAssistantWatcher --uri="ws://myhomeassistantserver:1234/api/websocket" --accessToken="a.access.token.generated.from.home.assistant"

## References

- [Home Assistant WebSocket API](https://developers.home-assistant.io/docs/api/websocket/)
- [SharpHomeAssistant repo](https://github.com/audreylace/SharpHomeAssistant)
- [.Net 5.0](https://dotnet.microsoft.com/download/dotnet/5.0)
