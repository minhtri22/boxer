param([string]$Baseline='10123ff984a4aa24b19dbe4aeda7aff3d7d01c3d',[ValidateSet('coach-ui','coach-analysis','bell-repair')][string]$EvidenceSuite='coach-ui')
$ErrorActionPreference='Stop'
$coachRoot=Split-Path -Parent $PSScriptRoot
$coachChecks=[Collections.Generic.List[string]]::new()
$coachFiles=@('Round2CombatRig.cs','Round2Motion.cs','PunchMotionProfile.cs','PlayerBoxer.cs','OpponentBoxer.cs','BoxerInput.cs','BoxerFeedback.cs','Phase0Telemetry.cs','P1PunchMechanics.cs','P1OpponentAttributes.cs')
foreach($coachFile in $coachFiles){
    $coachRelative='unity/BoxerP0/Assets/Scripts/'+$coachFile
    $coachPrevious=[string]::Join("`n",(git show ($Baseline+':'+$coachRelative)))
    if($LASTEXITCODE -ne 0){throw 'Missing held baseline source'}
    $coachCurrent=[IO.File]::ReadAllText((Join-Path $coachRoot $coachRelative)).Replace("`r`n","`n")
    if($coachPrevious.TrimEnd() -cne $coachCurrent.TrimEnd()){throw ('Held combat file changed: '+$coachFile)}
    $coachChecks.Add('UNCHANGED '+$coachRelative)
}
$coachBoutPath='unity/BoxerP0/Assets/Scripts/CombatBout.cs'
$coachPrevious=[regex]::Split([string]::Join("`n",(git show ($Baseline+':'+$coachBoutPath))),'(?m)^    public enum ProductScreen')[0]
$coachCurrent=[regex]::Split([IO.File]::ReadAllText((Join-Path $coachRoot $coachBoutPath)).Replace("`r`n","`n"),'(?m)^    public enum ProductScreen')[0]
if($coachPrevious.Length -lt 4000 -or $coachPrevious -cne $coachCurrent){throw 'Authoritative vitals / HP / KO model changed'}
$coachChecks.Add('UNCHANGED CombatBout.cs_authoritative_model_prefix_length='+$coachCurrent.Length)
$coachBootstrap=[IO.File]::ReadAllText((Join-Path $coachRoot 'unity/BoxerP0/Assets/Scripts/BoxerBootstrap.cs'))
if($coachBootstrap -notmatch 'private const float BoutSeconds = 45f;'){throw 'Bout duration changed'}
$coachChecks.Add('UNCHANGED Bootstrap_45_second_duration')
$coachChecks.Insert(0,'STATUS=COMBAT_HOLD_PRESERVED')
$coachChecks.Insert(1,'BASELINE='+$Baseline)
$coachChecks.Add('CHECKS=12 PASS=12 FAIL=0')
$coachOutput=Join-Path $coachRoot ('evidence/wave1/'+$EvidenceSuite+'/combat-freeze.txt')
[IO.File]::WriteAllLines($coachOutput,$coachChecks,[Text.UTF8Encoding]::new($false))
$coachChecks | Write-Output
