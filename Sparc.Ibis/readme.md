# Sparc.Ibis
[![Nuget](https://img.shields.io/nuget/v/Sparc.Ibis?label=Sparc.Ibis)](https://www.nuget.org/packages/Sparc.Ibis/)

The Ibis translation plugin, powered by Ibis, auto-translates your entire app or site for you into hundreds of languages using the Ibis engine.

With a one-line component Sparc.Ibis auto-translates everything inside the component to the user's language! 
Usage is as follows:

1. Inside your platform project install the `Sparc.Ibis` Nuget package [![Nuget](https://img.shields.io/nuget/v/Sparc.Ibis?label=Sparc.Ibis)](https://www.nuget.org/packages/Sparc.Ibis/)

2. Add this line to your platform `wwwroot/appsettings.json`:
```json
{ "IbisApi":  "https://ibis.chat" }
```
> [Ibis.chat](https://ibis.chat/) is live, you can create your account now or you can use your own API

3. Add the following to your `Program.cs`
```csharp
using Sparc.Ibis;
//...
builder.Services.AddIbis();
```

4. Wrap whatever you want to translate in 
```razor
<IbisTranslate ChannelId="[the Ibis channel/room ID to post translations to]"></IbisTranslate>
```

 5. (optional) If you want to test a specific language, just pass it into the component: 
 ```razor
 <IbisTranslate ChannelId="ibis-app" Language="de">
 ```
That's it! No necessary JS to add either, because .NET 7 has introduced a new way to auto-embed JS modules from a Razor Class Library, which we're taking advantage of.

> Check here an [example of use in Ibis Project](https://github.com/sparc-coop/ibis/blob/168b94ce97f232815ce270fcd49cef0f2311c028/Ibis.Web/Shared/MainLayout.razor#L6)
