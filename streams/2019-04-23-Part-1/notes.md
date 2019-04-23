CLR Meterpreter
===============

# Rationale

## Good feelz
* I want to give back.
* I want to educate.

## offsec reasons
* Powershell is dying.
* Native code exec == hard.
* Dynamic languages == poo -- javascript, python, ruby, etc.
* .NET/CLR is now on basically every windows host.
    - There is a .NET runtime of sorts on other machines too.
* JIT'd languages often have a lot of RWX pages

## Why Meterpreter
* Longer shelflife for meterp
* share knowledge of meterp in particular
    - you guys can contribute to it too
* Other ideas of how .NET stuff can be utilised and abused in .NET Meterpreter
    - New extensions
* Add more transports
    - WCF -> binary formatted TCP sockets

## Selfish reasons
* Because I like C#... more than most other languages that are mainstream.
    - If I could, I'd use F#.
* Need to keep streams shorter -- the long ones kill me!
* Twitch Affiliate

# Major implementation areas
* Stagers
    - support various ways to invoke the payload, ASPX, EXE, DLL, C#, MSIL
    - Supporting encoding/encryption of payloads
* The ability to load a stage and the extensions
    - How do we implement things like Mimikatz/Incognito?
* Transports!
    - HTTPS, HTTP, TCP, SMB/NamedPipe, (DNS...)
* TLV "Protocol"
    - Handling TLV packets properly
    - Handling encryption of packets properly
* Sessions
    - Managing the state of Meterpreter regardless of transport and connectivity
    - Transport Failover
* Channels
    - Provide the means for multiple actions to stream communications over the single transport/session
* Features in basically every Meterp
    - File IO
    - Process management
    - Interacting with the registry
    - Webcam/mic/trolling
    - ....
* Migration
    - These days, this stands out like Dogs ...
    - Should we do the "powerpick" style CLR loading and create a .NET session in a native process?
    - Should we just inject a native Meterpreter on migrate?
* Threading
    - Most commands are implemented on a separate thread
* Pivoting
    - Packet pivots
* Native vs Non-native
    - Migration
    - Process injection
    - Keylogging
* Railgun
    - Implementation of the Win32 API callable via Metasploit
* Configuration
    - taking the configuration of the listener (in MSF) and creating relevant transports/etc.
* Supporting post modules
    - Railgun
    - All the standard features
    - Loading of extension
    - We need to make sure that post modules that say they support "windows" work on .NET Meterpeter.
* venom payload generation
* PROXIES

## .NET specific considerations for implementation
* Which version(s) do we support?
    - Visual Studio version?
    - 2.0 --- 4.9..?
    - Mono.net and .NET Core
* AppDomains
* P/Invoke
* Mixed Mode Assemblies
* Dynamic Assembly Generation
* Built in C# compiler

## Validation of functionality
* How do we test this?

# Contstraints, gotchas, traps, pitfalls
* When we create threads in an assembly that was loaded from memory in a .NET application, are those memory areas marked as MEM_IMAGE? (ie. will they bypass Get-InjectedThread.ps1).
* Time/management/etc.
* Metasploit
    - TLV packets
    - Expose the features
    - "Chattiness"
    - Mutual auth -- this is not something that MSF currently does.
    - Can't easily handle multiple users talking to the same meterpreter session at once

