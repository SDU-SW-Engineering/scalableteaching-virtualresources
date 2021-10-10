import StringHelper from "./StringHelper"


function validateUsers(userString: string){
    if (userString.length === 0) return false;
    let cleanTokens:string[] = StringHelper.breakStringIntoTokenList(userString);
    for (let i:number = 0; i < cleanTokens.length; i++) {
        let token:string = cleanTokens[i];
        if (token.length > 0) {
            if (token.match(/[a-zA-Z0-9]+/) === null) {
                return false;
            }
        }
    }
    return true;
}

function validateGroups(groupsString: string){
    if (groupsString.length === 0) return null;
    let cleanTokens: string[] = StringHelper.breakStringIntoTokenList(groupsString);
    for (let i: number = 0; i < cleanTokens.length; i++) {
        let token: string = cleanTokens[i];
        if (token.length > 0) {
            if (token.match(/^(([a-zA-Z]|[a-zA-Z][a-zA-Z0-9-]*[a-zA-Z0-9])\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9-]*[A-Za-z0-9])$/) === null) {
                return false;
            }
        }
    }
    return true;
}

function validatePPA(ppaString: string){
    if (ppaString.length === 0) return null;
    let cleanTokens: string[] = StringHelper.breakStringIntoTokenList(ppaString);
    for (let i: number = 0; i < cleanTokens.length; i++) {
        let token: string = cleanTokens[i];
        if (token.length > 0) {
            if (token.match(/^(ppa:([a-z-]+)\/[a-z-]+)$/) === null) {
                return false;
            }
        }
    }
    return true;
}

function validateAPT(aptString: string){
    if (aptString.length === 0) return null;
    let cleanTokens: string[] = StringHelper.breakStringIntoTokenList(aptString);
    for (let i: number = 0; i < cleanTokens.length; i++) {
        let token: string = cleanTokens[i];
        if (token.length > 0) {
            if (token.match(/[0-9A-Za-z.+-]+/) === null) {
                return false;
            }
        }
    }
    return true;
}

function validatePorts(portsString: string){
    if (portsString.length === 0) return null;
    let cleanTokens: string[] = StringHelper.breakStringIntoTokenList(this.portsField);
    for (let i: number = 0; i < cleanTokens.length; i++) {
        let token: string = cleanTokens[i];
        if (token.length > 0) {
            if (!(token.match(/[0-9]{1,5}/) !== null && (parseInt(token) > 0 && parseInt(token) <= 65535)))
                return false;
        }
    }
    return true;
}
export default{
    validateUsers,
    validateGroups,
    validatePPA,
    validateAPT,
    validatePorts,
}