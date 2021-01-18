const port = "8080"
const protocol = "http"
const base = "vmdeployment.hounsvad.dk"
const creation = "/api/creation"
const management = "/api/management"
const credentials = "/api/my_machines"
const login = "/api/login"
const loginTokenReturnString = protocol + "%3A%2F%2F" + base + "%3A" + port + "%2Flogin"


export default {
    port,
    protocol,
    base,
    creation,
    management,
    credentials,
    login,
    loginTokenReturnString
}