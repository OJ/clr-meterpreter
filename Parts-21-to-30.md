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
