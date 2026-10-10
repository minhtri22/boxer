$ErrorActionPreference='Stop'
$bellRoot=Split-Path -Parent $PSScriptRoot
$bellSource=Join-Path $bellRoot 'art/media/ring-girl/source.mp4'
$bellTarget=Join-Path $bellRoot 'unity/BoxerP0/Assets/StreamingAssets/RingMedia/bell.mp3'
$bellEvidence=Join-Path $bellRoot 'evidence/wave1/bell-repair'
if((Get-FileHash -LiteralPath $bellSource -Algorithm SHA256).Hash.ToLowerInvariant() -ne '1dec4970847aa75bc8ef8d2cf518253554486ef8d2ee4460b3f270c9ca3569ac'){throw 'Owner source changed'}
if((Get-FileHash -LiteralPath $bellTarget -Algorithm SHA256).Hash.ToLowerInvariant() -ne '0ecb168ed8e1b7081c6112a3457ddc397b5a6355b03aede94c23a94ba80e8a13'){throw 'Expected silent baseline not present; refuse overwrite'}
$bellBackup=Join-Path $bellEvidence 'bell-silent-before.mp3'
if(Test-Path -LiteralPath $bellBackup){throw 'Backup already exists; do not overwrite evidence'}
New-Item -ItemType Directory -Path $bellEvidence -Force | Out-Null
Copy-Item -LiteralPath $bellTarget -Destination $bellBackup
# Filters run over original timestamps: select the tail, reset to zero, THEN fade locally.
# Output seeking combined with fades evaluated against the full clip can silence the tail.
& ffmpeg -hide_banner -y -i $bellSource -map 0:a:0 -vn -af 'atrim=start=5.15:end=6.00,asetpts=PTS-STARTPTS,afade=t=in:st=0:d=0.01,afade=t=out:st=0.74:d=0.11' -ar 44100 -ac 2 -c:a libmp3lame -b:a 160k $bellTarget
if($LASTEXITCODE -ne 0){throw 'Bell extraction failed; previous asset preserved'}
& node (Join-Path $PSScriptRoot 'audit_bell_signal.cjs')
if($LASTEXITCODE -ne 0){throw 'Decoded signal gate failed; do not build/deploy'}
