param(
    [Parameter(Mandatory=$true)][ValidateSet('Deploy','Status')][string]$Mode,
    [Parameter(Mandatory=$true)][ValidatePattern('^[a-f0-9]{40}$')][string]$ExpectedCommit
)
$ErrorActionPreference='Stop'
$waveRoot=Split-Path -Parent $PSScriptRoot
$waveBranch='feature/boxer-product-loop-wave1'
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
    if((Get-Content (Join-Path $waveRoot 'evidence/wave1/browser/report.json') -Raw | ConvertFrom-Json).status -ne 'PASS'){throw 'Browser gate not PASS'}
    if(-not ((Get-Content (Join-Path $waveRoot 'evidence/wave1/vitals-tests.txt') -Tail 1) -eq 'TOTAL=84 PASS=84 FAIL=0')){throw 'Vitals gate not PASS'}
    if(-not ((Get-Content (Join-Path $waveRoot 'evidence/wave1/controller-runtime.txt') -Tail 1) -eq 'CHECKS=56 EXIT=0')){throw 'Controller gate not PASS'}
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
