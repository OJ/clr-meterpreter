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

# Part 16 - 17 Jun 2019 @ 20:00 AEST

On this stream we went back to fill in the P/Invoke approach. We explored how it worked, and why we wanted to hide it away. The result was that we were able to get it working in a way that allowed for the functionality to be enabled at runtime rather than compile time. The goal of the next stream is to abstract this functionality away into something reusable, while also adding full support for loading the dynamic types into another app domain.

[Vimeo](https://vimeo.com/342722147) - [YouTube](https://youtu.be/Mxv-_Y2CDpE)

# Part 17 - 20 Jun 2019 @ 20:00 AEST

Collectively we decided to support TLV encryption. We quickly found that for some reason .NET doesn't natively support parsing PEM-formatted public keys, and so (ironically) we ended up having to steal an open-source implementation that used the native crypto APIs via P/Invoke (ugh). This code looked to work in a similar way to how the native Meterpreter PEM parsing worked. So we basically stole someone else's code. Yay! I didn't have to write it again, that's a huge win.

From there we butted heads against AES encryption before finishing the stream late. In short, we didn't get it working, but we're very close.

As soon as I finished streaming I realised what was wrong. Two things:

1. I was using the wrong padding (it should have been ISO10126).
1. I was decrypting the packet body _before_ XOR'ing it with the key, which was resulting in absolute garbage.

A quick local test showed that as soon as fixed that up, things "just worked". I reverted those changes (but left some comments in) so that we can finish it off on stream next week.

[Vimeo](https://vimeo.com/343435543) - [YouTube](https://youtu.be/c2bQ7xc3wlY)

# Part 18 - 24 Jun 2019 @ 20:30 AEST

We managed to fix up the TLV encryption code, and thankfully saw a functional Meterpreter session using TLV encryption. Metasploit even sees it as valid. We went back to the default padding algorithm though, as I was wrong about that at the end of the previous stream.

Once getting that working we moved on to sorting out the version detection code, only to get bitten by our use of `RtlGetVersion`. Just like last time, I figured out exactly what was going wrong within seconds of killing the stream. Note: make sure you understand the difference between `out` (ie. "I'll give you a thing") and `ref` (ie. "You give me a thing and I'll fill it") parameters. We used the former and should have used the latter.

We'll get that fixed at the start of the next stream.

[Vimeo](https://vimeo.com/344077231) - [YouTube](https://youtu.be/rXPAOO7SHok)

# Part 19 - 1 July 2019 @ 20:00 AEST

As promised we fixed up the issue with the use of `ref` vs `out` and that got us going with the version information. From there, I wanted to move onto finishing off the network interface enumeration (avoiding the use of P/Invoke as we went). I intended to use a native Meterpreter as a baseline and so fired up a standard session to see what the output was. Unfortunately for me, that uncovered an issue in the implementations of `core_enumextcmd` and `core_loadlib` (I assumed that the return type of the commands was the same, and it was not!). We dived into the MSF side for a while to fix that up before moving back onto the network interface enumeration.

We nearly finished it, but didn't quite get there. I have a good feeling we'll nail that on the next stream and get going with more stdapi functionality after.

[MSF Commit](https://github.com/OJ/metasploit-framework/commit/3d32f5e14a8c7dfd245bc5d55c5a66cf16eda375) - [Vimeo](https://vimeo.com/345456148) - [YouTube](https://youtu.be/_IOoPGH_E20)
