# TracePeek
TracePeek provides developers, power users, and IT Pros with live interactive Event Tracing for Windows (ETW). TracePeek works with cmdline, C#/.Net Core, and is particularly optimized for PowerShell 6.

## When to use TracePeek
For ETW scenarios which could be interactive, TracePeek provides a lightweight alternative to the cumbersome "install-WPT-then-log-and-repro-then-open-ETL-oops-bad-timing-try-logging-again-then-open-again..." pattern. For example:  
* Interactively waiting for some system event to occur 
* Exploring a new feature to learn what events fire as you take different actions

All of this assumes some level of expertise with Windows and ETW. For example, TracePeek assumes that you already know which providers are of interest to you.

## When *not* to use TracePeek
TracePeek fills a narrow gap in the ETW ecosystem (interactivity and PowerShell friendliness) and does not compete with tools like WPT/WPR/xperf/WPA/PerfView, et al. which are already great at other workflows.

Also, TracePeek parses each event to support easy interactive use and is not intended for high-volume events.

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
By default TracePeek projects each event as a PSCustomObject. Each object has property names from standard ETW header (e.g., ProviderName, Keywords), plus property names from the event-specific payload (e.g., MyAppSessionId). This object shape allows for natural interaction with PowerShell patterns like piping to Select-Object, Group-Object, and so on.

Alternative object shapes are available via the -ProjectionStyle parameter. For example, if you were going to export the results to CSV, or wanted to select the first payload field regardless of name, you might prefer the payload properties to have fixed names like "Field1" and "Field2".
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

Now stop reading, play with TracePeek, and let me know what you think. Thanks.


## *Extra notes for ETW geeks*

### Relationship to TraceEvent
TracePeek is a thin wrapper around Vance Morrison's lovely TraceEvent library. The TraceEvent library provides broad coverage of diverse ETW scenarios, while TracePeek optimizes for a specific use case (interactivity & PowerShell friendliness).

### What about WPP style events?
TraceEvent, and by extension TracePeek, can parse the payloads of Manifest style events and Tracelogging/EventSource style events. For old-school WPP events use other toolchains like tracefmt.

### Levels and Keywords
For providers with a mix of high and low volume events, filtering by ETW Levels and Keywords can be necessary for effefficiently limiting event volume. Like xperf, TracePeek allows for passing Levels and Keywords along with provider names. For example, 
-Providers 'Microsoft-Windows-Winlogon:0xFFFF:0xFFFFFFFFFFFFFFFF' is equivalent to
-Providers 'Microsoft-Windows-Winlogon'

### Valid identifiers for providers
TracePeek doesn't do anything special with provider names. The caller is subject to normal ETW behavior, for example:
- Specifying the provider by GUID (like -Providers 'DBE9B383-7CF3-4331-91CC-A3CB16A3B538') works reliably, but sacrifices readability and puts the burden on the caller to learn the GUID
- Specifying the provider by name (like -Providers 'Microsoft-Windows-Winlogon') is more readable, but works only if that name gets resolved to a GUID at execution time. The two known cases of this are:
  - The provider name is registered with the OS (run *logman.exe providers* for the list of providers registered with the OS) 
  - -OR- The original author of the provider used the Tracelogging/EventSource pattern and did not declare a specific GUID. In this pattern, the effective provider GUID is a function of the provider name, so it can always be resolved. Unfortunately, there is no OS based discovery mechanism in the Tracelogging/EventSource pattern, so the caller would need to learn out-of-band about the provider and its use of this pattern.

### What about the NT kernel provider?
The TraceEvent library can parse NT Kernel events, but given that kernel events tend to be very high volume I have never encountered a scenario where I wanted to use them with TracePeek. If there are cool scenarios for using TracePeek with the NT kernel provider, we could add support for named groups of kernel flags similar to xpert and PerfView.
