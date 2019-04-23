# From Smashery

- Are we going to be using Nuget?

# From FireFart

- I think the part where you need to agree on a minimum supported version will be the hardest to begin with as many .NET features will not be available if you target an older CLR like 2.0 (Tasks for example). Maybe it helps if we choose the supported OS before, because that will already limit the versions. I think XP can be handled by older meterpreter for example. Maybe you can also say meterpreter is only for Win10/Server2016 and up, older OS need to use the old meterpreter. This would simplify development, because you can make use of "all the new cool shit" :)
- Can we add transport and protocol level encryption by default on the new implementation? So not even add an option to disable encryption (on TLV packets for example)
- will the stagers also be .NET based or is the plan to use the old stagers to load the new meterpreter code?
- should we think about obfuscating (at least class names) during compile time to beat some simple pattern based AVs?
- I would love to see some unit tests during development to make sure the code works and to also use it to document some use cases and how the code should behave
- btw you can also think about some powershell wrapper that simply calls the meterpreter.dll (but would touch disk). but this way you would have an easy powershell meterpreter as it simply calls the native functions. But powershell CLR mode will prevent that :)
- another point is .NET client profile or the full one
