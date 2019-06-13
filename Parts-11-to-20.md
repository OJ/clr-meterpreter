# Part 11 - 17 May 2019 @ 20:00 AEST

Fixed reading of payloads off the wire, got to the point where we could invoke Met.Core.Server and we're close to being able to dispatch packets.

[Agenda](../master/streams/2019-05-17-Part-11/agenda.md) - [MSF Commit](https://github.com/OJ/metasploit-framework/commit/0154aa70903019eda1c7fd6e28d799922cf7f584) - [Vimeo](https://vimeo.com/336789460) - [YouTube](https://youtu.be/D03bc0dz01o)

# Part 12 - 23 May 2019 @ 20:00 AEST

I was all over the place tonight. Stumbled on a case where we need to support compressed data, but took ages to figure out why. Re-added flags attributes to the MetaType as we caused a few issues removing it in the past. We've got some handling in place for core commands, and will look to fill it out further when done. More to come. We're in the weeds at the moment but we'll start coming out of them soon to do the good stuff. Next stream we'll implement support for compression and rewrite some stuff on the MSF side.

[Vimeo](https://vimeo.com/337992280) - [YouTube](https://youtu.be/UQbGfzvCLrs)


# Part 13 - 24 May 2019 @ 20:00 AEST

Wow, that was ... frustrating! We did make progress in the end though. In this stream we moved the CLR payload and session code in MSF so that it was no longer associated with the `windows` platform. As a result, we ended up going down an awful rabbit hole to try to understand why things broke and how to fix them. Thankfully we had it figured out in the end, but it took a while. The big things to remember from this exercise are:

* Ruby is awful.
* MSF's error handling doesn't give you the information you need to fix things (most of the time).
* "Strongly" typed classes are required for individual platforms.
* Don't forget to add any new platforms to the likes of `multi/handler` or you end up with errors saying that your payload isn't correct!

It was fun nevertheless. We got to the point where we had created a stdapi instance, and were attempting to load it on the fly. We haven't got there yet though. Things we need to address include:

* Making sure that type references back to `metsrv` work so that the assemblies loaded on the fly can find the right types.
* Storing the CLR version with the session/listener information so that we load the right payload off disk down the track when we get to the point where we have `net40` in place as well.

[MSF Commit](https://github.com/OJ/metasploit-framework/commit/8cd16bc43b26e8ce98b99c72c5c3f8cae269fe9c) - [Rex Arch Commit](https://github.com/OJ/rex-arch/commit/46519f5533be959b94b5b03f6dcc665139bb2619) - [Vimeo](https://vimeo.com/338247816) - [YouTube](https://youtu.be/RGWseXls72w)

# Part 14 - 10 Jun 2019 @ 20:00 AEST

In this stream we managed to get the plugin system working with assembly resolution events. We rejigged the dispatcher so that we can support blocking and non blocking events. We adjusted the callbacks so that they can tell the caller whether or not to exit, and we added initial support for `getuid`. Progress!

[MSF Commit](https://github.com/OJ/metasploit-framework/commit/7f6540f7b56f582bc350fde2ab92bddec1b29e32) - [Vimeo](https://vimeo.com/341324902) - [YouTube](https://youtu.be/D4X5sGKpAXk)

# Part 15 - 13 Jun 2019 @ 20:00 AEST

More progress tonight, working through some of the stdapi function calls so that MSF sees our new CLR sessions as valid sessions. The  next few streams are going to be more of the same as we pad features out slowly.

[Vimeo](https://vimeo.com/342007094) - [YouTube](https://youtu.be/iOq5KAKzRBw)
