# Part 21 - 15 July 2019 @ 20:00 AEST

Managed to bash out a few smaller features, and it was nice to not have epic fails on stream for once. We did deliberately avoid some of the stickier issues, but we'll get around to those on a later stream. Things we implemented:

* Get/Set of transport timeouts
* File system separator detection
* Envirnoment variable extraction
* Local time information (including time zone and UTC offset)

We'll be tackling channel management on a future stream, and probably one that's going to go for more than two hours!

[Vimeo](https://vimeo.com/348152283) - [YouTube](https://youtu.be/EmhslnJ7Ljg)

# Part 22 - 22 July 2019 @ 20:00 AEST

More features done! We implemened `getpid`, which was a no-brainer, but then worked through the pain of `kill`, which behind the scenes relied on the ability to call `ps`, so we implemented that as best we could. It wasn't nice though.

Thanks [@atwolf](https://twitter.com/atwolf) for capturing [the best moment of the stream](https://clips.twitch.tv/SpicyRamshackleCasetteWow).

[Vimeo](https://vimeo.com/349435899) - [YouTube](https://youtu.be/H4HRblDpCrs)

# Part 23 - 29 July 2019 @ 19:30 AEST

Worked through some more STDAPI features tonight, mostly revolving around file system related things (such as working folders, creating and removing directories, etc). We ended up trying to implement the first pass of `ls` which requried `stat` functionality. That turned out to be painful, but we're close to finishing that off. We ended up getting close but failed at the last hurdle where we're trying to serialize the STAT BUF complex type. We'll get this sorted on the next stream.

[Vimeo](https://vimeo.com/350724825) - [YouTube](https://youtu.be/fstk2GW_L-o)

# Part 24 - 6 August 2019 @ 10:00 AEST

First of the streams done in the morning! Fixed up the file stat functionality, and finally made the first version of the `ls` command work. We don't yet support globbing, so that will work down the track. At least we finished on a positive note! I did go down a couple of debugging paths that I didn't need to go down, but hey. It was a good learning experience for us all on how not to do things :)

[Vimeo](https://vimeo.com/352175807) - [YouTube](https://youtu.be/Ktows-47jAs)

# Part 25 - 12 August 2019 @ 20:00 AEST

Great stream tonight. Was happy with the result. We managed to get the globbing functionality finished in the `ls` command, and also bashed out support for:

* MD5 and SHA1 checksums
* File delete, move and copy
* Showing local drives/mounts

I skipped on the `search` function for now because that's going to be a bit of a PITA to implement. Might have to dedicate a whole stream to it.

Attendence was really good. We had some very old faces, very new faces, and very regular faces show up. The chat was really fun and constant. Hope to see you all on the next one too!

[Vimeo](https://vimeo.com/353346087) - [YouTube](https://youtu.be/EdlU3of_dM0)

# Part 26 - 2 September 2019 @ 20:00 AEST

Good to be back at it tonight after a couple of weeks off (thanks to work and sickness). We got some stuff done. We tidied up a bunch of code (formalised the Pokemon stuff, and moved PInvoke to it's own area), which made me a lot happier. We then made some effort to "properly" support the two CLR versions, and that resulted in a bunch of changes on the MSF and Rex-Arch side. Finally we started work on support for `shutdown` and `reboot` commands, which as you can imagine took longer than expected. We'll aim to finish that off as part of next stream.

[Rex-Arch commit](https://github.com/OJ/rex-arch/commit/281aaee0c5d148b9d45fe687815c013e8576e680) - [MSF Commit](https://github.com/OJ/metasploit-framework/commit/bf07d7ddfadab3b58b1765fcfb3c65dd5541dc82) - [Vimeo](https://vimeo.com/357335886) - [YouTube](https://youtu.be/EKKgJ7c1bqc)

# Part 27 - 11 September 2019 @ 08:30 AEST

This morning we started working on adding HTTP transport. It actually went quite well. We got started in the MSF side and implemented the basic payloads/handler stuff. Then we wired up some of the code on the Meterpreter side. We had to rebase our CLR code in MSF onto the current master branch because of a database issue, but that wasn't a huge problem. We did a little bit of refactoring so that we could reuse chunks of code, but the "pain" of the implementation was actually surprisingly low. We have got to the point where the comms works just fine, but we need to do some more work on the MSF side to make a few things wire up automatically.

Next stream we'll probably finalise this stuff, add HTTPS support (which should be super easy) and finalise all the transport-related core functions.

[MSF Commit](https://github.com/OJ/metasploit-framework/commit/a797a14b6c833b88ed71d41b2e7248c308ff5714) - [Vimeo](https://vimeo.com/359186482) - [YouTube](https://youtu.be/uF6ZPqyCLjs)

# Part 28 - 16 September 2019 @ 20:00 AEST

Tonight we finished off support for HTTP and HTTPS transports in both CLR 2 and 4. Good night, despite some of the pain that we felt. Clearly some of the nicer features of .NET are in the later versions, such as being able to validate SSL certificates on a per-request basis.

There's still more to do, and we'll aim to cover that off in the next stream. The first thing we'll do is add certificate hash validation to SSL so that we know we're close to feature parity with the native version. We also need to finish wiring in (and testing) other properties such as user agents (which we missed on tonight's stream). Oh well, such is life!

[MSF Commit](https://github.com/OJ/metasploit-framework/commit/5aff55f1d0f6cd623733485ca2bf5ac7cb988540) - [Vimeo](https://vimeo.com/360255369) - [YouTube](https://youtu.be/Nn92F9uLo3Y)

# Part 29 - 30 October 2019 @ 19:30 AEST

After well over a month we got back into some development, and I think it went really well. The plan was to work through transport-related stuff after getting HTTP/HTTPS transprots working last time.

First up, we made sure that our transports were able to support resiliency. This means that MSF can go down and the transports are able to correctly reconnect to the listeners when they come back up.

Once we had that in place, the aim was to move closer to the "transport add" functionality, but it made sense to add "transport list" first because we could use that for debugging. We got that done with a little bit of refactoring!

Next stream we'll aim to finalise the transport commands, and from there we may be in a position to get started on channels!

[Vimeo](https://vimeo.com/369797347) - [YouTube](https://youtu.be/klMWcViWWgs)

# Part 30 - 07 November 2019 @ 09:00 AEST

Today we implemented `bind_tcp`, and made sure that it worked as a "resilient" transport. From there we added support to add transports on the fly (of all types) and also validated that they get invoked correctly when transports fail (so that they rollover as you would expect).

We may have to quickly address the listing functionality to make sure that what's shown is indicative of what's going on behind the scenes. We'll cover that in the next stream when we add support for removing transports.

[MSF Commit](https://github.com/OJ/metasploit-framework/commit/7708060c8444e6f1b01cca95334b0aa2ef8e4a55) - [Vimeo](https://vimeo.com/371523266) - [YouTube](https://youtu.be/uCTdsL14c8c)
