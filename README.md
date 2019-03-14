# TracePeek
Interactive view for Event Tracing for Windows (ETW) events live as they happen. Works with PowerShell, C# / .net, or cmdline. 

For quick interactive ETW scenarios TracePeek provides an alternative to the "install-WPT-then-log-and-repro-then-open-ETL-oops-bad-timing-try-logging-again..." pattern. For example:  
* Interactively waiting for some system event to occur 
* Exploring a new feature/provider to learn what events are fired

Each event is fully parsed to support easy interactive use. This means that the per-event CPU cost is much higher than native logging through xpert, et. al. Not recommended for high volume providers.

Powershell example:
```powershell
Import-Module .\TracePeek.psm1

Start-TracePeek -Providers "Microsoft-Windows-Wordpad" | Select-Object -Property ProviderName,Message
```
```
Starting up...
Press ALT+s to stop

## Now launch Wordpad to get this output. Don't forget to enter
## ALT+s in the console to stop and clean up resources.
 
 ProviderName              Message
 ------------              -------
 Microsoft-Windows-Wordpad Intializing current instance of the application
 Microsoft-Windows-Wordpad Wordpad Launch Start.
 Microsoft-Windows-Wordpad Wordpad Launch End.
 Microsoft-Windows-Wordpad Exiting current Instance of the application
Cleaning up...
```

By default TracePeek emits each event as a PSCustomObject where the property names include standard ETW items (e.g. ProviderName, Keywords), plus named properties based on the event payload. Alternative object shapes are available via the  -ProjectionStyle parameter. For example, if you were going to export the results to CSV you might prefer the payload properties to have fixed names like "Field1","Field2", etc. rather than "EventA-HasThisProperty","EventB-HasThisOtherProperty".
```powershell
Start-TracePeek -Providers "Microsoft-Windows-Wordpad" -ProjectionStyle [TracePeek.TracePeekProjectionStyle]::NumberedPayloadProperties
```

For the curious: TracePeek is a thin wrapper around Vance Morrison's wonderful TraceEvent library. Vance's library provides broad coverage of diverse ETW scenarios, while TracePeek optimizes for my narrow use case (interactivity & primarily PowerShell).