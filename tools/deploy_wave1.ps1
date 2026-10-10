param(
    [Parameter(Mandatory=$true)][ValidateSet('Deploy','Status')][string]$Mode,
    [Parameter(Mandatory=$true)][ValidatePattern('^[a-f0-9]{40}$')][string]$ExpectedCommit,
    [ValidatePattern('^wave1(/[a-z0-9-]+)?$')][string]$EvidenceSubdirectory='wave1',
    [ValidateRange(1,10000)][int]$ExpectedVitalsChecks=84,
    [ValidateRange(1,10000)][int]$ExpectedControllerChecks=56
)
$ErrorActionPreference='Stop'
$waveRoot=Split-Path -Parent $PSScriptRoot
$waveBranch='feature/boxer-product-loop-wave1'
$waveGateDirectory=Join-Path $waveRoot ('evidence/'+$EvidenceSubdirectory)
$waveApi='https://api.github.com/repos/minhtri22/boxer'
# Credential remains in process memory; never emit credential helper output or headers.
$waveLines="protocol=https`nhost=github.com`n`n" | git credential fill
$waveCredentials=@{}
foreach($waveLine in $waveLines){$wavePair=$waveLine -split '=',2;if($wavePair.Count -eq 2){$waveCredentials[$wavePair[0]]=$wavePair[1]}}
if(-not $waveCredentials['password']){throw 'GitHub credential unavailable'}
$waveHeaders=@{Authorization='Bearer '+$waveCredentials['password'];Accept='application/vnd.github+json';'X-GitHub-Api-Version'='2022-11-28'}
function WaveApi([string]$Url,[string]$Method='Get',$Body=$null){
    $waveParams=@{Uri=$Url;Headers=$waveHeaders;Method=$Method}
    if($null -ne $Body){$waveParams.Body=ConvertTo-Json $Body -Depth 10 -Compress;$waveParams.ContentType='application/json'}
    Invoke-RestMethod @waveParams
}
$waveRemote=WaveApi "$waveApi/git/ref/heads/$waveBranch"
if($waveRemote.object.sha -ne $ExpectedCommit){throw 'Remote branch SHA mismatch'}
if($Mode -eq 'Deploy'){
    $waveBrowserGate=Get-Content (Join-Path $waveGateDirectory 'browser/report.json') -Raw | ConvertFrom-Json
    if($waveBrowserGate.status -ne 'PASS' -or $waveBrowserGate.errors.Count -ne 0){throw 'Browser gate not PASS'}
    $waveLocalProof=Get-Content (Join-Path $waveRoot 'builds/web/boxer-round2/provenance.txt')
    $waveProductVersion=($waveLocalProof | Where-Object {$_ -like 'productVersion=*'}) -replace '^productVersion=',''
    if($waveProductVersion -ne $waveBrowserGate.productVersion){throw 'Stale browser report: compiled product version differs'}
    if($EvidenceSubdirectory -in @('wave1/combat-v3','wave1/ring-intro','wave1/training-pov','wave1/punch-feel','wave1/coach-ui','wave1/coach-analysis','wave1/bell-repair','wave1/arena-surround')){
        $waveContactGate=Get-Content (Join-Path $waveGateDirectory 'combat-browser/report.json') -Raw | ConvertFrom-Json
        if($waveContactGate.status -ne 'PASS' -or $waveContactGate.errors.Count -ne 0 -or $waveContactGate.productVersion -ne $waveProductVersion){throw 'Physical input/contact browser gate not PASS or stale'}
        $waveContactChecks=if($EvidenceSubdirectory -in @('wave1/ring-intro','wave1/training-pov','wave1/punch-feel','wave1/coach-ui','wave1/coach-analysis','wave1/bell-repair','wave1/arena-surround')){18}else{17}
        if($waveContactGate.checks.Count -ne $waveContactChecks -or @($waveContactGate.checks | Where-Object {-not $_.pass}).Count -ne 0){throw "Full $waveContactChecks-check contact suite required; tactical probe is not a release gate"}
        $waveCombatTail=Get-Content (Join-Path $waveGateDirectory 'combat-tests.txt') -Tail 1
        if($waveCombatTail -notmatch '^TOTAL=(\d+) PASS=(\d+) FAIL=0$' -or $Matches[1] -ne $Matches[2]){throw 'Combat geometry invariant gate not PASS'}
    }
    if($EvidenceSubdirectory -in @('wave1/ring-intro','wave1/training-pov','wave1/punch-feel','wave1/coach-ui','wave1/coach-analysis','wave1/bell-repair','wave1/arena-surround')){
        $waveMediaGate=Get-Content (Join-Path $waveGateDirectory 'media-browser/report.json') -Raw | ConvertFrom-Json
        if($waveMediaGate.status -ne 'PASS' -or $waveMediaGate.errors.Count -ne 0 -or $waveMediaGate.productVersion -ne $waveProductVersion -or $waveMediaGate.checks.Count -ne $(if($EvidenceSubdirectory -in @('wave1/bell-repair','wave1/arena-surround')){21}else{20}) -or @($waveMediaGate.checks | Where-Object {-not $_.pass}).Count -ne 0){throw 'Current compiled full 20-check media lifecycle gate not PASS'}
    }
    if(-not ((Get-Content (Join-Path $waveGateDirectory 'vitals-tests.txt') -Tail 1) -eq "TOTAL=$ExpectedVitalsChecks PASS=$ExpectedVitalsChecks FAIL=0")){throw 'Vitals gate not PASS'}
    if(-not ((Get-Content (Join-Path $waveGateDirectory 'controller-runtime.txt') -Tail 1) -eq "CHECKS=$ExpectedControllerChecks EXIT=0")){throw 'Controller gate not PASS'}
    if($EvidenceSubdirectory -in @('wave1/training-pov','wave1/punch-feel','wave1/coach-ui','wave1/coach-analysis','wave1/bell-repair','wave1/arena-surround')){
        if($waveBrowserGate.checks.Count -ne 33 -or @($waveBrowserGate.checks | Where-Object {-not $_.pass}).Count -ne 0){throw 'Full training and POV browser suite required'}
        if(-not ((Get-Content (Join-Path $waveGateDirectory 'presentation-tests.txt') -Tail 1) -eq 'TOTAL=550 PASS=550 FAIL=0')){throw 'POV presentation geometry gate not PASS'}
    }
    if($EvidenceSubdirectory -eq 'wave1/punch-feel'){
        $feelTail=Get-Content (Join-Path $waveGateDirectory 'punch-feel-tests.txt') -Tail 1
        if($feelTail -notmatch '^TOTAL=(\d+) PASS=(\d+) FAIL=0$' -or $Matches[1] -ne $Matches[2]){throw 'Punch timeline/profile/impact gate not PASS'}
        $feelGate=Get-Content (Join-Path $waveGateDirectory 'feel-browser/report.json') -Raw | ConvertFrom-Json
        if($feelGate.status -ne 'PASS' -or $feelGate.errors.Count -ne 0 -or $feelGate.productVersion -ne $waveProductVersion -or @($feelGate.checks | Where-Object {-not $_.pass}).Count -ne 0){throw 'Current compiled punch-feel interaction gate not PASS'}
    }
    if($EvidenceSubdirectory -in @('wave1/coach-ui','wave1/coach-analysis','wave1/bell-repair','wave1/arena-surround')){
        $coachSuite=if($EvidenceSubdirectory -eq 'wave1/arena-surround'){'arena-surround'}elseif($EvidenceSubdirectory -eq 'wave1/bell-repair'){'bell-repair'}elseif($EvidenceSubdirectory -eq 'wave1/coach-analysis'){'coach-analysis'}else{'coach-ui'}
        & (Join-Path $PSScriptRoot 'check_coach_combat_freeze.ps1') -EvidenceSuite $coachSuite
        if((Get-Content (Join-Path $waveGateDirectory 'coach-tests.txt') -Tail 1) -ne 'TOTAL=53 PASS=53 FAIL=0'){throw 'Full Coach model gate required'}
        if((Get-Content (Join-Path $waveGateDirectory 'punch-feel-tests.txt') -Tail 1) -ne 'TOTAL=14741 PASS=14741 FAIL=0'){throw 'Existing punch motion/timeline regression required'}
        $coachGate=Get-Content (Join-Path $waveGateDirectory 'coach-browser/report.json') -Raw | ConvertFrom-Json
        $coachCount=if($coachSuite -in @('coach-analysis','bell-repair','arena-surround')){52}else{31}
        if($coachGate.status -ne 'PASS' -or $coachGate.productVersion -ne $waveProductVersion -or $coachGate.errors.Count -ne 0 -or $coachGate.checks.Count -ne $coachCount -or @($coachGate.checks | Where-Object {-not $_.pass}).Count -ne 0){throw "Current compiled full $coachCount-check Coach browser gate required"}
        if($coachSuite -in @('coach-analysis','bell-repair','arena-surround') -and (Get-Content (Join-Path $waveGateDirectory 'analysis-tests.txt') -Tail 1) -ne 'TOTAL=56 PASS=56 FAIL=0'){throw 'Full completed-record and analysis model gate required'}
        if($coachSuite -eq 'bell-repair'){
            $bellSignalGate=Get-Content (Join-Path $waveGateDirectory 'bell-signal.json') -Raw | ConvertFrom-Json
            $bellFile=Join-Path $waveRoot 'unity/BoxerP0/Assets/StreamingAssets/RingMedia/bell.mp3'
            if($bellSignalGate.status -ne 'PASS' -or $bellSignalGate.checks.Count -ne 6 -or @($bellSignalGate.checks | Where-Object {-not $_.pass}).Count -ne 0 -or $bellSignalGate.after.sha256 -ne (Get-FileHash -LiteralPath $bellFile).Hash.ToLowerInvariant()){throw 'Actual decoded bell signal gate not PASS or stale'}
        }
    }
    if($EvidenceSubdirectory -eq 'wave1/arena-surround'){
        if((Get-Content (Join-Path $waveGateDirectory 'arena-tests.txt') -Tail 1) -ne 'TOTAL=3700 PASS=3700 FAIL=0'){throw 'Full bounded arena invariant gate required'}
        $arenaGate=Get-Content (Join-Path $waveGateDirectory 'arena-browser/report.json') -Raw | ConvertFrom-Json
        if($arenaGate.status -ne 'PASS' -or $arenaGate.productVersion -ne $waveProductVersion -or $arenaGate.errors.Count -ne 0 -or $arenaGate.checks.Count -ne 12 -or @($arenaGate.checks | Where-Object {-not $_.pass}).Count -ne 0){throw 'Current compiled full arena real-UI gate required'}
    }
    $wavePolicies=WaveApi "$waveApi/environments/github-pages/deployment-branch-policies"
    $waveBefore=@($wavePolicies.branch_policies | ForEach-Object { $_.name+'|'+$_.type })
    if(-not ($wavePolicies.branch_policies | Where-Object {$_.name -eq $waveBranch -and $_.type -eq 'branch'})){
        # Owner explicitly approved this exact additive policy on 2026-10-08.
        $null=WaveApi "$waveApi/environments/github-pages/deployment-branch-policies" 'Post' @{name=$waveBranch;type='branch'}
    }
    $waveAfter=WaveApi "$waveApi/environments/github-pages/deployment-branch-policies"
    foreach($waveOriginal in $waveBefore){if($waveOriginal -notin @($waveAfter.branch_policies | ForEach-Object {$_.name+'|'+$_.type})){throw 'Original deployment policy missing'}}
    $waveAfter.branch_policies | Select-Object name,type | ConvertTo-Json | Write-Output
    $null=WaveApi "$waveApi/actions/workflows/p0-web-deploy.yml/dispatches" 'Post' @{ref=$waveBranch}
    Write-Output 'WORKFLOW_DISPATCHED'
}
$waveRuns=WaveApi ("$waveApi/actions/workflows/p0-web-deploy.yml/runs?branch="+[Uri]::EscapeDataString($waveBranch)+'&per_page=5')
$waveRuns.workflow_runs | Where-Object head_sha -eq $ExpectedCommit | Select-Object id,status,conclusion,head_sha,html_url | ConvertTo-Json | Write-Output
