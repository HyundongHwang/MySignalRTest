Write-Debug "configuration ..."
$cname = "my company"
$httpHandlerAppID = "12345678-db90-4b66-8b01-88f7af2e36bf"
$sslPort = 8080
pause



Write-Debug "delete certificate in machine ..."
ls cert:\ -Recurse | where { $_.Subject -like "*$cname*" } | rm
pause



Write-Debug "delete cer file ..."
rm *.cer
rm *.pvk
pause



Write-Debug "create new certificate ..."
makecert -n "CN=$cname" my.cer -sr localmachine -ss my
pause



Write-Debug "check new certificate ..."
ls cert:\ -Recurse | where { $_.Subject -like "*$cname*" }
pause



Write-Debug "get certificate hash ..."
$hash = (ls cert:\ -Recurse | where { $_.Subject -like "*$cname*" } | select -First 1).Thumbprint
$hash
pause



Write-Debug "delete ssl binding ..."
netsh http delete sslcert ipport=0.0.0.0:$sslPort
pause



Write-Debug "add new binding ..."
$cmd = "http add sslcert ipport=0.0.0.0:$sslPort certhash=$hash appid={$httpHandlerAppID}"
$cmd
$cmd | netsh
pause



Write-Debug "check new binding ..."
netsh http show sslcert ipport=0.0.0.0:$sslPort
pause



Write-Debug "copy cer file ..."

if (Test-Path ..\MySignalRClient)
{
    cp *.cer ..\MySignalRClient
}

pause