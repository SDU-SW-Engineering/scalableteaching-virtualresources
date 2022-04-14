import apiHelper from "@/helpers/apiHelper";
import urlConfig from "@/config/urlconfig";
import StorageHelper from "@/helpers/StorageHelper";

export default {
    login,
    getPublicKey
};

async function login(SSOToken) {
    return await fetch(urlConfig.login, {
        method: "POST",
        headers: new Headers({
            'Authorization': 'Bearer ' + StorageHelper.get("login-token"),
            'Content-Type': 'application/json',
            'Accept': 'application/json',
            'Access-Control-Allow-Origin':'https://virtualresources.sdu.dk',
        }),
        body: JSON.stringify({Token: SSOToken, ServiceEndpoint: urlConfig.loginTokenReturnString()})

    }).then(res => {return apiHelper.parseResponse(res);});
}

async function getPublicKey() {
    return await fetch(urlConfig.login, {
        method: "GET"
    }).then(res => apiHelper.parseResponse(res));
}