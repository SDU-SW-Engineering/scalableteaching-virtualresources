function startssh (){
    ssh sdu2 "cd /ScalableTeaching && docker-compose down && docker-compose pull && docker-compose up"
}

function buildpush(){
    docker build --no-cache -t ghcr.io/hounsvad/scalableteaching . 
    docker push ghcr.io/hounsvad/scalableteaching 
}

if((Test-Path ./lastRun) -eq $false){
    Write-Output "./lastRun file not found, building" 
    buildpush
    Write-Output "image built"
}else{
    $hash = git log -1 --pretty=format:"%h"
    $oldhash = Get-Content ./lastRun
    if($hash -ne $oldhash){
        Write-Output "New commit since last run, rebuilding"
        buildpush
    }
}
Write-Output "imag built, saving current commit to ./lastRun "
$hash | Out-File ./lastRun

startssh




