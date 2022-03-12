import apiHelper from "@/helpers/apiHelper";
import urlConfig from "@/config/urlconfig";
import StorageHelper from "@/helpers/StorageHelper";

export default {
    login,
    getPublicKey
};

function login(SSOToken) {
    return fetch(urlConfig.login, {
        method: "POST",
        headers: new Headers({
            'Authorization': 'Bearer ' + StorageHelper.get("login-token"),
            'Content-Type': 'application/json',
            'Accept': 'application/json',
            'Access-Control-Allow-Origin':'https://virtualresources.sdu.dk',
        }),
        body: JSON.stringify({Token: SSOToken, ServiceEndpoint: urlConfig.loginTokenReturnString()})

    }).then(res => {apiHelper.parseResponse(res);window.console.log("test",res)});
}

async function getPublicKey() {
    return await fetch(urlConfig.login, {
        method: "GET"
    }).then(res => apiHelper.parseResponse(res));
}