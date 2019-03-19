$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

Add-Type -Path (Join-Path $psscriptRoot 'TracePeek.dll') -ErrorAction Stop

function New-TracePeekController
{
    param(
        [Parameter(Mandatory=$true)]
        [string]$SessionName
    )
        New-Object -TypeName TracePeek.TracePeekController -ArgumentList $SessionName
}

function Start-TracePeek
{
    [CmdletBinding(PositionalBinding=$true)]
    param(
        [Parameter(Mandatory=$true)]
        [string[]]$Providers,
        [TracePeek.TracePeekProjectionStyle]$ProjectionStyle,
        [string]$SessionName = "TracePeek_DefaultSessionName"
    )
    $tpc = New-TracePeekController -SessionName $SessionName
    
    $providersList = New-Object -TypeName System.Collections.Generic.List`[string]
    foreach($provider in $providers){
        $providersList.Add($provider)
    }
    $tpc.EnableProviders($providersList)
    $eventSubscriberJob = Register-ObjectEvent -InputObject $tpc -EventName OnTracePeekEvent -SourceIdentifier $SessionName
    Write-Host "TracePeek starting up..."
    if($null -eq $ProjectionStyle){$null = $tpc.StartPeek()}
    else{$null = $tpc.StartPeek($ProjectionStyle)}
    Write-Host "TracePeek running."
    Write-Host '!!! Press ALT+s when done to stop session and clean up resources !!!'
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
                Write-Host "TracePeek stopping and cleaning up."
                $consoleCancellationRequested = $true
                $tpc.StopPeek()
            }
        }
    }
    Unregister-Event -SourceIdentifier $SessionName
    if($null -ne $eventSubscriberJob){Remove-Job -Id $eventSubscriberJob.Id -Force}
}