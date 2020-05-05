# Part 31 - 30 April 2020 @ 20:00 AEST

After a very long period of down time, thanks to life, work, rock climbing, and streaming CTF stuff, we're back with another .NET Meterpreter dev stream! I really enjoyed getting back into this.

The session was a bit of a refresher for many of us, with some discussion of the inner workings of some of the existing MSF features. We spent a fair bit of time looking at how transport management works on both sides of the comms channel, and tweaked the MSF side a bit to make a bit more sense for us. We also implemented the ability to remove transports on the fly as promised.

We also fixed a small issue where setting the return code/result on a packet multiple times was allowed and shouldn't have been! We may refactor this a little back into the TLV space at some point, but this will do for now.

We're getting to the point where we can't delay implmenting channels, so I think we might just embark on that epic adventure on the next stream.

[MSF Commit](https://github.com/OJ/metasploit-framework/commit/912329c6fbe8158f20194df82e72691853d7ee09) - [Vimeo](https://vimeo.com/413547361) - [YouTube](https://youtu.be/7oICP27gCAE)

# Part 32 - 05 May 2020 @ 20:00 AEST

As promised, we got started on building channel support into our CLR Meterpreter implementation. This is a big beast, and will probably end up taking a bit longer than I first thought. This is because this area of the Meterpreter functionality is not one that I'm intimately familiar with, and hence we're all learning about this stuff together.

We decided to just get started with the simplest channel: TCP server (via the port forward functioanlity). This allows us to ge across the most basic usage of a channel before providing a better abstraction layer across other channel types such as processes and files.

After covering off some of the theory, we got started with code and ended up getting to a point where MSF thinks we have a valid channel object on the other end of a TCP client channel. We do have an active and valid connection, but we don't have any form of channel management in place, nor the ability to shuffle data around as we would like.

Next stream we'll try to finalise the data shuffling bit, and add the abstraction that allows us to talk to the channel while its doing the shuffling thing.

[Vimeo](https://vimeo.com/415118815) - [YouTube](https://youtu.be/JgW4ks6L-z4)
