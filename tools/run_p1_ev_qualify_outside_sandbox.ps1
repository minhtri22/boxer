[CmdletBinding()]
param(
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

$Repo = 'D:\WORK\RESEARCH\POVGame\boxer-p1-ev-completion'
$UnityExe = 'C:\Program Files\Unity\Hub\Editor\6000.5.8f1\Editor\Unity.exe'
$Project = Join-Path $Repo 'unity\BoxerP0'
$Baseline = 'b0eeed2fcb60006f05956ebca9f211de09608d9d'
$ExpectedBranch = 'p1/ev-completion'
$Timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$Evidence = Join-Path $Repo ("evidence\p1-ev\outside-$Timestamp")
$Handoff = Join-Path $Repo 'handoff.md'
$Stage = 'P0-PREFLIGHT'

$Protected = @(
    'unity/BoxerP0/Assets/Scripts/Round2CombatRig.cs',
    'unity/BoxerP0/Assets/Scripts/Round2Motion.cs',
    'unity/BoxerP0/Assets/Scripts/Round2Footwork.cs',
    'unity/BoxerP0/Assets/Scripts/OpponentBoxer.cs',
    'unity/BoxerP0/Assets/Scripts/PlayerBoxer.cs',
    'unity/BoxerP0/Assets/Scripts/P1PunchMechanics.cs'
)

$AllowedExact = @(
    'tools/run_p1_ev_qualify_outside_sandbox.ps1',
    'unity/BoxerP0/Assets/Editor/Round2Build.cs',
    'unity/BoxerP0/Assets/Scripts/EVVisualShell.cs',
    'unity/BoxerP0/Assets/Scripts/EVReferenceVisuals.cs',
    'unity/BoxerP0/Assets/Scripts/EVReferenceVisuals.cs.meta'
)

function Write-Handoff {
    param([string]$Status,[string]$AtStage,[string]$Message)
    $body = @"
# P1-EV outside-sandbox handoff

Status: ``$Status``

Stage: ``$AtStage``

Timestamp: ``$Timestamp``

Evidence: ``$Evidence``

$Message
"@
    Set-Content -LiteralPath $Handoff -Value $body -Encoding UTF8
}

function Fail-Run {
    param([string]$Message)
    Write-Handoff -Status 'BLOCKED' -AtStage $Stage -Message $Message
    throw "${Stage}: $Message"
}

function Require-Path {
    param([string]$Path,[string]$Description)
    if (-not (Test-Path -LiteralPath $Path)) { Fail-Run "$Description missing: $Path" }
}

function Is-AllowedSourcePath {
    param([string]$Path)
    $p = $Path.Replace('\','/')
    return $AllowedExact -contains $p
}

function Assert-Protected-Clean {
    foreach ($path in $Protected) {
        $changed = @(& git diff --name-only $Baseline -- $path)
        if ($changed.Count -ne 0) { Fail-Run "Protected mechanics changed relative to ${Baseline}: $path" }
    }
}

function Assert-NoUnexpectedSourceChanges {
    $lines = @(& git status --porcelain=v1 --untracked-files=all)
    foreach ($line in $lines) {
        if ($line.Length -lt 4) { continue }
        $path = $line.Substring(3).Trim().Replace('\','/')
        if ($path.Contains(' -> ')) { $path = ($path -split ' -> ')[-1] }
        $sourceArea = $path.StartsWith('unity/BoxerP0/Assets/Scripts/') -or
                      $path.StartsWith('unity/BoxerP0/Assets/Editor/') -or
                      $path.StartsWith('unity/BoxerP0/Assets/Resources/') -or
                      $path.StartsWith('tools/')
        if ($sourceArea -and -not (Is-AllowedSourcePath $path)) {
            Fail-Run "Unexpected source change outside P1-EV whitelist: $path"
        }
    }
}

function Assert-VisualAssetInventory {
    $dir = Join-Path $Repo 'unity\BoxerP0\Assets\Resources\P1V'
    $expected = @(
        'championship-arena.png','championship-arena.png.meta',
        'player-glove-left-chroma.png','player-glove-left-chroma.png.meta',
        'ramirez-guard-chroma.png','ramirez-guard-chroma.png.meta',
        'ramirez-hook-chroma.png','ramirez-hook-chroma.png.meta',
        'ramirez-straight-chroma.png','ramirez-straight-chroma.png.meta'
    ) | Sort-Object
    $actual = @(Get-ChildItem -LiteralPath $dir -File | ForEach-Object Name | Sort-Object)
    if (($expected -join '|') -ne ($actual -join '|')) {
        Fail-Run "P1V reference asset inventory mismatch. Expected $($expected.Count) files, found $($actual.Count)."
    }
}

function Run-Unity {
    param(
        [string]$Method,
        [string]$UnityLog,
        [string]$ProcessLog,
        [bool]$QuitAfterMethod,
        [bool]$UseGraphics = $false
    )
    $args = @('-batchmode')
    if ($UseGraphics) {
        # Runtime visual capture calls Camera.Render(); it requires a real graphics device.
        # Force D3D11 for the Windows qualification path and never combine this with -nographics.
        $args += '-force-d3d11'
    }
    else {
        $args += '-nographics'
    }
    if ($QuitAfterMethod) { $args += '-quit' }
    $args += @('-projectPath',$Project,'-executeMethod',$Method,'-logFile',$UnityLog)
    $oldVariant = $env:BOXER_EV_VARIANT
    $env:BOXER_EV_VARIANT = 'B'
    try {
        & $UnityExe @args 2>&1 | Tee-Object -FilePath $ProcessLog
        $code = $LASTEXITCODE
    }
    finally {
        if ($null -eq $oldVariant) { Remove-Item Env:BOXER_EV_VARIANT -ErrorAction SilentlyContinue }
        else { $env:BOXER_EV_VARIANT = $oldVariant }
    }
    if ($code -ne 0) { Fail-Run "Unity method $Method failed with exit code $code. See $UnityLog" }
}

try {
    Set-Location -LiteralPath $Repo
    New-Item -ItemType Directory -Force -Path $Evidence | Out-Null

    Require-Path $UnityExe 'Unity 6000.5.8f1 executable'
    Require-Path (Join-Path $Repo 'docs\handoff\26-p1-ev-completion-plan.md') 'P1-EV completion plan'
    Require-Path (Join-Path $Project 'Assets\Editor\EVEvaluation.cs') 'EVEvaluation entrypoint'
    Require-Path (Join-Path $Project 'Assets\Editor\Round2RuntimeAudit.cs') 'Runtime audit entrypoint'
    Require-Path (Join-Path $Project 'Assets\Scripts\EVReferenceVisuals.cs') 'Reference visual follower'
    Require-Path (Join-Path $Project 'Assets\Resources\BoxerP1VChromaKey.shader') 'P1V chroma-key shader'
    Require-Path (Join-Path $Project 'Assets\Resources\P1V') 'P1V reference assets'

    $branch = (& git branch --show-current).Trim()
    if ($branch -ne $ExpectedBranch) { Fail-Run "Expected branch $ExpectedBranch, actual branch: $branch" }
    & git cat-file -e "$Baseline^{commit}" 2>$null
    if ($LASTEXITCODE -ne 0) { Fail-Run "Protected baseline unavailable: $Baseline" }

    Assert-Protected-Clean
    Assert-NoUnexpectedSourceChanges
    Assert-VisualAssetInventory

    if ($DryRun) {
        Write-Handoff -Status 'DRY_RUN_PASS' -AtStage $Stage -Message 'Preflight passed. No Unity execution was performed.'
        Write-Output "DRY_RUN_PASS branch=$branch baseline=$Baseline evidence=$Evidence"
        exit 0
    }

    $Stage = 'P1-P4-RUNTIME-VISUAL'
    Run-Unity -Method 'BoxerP0.Editor.EVEvaluation.RunVisual' `
        -UnityLog (Join-Path $Evidence 'p1-p4-unity-runtime.log') `
        -ProcessLog (Join-Path $Evidence 'p1-p4-process.log') `
        -QuitAfterMethod $false `
        -UseGraphics $true

    $ownership = Join-Path $Repo 'evidence\p1-ev\ownership-runtime.txt'
    $runtime = Join-Path $Repo 'evidence\uat-round2\optimization\runtime.txt'
    Require-Path $ownership 'Ownership runtime evidence'
    Require-Path $runtime 'Runtime invariant evidence'
    if (-not (Select-String -LiteralPath $ownership -SimpleMatch 'RESULT=PASS' -Quiet)) { Fail-Run 'Ownership runtime did not PASS' }
    if (-not (Select-String -LiteralPath $runtime -SimpleMatch 'RUNTIME_INVARIANTS=PASS' -Quiet)) { Fail-Run 'Runtime invariants did not PASS' }
    Copy-Item -LiteralPath $ownership -Destination (Join-Path $Evidence 'ownership-runtime.txt')
    Copy-Item -LiteralPath $runtime -Destination (Join-Path $Evidence 'runtime.txt')

    $motionSource = Join-Path $Repo 'evidence\uat-round2\optimization'
    $shots = @(Get-ChildItem -LiteralPath $motionSource -Filter 'motion-??.png' -File | Sort-Object Name)
    if ($shots.Count -ne 10) { Fail-Run "Expected exactly 10 motion captures, found $($shots.Count)" }
    $motionTarget = Join-Path $Evidence 'motion'
    New-Item -ItemType Directory -Force -Path $motionTarget | Out-Null
    foreach ($shot in $shots) { Copy-Item -LiteralPath $shot.FullName -Destination (Join-Path $motionTarget $shot.Name) }

    $Stage = 'P2-P3-DETERMINISTIC'
    Run-Unity -Method 'BoxerP0.Editor.EVEvaluation.Tests' `
        -UnityLog (Join-Path $Evidence 'p2-p3-unity-tests.log') `
        -ProcessLog (Join-Path $Evidence 'p2-p3-process.log') `
        -QuitAfterMethod $true `
        -UseGraphics $false

    $geometry = Join-Path $Repo 'evidence\p1-ev\geometry-tests.txt'
    Require-Path $geometry 'EV geometry evidence'
    if (-not (Select-String -LiteralPath $geometry -SimpleMatch 'TOTAL=6 FAIL=0' -Quiet)) { Fail-Run 'EV geometry suite is not 6/6 PASS' }
    Copy-Item -LiteralPath $geometry -Destination (Join-Path $Evidence 'geometry-tests.txt')
    Assert-Protected-Clean

    $Stage = 'P4-VISUAL-REVIEW-GATE'
    $report = @"
# P1-EV outside-sandbox qualification

Status: AWAITING_VISUAL_REVIEW

Branch: $branch
Baseline: $Baseline
Unity: 6000.5.8f1

- ownership runtime: PASS
- runtime invariants: PASS
- EV geometry: 6/6 PASS
- protected mechanics: CLEAN
- motion captures: 10 x 900x1600

Review required: ``$motionTarget``
"@
    Set-Content -LiteralPath (Join-Path $Evidence 'qualification-report.md') -Value $report -Encoding UTF8
    Write-Handoff -Status 'AWAITING_VISUAL_REVIEW' -AtStage $Stage -Message "Qualification passed. Review the 10 runtime captures in $motionTarget before P6-P9 finalization."
    Write-Output "QUALIFICATION_PASS_AWAITING_VISUAL_REVIEW evidence=$Evidence"
}
catch {
    $message = $_.Exception.Message
    Write-Handoff -Status 'BLOCKED' -AtStage $Stage -Message $message
    Write-Error $message
    exit 1
}
