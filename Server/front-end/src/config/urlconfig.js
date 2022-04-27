const port = "8080";
const protocol = process.env.VUE_APP_PROTOCOL;
const base = "virtualresources.sdu.dk";
const creation = "/api/creation";
const management = "/api/management";
const credentials = "/api/my_machines";
const course = "/api/course";
const group = "/api/group";
const educator = "/api/educator";
const groupAssignment = "/api/groupassignment";
const machine = "/api/machine";
const login = "/api/login";

function getBase (){
    if(process.env.NODE_ENV === 'development'){
        return window.location.host
    }else{
        return base;
    }
}

function loginTokenReturnString (){
    let uriComponent = process.env.VUE_APP_PROTOCOL + "://" + getBase() + "/Login";
    if(process.env.NODE_ENV === 'development')
        window.console.log(process.env.NODE_ENV + " " + uriComponent);
    return encodeURIComponent(uriComponent);
}


export default {
    getBase,
    port,
    protocol,
    base,
    creation,
    management,
    educator,
    credentials,
    login,
    loginTokenReturnString,
    course,
    group,
    groupAssignment,
    machine,
};
