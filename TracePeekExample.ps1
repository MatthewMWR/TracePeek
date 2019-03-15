Install-Module TracePeek -Force -ErrorAction Stop

Import-Module TracePeek -Force -ErrorAction Stop

Start-TracePeek -Providers "Microsoft-Windows-Winlogon","Microsoft-Windows-Wordpad"