EFCodeFirstProviderWrapperDemo
==============================

Entity Framework Code First approch providers demo - brings support for Microsoft EFCachingProvider and EFTracingProvider (Exists in Code Gallery) in Code-First Approch.

Files & Changes
==============================

This git project contains the original solution created by Microsoft (http://code.msdn.microsoft.com/EFProviderWrappers) in addition to a sample project I've created.
I've made two changes to Microsoft's files:
	- Edited EFCachedCommand: Added another constructor which accepts the wrapped provider invariant name.
	- Edited EFTracingCommand: Added another constructor which accepts the wrapped provider invariant name.

ExtendedDbContext
==============================

In the sample project - you can find ExtendedDbContext. This is a DbContext exnteded class which implements the features of tracing and caching.
In order to use the caching and/or tracing in your project, you should:
1. Copy & Paste to your project the ExtendedDbContext.
2. Extend your own custom DbContext from ExtendedDbContext.
3. Create a constructor that derrived from ExtendedDbContext(string nameOrConnectionString, bool contextOwnsConnection, bool enableTracing, bool enableCaching).
4. [Optional] In case you're using cacing - you have to set the Cache and CachingPolicy attributes of the ExtendedDbContext class.

ASP.NET & ASP.NET MVC
==============================
In case you're using ASP.NET or ASP.NET MVC, you can take a look on Microsoft ASP.NET Forms example - they've done a minor change which is usage of AspNetCache class for caching (which uses HttpContext as memory storage).
You can use the exact same approch in ASP.NET MVC and it'll integrate with ExtendedDbContext.

Licensing and Ownership
==============================
This project is subjected to Microsoft licensing terms, which supplied in the README and LICENSE files.
The ExtendedDbContext and sample project I've created subjected to the BSD License.

TODO & Knwon Bugs
==============================
1. This project won't work with StackExchange MiniProfiler. It seems like they've used also wrapped connection with interrupt with Microsoft's wrappers.