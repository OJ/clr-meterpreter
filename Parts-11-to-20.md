# Part 11 - 17 May 2019 @ 20:00 AEST

Fixed reading of payloads off the wire, got to the point where we could invoke Met.Core.Server and we're close to being able to dispatch packets.

[Agenda](../master/streams/2019-05-17-Part-11/agenda.md) - [MSF Commit](https://github.com/OJ/metasploit-framework/commit/0154aa70903019eda1c7fd6e28d799922cf7f584) - [Vimeo](https://vimeo.com/336789460) - [YouTube](https://youtu.be/D03bc0dz01o)

# Part 12 - 23 May 2019 @ 20:00 AEST

I was all over the place tonight. Stumbled on a case where we need to support compressed data, but took ages to figure out why. Re-added flags attributes to the MetaType as we caused a few issues removing it in the past. We've got some handling in place for core commands, and will look to fill it out further when done. More to come. We're in the weeds at the moment but we'll start coming out of them soon to do the good stuff. Next stream we'll implement support for compression and rewrite some stuff on the MSF side.

[Vimeo](https://vimeo.com/337992280) - [YouTube](https://youtu.be/UQbGfzvCLrs)

