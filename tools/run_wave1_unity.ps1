param(
    [Parameter(Mandatory=$true)][string]$Method,
    [Parameter(Mandatory=$true)][string]$LogName,
    [switch]$Quit,
    [switch]$NoGraphics,
    [int]$TimeoutSeconds = 1800
)
$ErrorActionPreference = 'Stop'
$waveRoot = Split-Path -Parent $PSScriptRoot
$waveProject = Join-Path $waveRoot 'unity/BoxerP0'
$waveEvidence = Join-Path $waveRoot 'evidence/wave1'
New-Item -ItemType Directory -Path $waveEvidence -Force | Out-Null
$waveLog = Join-Path $waveEvidence $LogName
$waveUnityExe = 'C:\Program Files\Unity\Hub\Editor\6000.5.8f1\Editor\Unity.exe'
$waveRunnerExe = 'C:\Program Files\Unity\Hub\Editor\6000.5.8f1\Editor\Data\Tools\BuildPipeline\Compilation\Unity.ILPP.Runner\Unity.ILPP.Runner.exe'
$waveArgs = @('-batchmode','-noUpm','-projectPath',$waveProject,'-executeMethod',$Method,'-logFile',$waveLog)
if ($Quit) { $waveArgs += '-quit' }
if ($NoGraphics) { $waveArgs += '-nographics' } else { $waveArgs += '-force-d3d11' }
$waveProcess = Start-Process -FilePath $waveUnityExe -WindowStyle Hidden -ArgumentList $waveArgs -PassThru
Write-Output "UNITY_PID=$($waveProcess.Id) METHOD=$Method"
$waveStopAt = (Get-Date).AddSeconds($TimeoutSeconds)
$waveHelpers = @()
$waveReplaced = @{}
$waveCpuWatch = @{}
try {
    while (-not $waveProcess.HasExited) {
        if ((Get-Date) -gt $waveStopAt) { throw 'Unity task timeout; no PASS inferred' }
        $waveChildren = Get-CimInstance Win32_Process -Filter "ParentProcessId=$($waveProcess.Id)" |
            Where-Object { $_.Name -eq 'Unity.ILPP.Runner.exe' }
        foreach ($waveChild in $waveChildren) {
            if ($waveReplaced.ContainsKey($waveChild.ProcessId)) { continue }
            $waveChildProcess = Get-Process -Id $waveChild.ProcessId -ErrorAction SilentlyContinue
            if (-not $waveChildProcess) { continue }
            $waveCpu = $waveChildProcess.CPU
            $waveWatch = $waveCpuWatch[$waveChild.ProcessId]
            if (-not $waveWatch -or $waveWatch.Cpu -ne $waveCpu) {
                $waveCpuWatch[$waveChild.ProcessId] = @{Cpu=$waveCpu;Since=Get-Date}
                continue
            }
            if (((Get-Date)-$waveWatch.Since).TotalSeconds -lt 20) { continue }
            # Do not restart an idle but healthy compiler. Require the editor's IPC retry evidence.
            if (-not (Test-Path -LiteralPath $waveLog) -or
                -not ((Get-Content -LiteralPath $waveLog -Tail 3) -match 'Connectivity with IL Post Processor runner cannot be established')) { continue }
            $waveMatch = [regex]::Match($waveChild.CommandLine, '"(unity-ilpp-[a-f0-9]+)"')
            if (-not $waveMatch.Success) { continue }
            $waveOwner = Get-CimInstance Win32_Process -Filter "ProcessId=$($waveProcess.Id)"
            if ($waveOwner.CommandLine -notlike "*$waveProject*") { throw 'Unexpected editor ownership; refusing process termination' }
            if ($waveReplaced.Count -ge 8) { throw 'Repeated ILPP startup failure; no further restarts' }
            $waveReplaced[$waveChild.ProcessId] = $true
            Stop-Process -Id $waveChild.ProcessId
            $waveNumber = $waveReplaced.Count
            $waveHelper = Start-Process -FilePath $waveRunnerExe -WindowStyle Hidden -WorkingDirectory $waveProject `
                -ArgumentList @($waveMatch.Groups[1].Value,'-name','ILPP') `
                -RedirectStandardOutput (Join-Path $waveEvidence "$LogName.ilpp-$waveNumber.log") `
                -RedirectStandardError (Join-Path $waveEvidence "$LogName.ilpp-$waveNumber.err") -PassThru
            $waveHelpers += $waveHelper
            Write-Output "SCOPED_ILPP_RESTART=$waveNumber PID=$($waveHelper.Id)"
        }
        Start-Sleep -Seconds 2
        $waveProcess.Refresh()
    }
    $waveProcess.WaitForExit()
    Write-Output "UNITY_EXIT=$($waveProcess.ExitCode)"
    exit $waveProcess.ExitCode
} finally {
    if (-not $waveProcess.HasExited) {
        $waveOwner = Get-CimInstance Win32_Process -Filter "ProcessId=$($waveProcess.Id)" -ErrorAction SilentlyContinue
        if ($waveOwner.CommandLine -like "*$waveProject*") { Stop-Process -Id $waveProcess.Id -ErrorAction SilentlyContinue }
    }
    foreach ($waveHelper in $waveHelpers) {
        if (-not $waveHelper.HasExited) { Stop-Process -Id $waveHelper.Id -ErrorAction SilentlyContinue }
    }
}
