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

# Part 35 - 26 May 2020 @ 20:00 AEST

As promised in the notes on the previous stream, we started this stream by fixing the issues with the process channels we started on the last one. I whipped through it using a sample program I wrote behind the scenes, tidied a few things up and moved on to TCP Server channels.

We managed to get through thoughts without too much pain, which was great. The channels appear to function as they should, even though we haven't done anything with buffering yet.

So on the whole, progress on the channel front is going well. I know that when the time comes to support things like impersonation when creating processes that things will get a little trickier, but for now we'll just ride the small wave of wins.

Up next week? Really not sure! Let's play it by ear.

[Vimeo](https://vimeo.com/422770738) - [YouTube](httpsi//youtu.be/WLNiJWSAVmo)

# Part 36 - 02 June 2020 @ 20:00 AEST

On this stream we added support for simple "file channels". This allows us to provide the underlying functionality required to drive multiple commands from Metasploit, including `upload`, `download`, `edit` and `cat`. To support this we had to work on our channel abstraction a little and refactor the interface to make it more sane.

It would appear so far that the notion of "class" for a channel indicates what its capabilities are, so next stream we may need to revisit this stuff to make sure we are doing the right thing. We'll probably also make sure that the flags are accurate as well, as I'm still not sure where and how "buffered" vs "sync" channels are used.

Another great turnout with a few new faces. Thoroughly enjoyed it.

[Vimeo](https://vimeo.com/425109674) - [YouTube](https://youtu.be/b9smcKzwj2k)

# Part 37 - 09 June 2020 @ 20:00 AEST

We started by checking that a few things works as expected (such as recursive downloads). We then properly wired in the EOF check for file streams before implementing the `play` feature. This was a bit of fun and we all got to hear the magical sounds of Batman.

After that, we collectively made the decision to dive into packet pivoting. The beginnings of the infra have been built to make this happen, though we didn't get to the end. We have the ability to listen on Named Pipes and we can even start the staging process. Exciting times.

On the next stream we need to inform MSF of new client connections, handle session guids, and handle the dispatching of packets to different sessions based on their GUIDs. Plenty to do, but should be fun.

[Vimeo](https://vimeo.com/427362794) - [YouTube](https://youtu.be/DuOeCfOQxBQ)

# Part 38 - 17 June 2020 @ 20:00 AEST

The aim of the stream was to finish off the packet pivots. However, this was one of those sessions where things didn't go as intended. It took a bit to get into the vibe, and for some reason I had forgotten to send the size of the stage down the wire when the pipe connection first came in. As a result, we had a little sojourn into debugging native Meterpreter to figure out what was going on. That was fun and educational, but a bit of a distraction.

We ended up not getting it done, thanks to some odd behaviour with NamedPipeServerStream objects wrapped up in BinaryReader objects. For some reason reading a subset of data off the pipe doesn't seem to be working as we would like. This is something that we're going to have to get to the bottom of on the next stream.

[Vimeo](https://vimeo.com/429968828) - [YouTube](https://youtu.be/FjzGMskTPAg)

# Part 39 - 29 July 2020 @ 20:00 AEST

First time back at streaming for well over a month. Took a bit to get back into the swing of it, but we got to the point where we were able to read packets from the Named Pipe pivot after a bit of mucking around. The code is definitely messy and requires some refactoring, but we're at least at a point where we're able to communicate with both ends of the pivot.

Next stream we're going to finalise this code, make sure that MSF is able to handle the pivoted session, and that the middle agent is able to keep track of the packets and agents that are pivoting through it. From there we can refactor the code to be less messy and move on.

[Vimeo](https://vimeo.com/)442677826 - [YouTube](https://youtu.be/0awhQUW7-7I)
