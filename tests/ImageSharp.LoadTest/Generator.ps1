$perc = @(10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 70, 80,  90, 90, 100, 110, 120, 130, 140, 150, 160, 170)

$inDir = "..\Images\External\LoadTestInput"
$outDir = ".\GeneratedInput"

if (!(Test-Path $outDir)) {
    New-Item -ItemType directory -Path $outDir
}

Get-ChildItem $inDir | ForEach-Object {
    #magick identify $_.FullName
    $inputFileFull = $_.FullName
    $inputFileShort = [System.IO.Path]::GetFileNameWithoutExtension($_.Name)

    $dimsStr = (magick identify -format "%wx%h" $inputFileFull) | Out-String
    $a = $dimsStr.Split('x')
    $origMegaPixels = [double]$a[0]*[double]$a[1] / (1024*1024)
    
    Write-Host "$dimsStr MP=$origMegaPixels"

    $perc | ForEach-Object {
        $p = $_
        $scale = $p * 0.01
        $mp = $origMegaPixels * $scale * $scale
        #$mp = [math]::Round($mp,3)
        #if ($mp -lt 42){
        if (($mp -gt 0.25) -and ($mp -lt 80)) {
            $mpStr = $mp.ToString("00.000")
            $outFileName = "$outDir\MP-$mpStr-$inputFileShort.jpg"
            magick convert $inputFileFull -resize "$p%" $outFileName
            Write-Host $outFileName   
        }
    }
}

Get-ChildItem $outDir | ForEach-Object {
    magick identify $_.FullName
}
