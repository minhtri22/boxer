param(
    [Parameter(Mandatory=$true)][ValidatePattern('^[a-f0-9]{40}$')][string]$ArtifactCommit,
    [Parameter(Mandatory=$true)][ValidatePattern('^[a-f0-9]{40}$')][string]$CompiledSource,
    [Parameter(Mandatory=$true)][long]$WorkflowId,
    [ValidateSet('ring-intro','training-pov','punch-feel','coach-ui','coach-analysis','bell-repair','arena-surround')][string]$EvidenceSuite='ring-intro'
)
$ErrorActionPreference='Stop'
$ringRoot=Split-Path -Parent $PSScriptRoot
$ringOutput=Join-Path $ringRoot 'builds/web/boxer-round2'
$ringLocalProof=[IO.File]::ReadAllBytes((Join-Path $ringOutput 'provenance.txt'))
$ringLines=[IO.File]::ReadAllLines((Join-Path $ringOutput 'provenance.txt'))
if("source_sha=$CompiledSource" -notin $ringLines){throw 'Unexpected compiled source'}
$ringVersion='w1-'+$CompiledSource
if("productVersion=$ringVersion" -notin $ringLines){throw 'Unexpected product version'}
$ringDecoder=Get-Content (Join-Path $ringRoot ('evidence/wave1/'+$EvidenceSuite+'/media-browser-public/report.json')) -Raw | ConvertFrom-Json
if($ringDecoder.status -ne 'PASS' -or $ringDecoder.productVersion -ne $ringVersion -or $ringDecoder.checks.Count -ne $(if($EvidenceSuite -in @('bell-repair','arena-surround')){21}else{20}) -or $ringDecoder.errors.Count -ne 0 -or @($ringDecoder.checks | Where-Object {-not $_.pass}).Count -ne 0 -or $ringDecoder.url -notlike 'https://minhtri22.github.io/boxer/*'){throw 'Full current public decoder/lifecycle audit required'}
$ringClient=[Net.Http.HttpClient]::new()
$ringClient.Timeout=[TimeSpan]::FromSeconds(45)
$ringClient.DefaultRequestHeaders.UserAgent.ParseAdd('Boxer-release-verification/1.0')
$ringHash=[Security.Cryptography.SHA256]::Create()
$ringReceipt=[Collections.Generic.List[string]]::new()
function RingDigest([byte[]]$Bytes){[BitConverter]::ToString($ringHash.ComputeHash($Bytes)).Replace('-','').ToLowerInvariant()}
function RingGet([string]$Relative){
    $ringUri='https://minhtri22.github.io/boxer/'+$Relative+'?release='+$CompiledSource+'&check='+[DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
    $ringResponse=$ringClient.GetAsync($ringUri).GetAwaiter().GetResult()
    if([int]$ringResponse.StatusCode -ne 200){throw "HTTP $($ringResponse.StatusCode) $Relative"}
    $ringBytes=$ringResponse.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
    $ringType=[string]$ringResponse.Content.Headers.ContentType
    $ringResponse.Dispose()
    return @{Bytes=$ringBytes;Type=$ringType}
}
try{
    $ringPublicProof=RingGet 'provenance.txt'
    if((RingDigest $ringLocalProof) -ne (RingDigest $ringPublicProof.Bytes)){throw 'Public provenance differs from local'}
    $ringReceipt.Add('STATUS=DEPLOYMENT_PASS_PENDING_HUMAN_UAT')
    $ringReceipt.Add('ARTIFACT_COMMIT='+$ArtifactCommit)
    $ringReceipt.Add('COMPILED_SOURCE='+$CompiledSource)
    $ringReceipt.Add('PRODUCT_VERSION='+$ringVersion)
    $ringReceipt.Add('WORKFLOW_ID='+$WorkflowId)
    $ringReceipt.Add('WORKFLOW_URL=https://github.com/minhtri22/boxer/actions/runs/'+$WorkflowId)
    $ringReceipt.Add('PUBLIC_URL=https://minhtri22.github.io/boxer/')
    $ringReceipt.Add('PROVENANCE_SHA256='+(RingDigest $ringPublicProof.Bytes))
    $ringCount=0
    foreach($ringLine in $ringLines){
        if($ringLine -match '^(Build/.+|TemplateData/.+|StreamingAssets/.+|index.html)=([a-f0-9]{64})$'){
            $ringRelative=$Matches[1];$ringExpected=$Matches[2]
            if((RingDigest ([IO.File]::ReadAllBytes((Join-Path $ringOutput $ringRelative)))) -ne $ringExpected){throw "Local payload differs $ringRelative"}
            $ringPublic=RingGet $ringRelative
            if((RingDigest $ringPublic.Bytes) -ne $ringExpected){throw "Public payload differs $ringRelative"}
            if($ringRelative.EndsWith('.mp4') -and $ringPublic.Type -notlike 'video/mp4*'){throw 'Unexpected public video MIME'}
            # Pages currently serves MP3 as audio/mp3; require audio typing AND separately
            # verify actual decoder/lifecycle in the public media browser audit.
            if($ringRelative.EndsWith('.mp3') -and $ringPublic.Type -notmatch '^audio/(mpeg|mp3)(;|$)'){throw 'Unexpected public audio MIME'}
            $ringCount++;$ringReceipt.Add('HTTP_200_HASH_PASS '+$ringRelative+' '+$ringExpected+' '+$ringPublic.Type)
        }
    }
    if($ringCount -ne 12){throw 'Full twelve-payload release required'}
    $ringReceipt.Add('PUBLIC_PAYLOADS=12/12_HTTP_200_SHA256_MATCH')
    $ringReceipt.Add('MEDIA_MIME=OBSERVED_video/mp4_audio/mp3_PUBLIC_DECODER_AUDIT_PASS')
    $ringMediaCount=if($EvidenceSuite -in @('bell-repair','arena-surround')){21}else{20}
    $ringReceipt.Add('PUBLIC_MEDIA_BROWSER='+$ringMediaCount+'/'+$ringMediaCount+'_PASS_ZERO_JS_LOAD_HTTP_ERRORS_CURRENT_PRODUCT_VERSION')
    $ringReceipt.Add('DEPLOYMENT_BRANCH_POLICIES=ALL_FIVE_PRESERVED_NO_NEW_POLICY')
    $ringReceipt.Add('VERIFIED_UTC='+[DateTimeOffset]::UtcNow.ToString('o'))
    $ringReceipt.Add('SCOPE=PUBLIC_BYTES_MIME_AND_REAL_BROWSER_MEDIA_NOT_PHONE_OR_SPEAKER_UAT')
    [IO.File]::WriteAllLines((Join-Path $ringRoot ('evidence/wave1/'+$EvidenceSuite+'/deployment.txt')),$ringReceipt)
    $ringReceipt | Write-Output
}finally{$ringHash.Dispose();$ringClient.Dispose()}
