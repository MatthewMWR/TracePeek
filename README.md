# TracePeek
Interactive view for Event Tracing for Windows (ETW) events live as they happen. Works with PowerShell 6, C#/.Net Core, or cmdline. 
*Note: If there is demand it should be feasible to make a version for PowerShell 5. Current version uses .Net Core so it easiest to integrate with PowerShell 6.*

## Who should use TracePeek, and when
For interactive ETW scenarios TracePeek provides a lightweight alternative to the "install-WPT-then-log-and-repro-then-open-ETL-oops-bad-timing-try-logging-again..." pattern. For example:  
* Interactively waiting for some system event to occur 
* Exploring a new feature to learn what events are fired as you take different actions

All of this assumes some level of expertise with Windows and ETW. For example TracePeek assumes you already know what providers you care about.

## When *not* to use TracePeek
Each event is fully parsed to support easy interactive use. This means that the per-event CPU cost is much higher than pure ETW logging (as controlled through  xpert or logman, etc.). Not recommended for high volume providers.

## Getting started
Powershell example:
```powershell
Install-Module TracePeek

Start-TracePeek -Providers "Microsoft-Windows-Wordpad" | Select-Object -Property ProviderName,Message

## Now launch Wordpad to get some events
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
By default TracePeek projects each event as a PSCustomObject. Each object has property names from standard ETW header (e.g. ProviderName, Keywords), plus property names from the event specific payload (e.g., MyAppSessionId). This allows for easy interaction with PowerShell patterns like piping to Select-Object, Group-Object, etc.

Alternative object shapes are available via the -ProjectionStyle parameter. For example, if you were going to export the results to CSV, or wanted to always select the first payload field, you might prefer the payload properties to have fixed names like "Field1","Field2", etc. rather than "EventAHasThisProperty","EventBHasThisOtherProperty".
```powershell
Start-TracePeek -Providers 'Microsoft-Windows-GroupPolicy' -ProjectionStyle NumberedNestedPayloadProperties | Format-Table Name,Field0,Field1

## Now run gpupdate.exe /force in another cmd prompt
```
```
  Starting up...
  Press ALT+s to stop
  
  Name          Field0                               Field1
  ----          ------                               ------
  EventID(4004) PolicyActivityId : 166a7d37-f0cc-458 PrincipalSamName : …
  EventID(5340) PolicyApplicationMode : Background    :
  EventID(5311) PolicyProcessingMode : "No loopback   :
  EventID(4126) IsMachine : True                      :
  EventID(5257) IsMachine : True                       PolicyDownloadTimeElapsedInMilliseconds : 6…
  EventID(5126) IsMachine : True                     IsBackgroundProcessing : False
  EventID(5312) DescriptionString : None             GPOInfoList :
  EventID(5313) DescriptionString : Local Group Poli GPOInfoList : <GPO ID="Local Group   Policy">…
```

Now stop reading, go play with TracePeek, and provide feedback. Thanks.


## *Extra notes for ETW geeks*

### Relationship to other tools
TracePeek is intended to fill a narrow gap in the ETW ecosystem (interactvity and PowerShell friendliness), not to compete with tools like WPT/WPR/xpert/WPA/PerfView, etc. which are already great at other workflows.

TracePeek is a thin wrapper around Vance Morrison's wonderful TraceEvent library. The TraceEvent library provides broad coverage of diverse ETW scenarios, while TracePeek optimizes for a specific use case (interactivity & PowerShell friendliness).

### Levels and Keywords
For providers with a mix of high and low volume events, ETW Levels and Keywords can be an efficient filtering mechanism. Like xperf, TracePeek allows for including Levels and Keywords in the input along with provider names. Example:
-Providers 'Microsoft-Windows-Winlogon:0xFFFF:0xFFFFFFFFFFFFFFFF' is equivalent to
-Providers 'Microsoft-Windows-Winlogon'

### Valid identifiers for providers
TracePeek doesn't do anything special here, so the caller is subject to normal ETW behavior about provider names, for example:
- Specifying the provider by Guid (like -Providers 'DBE9B383-7CF3-4331-91CC-A3CB16A3B538') is the most reliable, but sacrifices readability and puts the burden on the caller to know the Guid
- Specifying the provider by name (like -Providers 'Microsoft-Windows-Winlogon') is more readable, but works only if that name can be resolved to a Guid at run-time. The two known cases of this are:
  - The provider name is registerd with the OS-- i.e., it can be seen in the output of logman.exe providers
  - -OR- The original author of the provider used the Tracelogging/EventSource pattern, and did not declare a specific Guid, thus allowing the Guid to be a function of the name. Unfortunately there is no OS based discovery mechanism in the Tracelogging/EventSource pattern, so the caller would need to learn this fact about the provider out-of-band.

### What about the NT kernel provider, kernel flags, etc.?
Given that kernel events tend to be very high volume, I have never had a scenario where I wanted to use TracePeek with them. If there is a cool scenario for using TracePeek with the NT kernel provider we could add support, probably using named groups of kernel flags similar to xpert and PerfView.
