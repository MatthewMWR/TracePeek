# TracePeek
Interactive view for Event Tracing for Windows (ETW) events live as they happen. Works with PowerShell 6, C#/.Net Core, or cmdline. 
Note: If there is demand it should be feasible to make a version for PowerShell 5. Current version uses .Net Core so it easiest to integrate with PowerShell 6.

## Who should use TracePeek, and when
For interactive ETW scenarios TracePeek provides a lightweight alternative to the "install-WPT-then-log-and-repro-then-open-ETL-oops-bad-timing-try-logging-again..." pattern. For example:  
* Interactively waiting for some system event to occur 
* Exploring a new feature/provider to learn what events are fired

All of this assumes some level of expertise with Windows and ETW. For example TracePeek assumes you already know what providers you care about.

## When not to use TracePeek
Each event is fully parsed to support easy interactive use. This means that the per-event CPU cost is much higher than pure ETW logging (as controlled through  xpert or logman, etc.). Not recommended for high volume providers.

## Getting started
Powershell example:
```powershell
Install-Module TracePeek

Start-TracePeek -Providers "Microsoft-Windows-Wordpad" | Select-Object -Property ProviderName,Message

## Now launch Wordpad while it is running to get some events
```
```
Starting up...
Press ALT+s to stop
 
 ProviderName              Message
 ------------              -------
 Microsoft-Windows-Wordpad Intializing current instance of the application
 Microsoft-Windows-Wordpad Wordpad Launch Start.
 Microsoft-Windows-Wordpad Wordpad Launch End.
 Microsoft-Windows-Wordpad Exiting current Instance of the application

Cleaning up...
```

## What is the output?
By default TracePeek emits each event as a PSCustomObject where the property names include standard ETW items (e.g. ProviderName, Keywords), plus named properties based on the event payload. This allows for easy interaction with PowerShell patterns like piping to Select-Object, Group-Object, etc.

Alternative object shapes are available via the -ProjectionStyle parameter. For example, if you were going to export the results to CSV you might prefer the payload properties to have fixed names like "Field1","Field2", etc. rather than "EventA-HasThisProperty","EventB-HasThisOtherProperty".
```powershell
Start-TracePeek -Providers "Microsoft-Windows-GroupPolicy" -ProjectionStyle NumberedPayloadProperties
```

Stop reading, go play with TracePeek, and provide feedback. Thanks.

## Notes for ETW geeks

### Relationship to other tools
TracePeek is a thin wrapper around Vance Morrison's wonderful TraceEvent library. Vance's library provides broad coverage of diverse ETW scenarios, while TracePeek optimizes for my narrow use case (interactivity & PowerShell friendliness).

TracePeek is intended to fill a narrow gap in the ETW ecosystem (interactviity and PowerShell friendliness), not to compete with already-great-at-other-workflows tools like WPT/WPR/xpert/WPA/PerfView, etc.

### Levels and Keywords
For providers with a mix of high and low volume events, ETW Levels and Keywords can be an efficient filtering mechanism. To support this TracePeek allows (like xperf) for colon separated Levels and Keywords. In other words:
-Providers "Microsoft-Windows-Winlogon:0xFFFF:0xFFFFFFFFFFFFFFFF" is equivalent to
-Providers "Microsoft-Windows-Winlogon"

### Valid identifiers for providers
TracePeek doesn't do anything special to parse provider names specified by the caller. The caller is subject to normal ETW rules about provider names, for example:
- If you know the Guid for the provider, that should always work
- You can use the name of the provider *if* that can be resolved to a Guid at run-time. The two known cases of this are:
  - The provider name is registerd with the OS-- i.e., it can be seen in the output of logman.exe providers
  - -OR- Whoever created the provider did not declare a specific Guid, and instead allowed the provider Guid to be a function of the provider name (aka the EventSource pattern).

### What about the NT kernel provider, kernel flags, etc.?
Given that kernel events tend to be high volume, I have never had a scenario where I wanted TracePeek to work with them. If there is a cool scenario for using TracePeek with the NT kernel provider we could add support, probably using named groups of kernel flags similar to xpert and PerfView.
