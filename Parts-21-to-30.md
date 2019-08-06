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
