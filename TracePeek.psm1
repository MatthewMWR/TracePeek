$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

Add-Type -Path .\TracePeek.dll -ErrorAction Stop

function New-TracePeekController
{
    param($SessionName = "TracePeekDefaultSessionName")
    New-Object -TypeName TracePeek.TracePeekController -ArgumentList $SessionName
}

function Start-TracePeek
{
    [CmdletBinding()]
    param(
        $SessionName = "TracePeekDefaultSessionName",
        [string[]]$Providers,
        [TracePeek.TracePeekProjectionStyle]$ProjectionStyle
    )
    $tpc = New-TracePeekController -SessionName $SessionName
    
    $providersList = New-Object -TypeName System.Collections.Generic.List`[string]
    foreach($provider in $providers){
        $providersList.Add($provider)
    }
    $tpc.EnableProviders($providersList)
    $eventSubscriberJob = Register-ObjectEvent -InputObject $tpc -EventName OnTracePeekEvent -SourceIdentifier $SessionName #-Action {$args}
    ##[TracePeek.Utility]::HandleConsoleCancelKeyPress($tpc)
    Write-Host "Starting up..."
    Write-Host "Press ALT+s to stop"
    if($null -eq $ProjectionStyle){$tpc.StartPeek()}
    else{$tpc.StartPeek($ProjectionStyle)}
    $null = $tpc.StartPeek()
    $consoleCancellationRequested = $false
    while($consoleCancellationRequested -eq $false)
    {
        Wait-Event -SourceIdentifier $SessionName -Timeout 1 -ErrorAction SilentlyContinue |
        ForEach-Object{ Write-Output $_.SourceArgs[0] ; Remove-Event -EventIdentifier $_.EventIdentifier}
        while([Console]::KeyAvailable)
        {
            $keyPress = [Console]::ReadKey($true)
            if ($keyPress.Modifiers.HasFlag([System.ConsoleModifiers]::Alt) -and $keyPress.KeyChar -eq 's')
            {
                Write-Host "Cleaning up..."
                $consoleCancellationRequested = $true
                $tpc.StopPeek()
            }
        }
    }

    Unregister-Event -SourceIdentifier $SessionName
    if($null -ne $eventSubscriberJob){Remove-Job -Id $eventSubscriberJob.Id -Force}
}

<# 
function New-TracePeekEventListeningDescriptor
{
    param([string[]]$ProviderName)
    foreach($provider in $ProviderName)
    {
        $descriptor = New-Object -TypeName TracePeek.EventListeningDescriptor
        $descriptor.ProviderName = $provider
        $descriptor
    }
} #>