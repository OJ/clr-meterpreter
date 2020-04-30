# Part 31 - 30 April 2020 @ 20:00 AEST

After a very long period of down time, thanks to life, work, rock climbing, and streaming CTF stuff, we're back with another .NET Meterpreter dev stream! I really enjoyed getting back into this.

The session was a bit of a refresher for many of us, with some discussion of the inner workings of some of the existing MSF features. We spent a fair bit of time looking at how transport management works on both sides of the comms channel, and tweaked the MSF side a bit to make a bit more sense for us. We also implemented the ability to remove transports on the fly as promised.

We also fixed a small issue where setting the return code/result on a packet multiple times was allowed and shouldn't have been! We may refactor this a little back into the TLV space at some point, but this will do for now.

We're getting to the point where we can't delay implmenting channels, so I think we might just embark on that epic adventure on the next stream.

[MSF Commit](https://github.com/OJ/metasploit-framework/commit/912329c6fbe8158f20194df82e72691853d7ee09) - [Vimeo](https://vimeo.com/413547361) - [YouTube](https://youtu.be/7oICP27gCAE)
