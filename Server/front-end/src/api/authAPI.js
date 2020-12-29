import { urlConfig } from "../config/urlConfig"
import { parseResponse } from "../helpers/apiHelper"

export default {
    login
}

export function login(SSOToken) {
    return fetch(urlConfig.login, {
        method: "POST",
        headers: new Headers({
            // 'Authorization': 'Bearer ' + getToken(),
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        }),
        body: JSON.stringify({token: SSOToken})

    }).then(res => parseResponse(res))
}