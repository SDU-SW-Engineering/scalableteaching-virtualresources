import { urlConfig } from "../config/urlConfig"
import { parseResponse } from "../helpers/apiHelper"

export const authAPI = {
    login
}

export function login(username, hashedPassword) {
    return fetch(urlConfig.login, {
        method: "POST",
        headers: new Headers({
            // 'Authorization': 'Bearer ' + getToken(),
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        }),
        body: JSON.stringify({username, hashedPassword})
    }).then(res => parseResponse(res))
}