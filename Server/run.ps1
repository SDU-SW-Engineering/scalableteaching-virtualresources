function startssh (){
    ssh sdu2 "cd /ScalableTeaching && docker-compose down && docker-compose pull && docker-compose up"
}

function buildpush(){
    docker build --no-cache -t ghcr.io/hounsvad/scalableteaching . 
    docker push ghcr.io/hounsvad/scalableteaching 
}

if((Test-Path ./lastRun) -eq $false){
    buildpush
}else{
    $hash = git log -1 --pretty=format:"%h"
    $oldhash = Get-Content ./lastRun
    if($hash -ne $oldhash){
        buildpush
        $hash | Out-File ./lastRun
    }
}
startssh




