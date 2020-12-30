import apiHelper from "@/helpers/apiHelper"
import urlConfig from "@/config/urlconfig";

export default {
    login
}

function login(SSOToken) {
    return fetch(urlConfig.login, {
        method: "POST",
        headers: new Headers({
            // 'Authorization': 'Bearer ' + getToken(),
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        }),
        body: JSON.stringify({token: SSOToken})

    }).then(res => apiHelper.parseResponse(res))
}