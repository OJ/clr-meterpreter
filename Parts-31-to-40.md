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

# Part 33 - 12 May 2020 @ 20:00 AEST

Good constructive session tonight. The goal was to finalise the reading and writing to and from TCP client channels, and have them function as we would expect from both ends. We were able to achieve this goal! Standard port forwards are now a funcional thing, and they work nicely (as proved by curling and nc).

Handling other types of channels should be easier now that we have this basic infrastructure in place. However, there is still more to do on the TCP client channel before we can put it to bed. Next stream, we'll aim to provide all of the channel options that are available. We also need to make sure that we cater for the error condition where the client isn't able to connect to the target.

Once that's one of the way, we can move on to handling TCP server channels (reverse port forwards), and get going on file and process channels.

[Vimeo](https://vimeo.com/417591044) - [YouTube](https://youtu.be/REU09qYkTrI)

# Part 34 - 19 May 2020 @ 20:00 AEST

So the plan with this session was to bash out the implementation of process channels. The aim was to allow for process execution, and them from there support interactivity via the channel abstraction. This would not only mean we could fire up processes, it would also mean that we would have reworked the channel code to be more abstract to support different channel types.

It started so well! We even managed to merge Metasploit master relatively easily. But we hit a pretty crappy roadblock as soon as we got to handling stdout/stderr from the .NET Process object.

The first couple of hours of the stream were fun and constructive. The latter two hours were a shitfight :D I got angry, frustrated, and tried a bunch of things that didn't work. It was nice that other people were hanging out and helping me work through it, offering suggestions, etc. But they came unstuck with me. We weren't able to get to the bottom of the issue. At midnight I threw in the towel.

I don't like going to bed on a low note. But clearly something ticked over in my head last night, and I woke up this morning with an idea. So, sticking with my rule of making sure that I code everything live on stream, I wrote up a little project locally to play with the problem. While my original idea didn't work, it did lead me down the path of something that DID work.

So on the next stream, I'm going to implement that fix. Together, we can all lament about it.

[Vimeo](https://vimeo.com/420533237) - [YouTube](https://youtu.be/WLNiJWSAVmo)
