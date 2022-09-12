$ConfigFile = Get-Content "$PSScriptRoot/config"

$Config = @{}

ForEach($Line in $ConfigFile)
{
If ($Line -ne "")
{
$SplitArray = $Line.Split("=")
$Config += @{$SplitArray[0].Trim() = $SplitArray[1].Trim()}
}
}