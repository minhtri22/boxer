param(
    [Parameter(Mandatory=$true)][string]$Source,
    [ValidateSet('boxer-favicon-source.png','boxer-favicon-clean.png')]
    [string]$SourceAssetName='boxer-favicon-source.png'
)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$iconRoot=Split-Path -Parent $PSScriptRoot
$iconOutput=Join-Path $iconRoot 'unity/BoxerP0/Assets/WebGLTemplates/BoxerP0Mobile/TemplateData/favicon.ico'
$iconBranding=Join-Path $iconRoot 'art/branding'
New-Item -ItemType Directory -Path $iconBranding -Force | Out-Null
$iconSourceCopy=Join-Path $iconBranding $SourceAssetName
if(Test-Path -LiteralPath $iconSourceCopy){throw 'Preserved source already exists; refusing overwrite'}
if(Test-Path -LiteralPath $iconOutput){throw 'Icon already exists; refusing overwrite'}
Copy-Item -LiteralPath $Source -Destination $iconSourceCopy
$iconImage=[Drawing.Image]::FromFile($iconSourceCopy)
try {
    if($iconImage.Width -ne $iconImage.Height){throw 'Square source required; no implicit crop'}
    $iconSizes=@(16,32,48,64,128,256)
    $iconFrames=@()
    foreach($iconSize in $iconSizes){
        $iconBitmap=New-Object Drawing.Bitmap($iconSize,$iconSize)
        $iconGraphics=[Drawing.Graphics]::FromImage($iconBitmap)
        $iconMemory=New-Object IO.MemoryStream
        try {
            $iconGraphics.CompositingQuality=[Drawing.Drawing2D.CompositingQuality]::HighQuality
            $iconGraphics.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $iconGraphics.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $iconGraphics.DrawImage($iconImage,0,0,$iconSize,$iconSize)
            $iconBitmap.Save($iconMemory,[Drawing.Imaging.ImageFormat]::Png)
            $iconFrames+=,@{Size=$iconSize;Bytes=$iconMemory.ToArray()}
        } finally {$iconMemory.Dispose();$iconGraphics.Dispose();$iconBitmap.Dispose()}
    }
    $iconStream=[IO.File]::Open($iconOutput,[IO.FileMode]::CreateNew)
    $iconWriter=New-Object IO.BinaryWriter($iconStream)
    try {
        $iconWriter.Write([uint16]0);$iconWriter.Write([uint16]1);$iconWriter.Write([uint16]$iconFrames.Count)
        $iconOffset=6+16*$iconFrames.Count
        foreach($iconFrame in $iconFrames){
            $iconDimension=if($iconFrame.Size -eq 256){0}else{$iconFrame.Size}
            $iconWriter.Write([byte]$iconDimension);$iconWriter.Write([byte]$iconDimension)
            $iconWriter.Write([byte]0);$iconWriter.Write([byte]0)
            $iconWriter.Write([uint16]1);$iconWriter.Write([uint16]32)
            $iconWriter.Write([uint32]$iconFrame.Bytes.Length);$iconWriter.Write([uint32]$iconOffset)
            $iconOffset+=$iconFrame.Bytes.Length
        }
        foreach($iconFrame in $iconFrames){$iconWriter.Write([byte[]]$iconFrame.Bytes)}
    } finally {$iconWriter.Dispose();$iconStream.Dispose()}
    $iconPreviewName=if($SourceAssetName -eq 'boxer-favicon-clean.png'){'favicon-clean-preview.png'}else{'favicon-preview.png'}
    $iconPreview=Join-Path $iconRoot ('evidence/wave1/'+$iconPreviewName)
    [IO.File]::WriteAllBytes($iconPreview,[byte[]]$iconFrames[-1].Bytes)
    Write-Output "ICO_SIZES=$($iconSizes -join ',') SOURCE_CONTENT_UNCHANGED"
    Get-FileHash -LiteralPath $iconSourceCopy,$iconOutput -Algorithm SHA256 | Format-Table Path,Hash
} finally {$iconImage.Dispose()}
