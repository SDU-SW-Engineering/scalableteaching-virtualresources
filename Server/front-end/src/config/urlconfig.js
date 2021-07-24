const port = "8080"
const protocol = "https"
const base = "vmdeployment.hounsvad.dk"
const creation = "/api/creation"
const management = "/api/management"
const credentials = "/api/my_machines"
const course = "/api/course"
const group = "/api/group"
const groupAssignment = "/api/groupassignment"
const login = "/api/login"
const loginTokenReturnString = protocol + "%3A%2F%2F" + base + "%2FLogin"


export default {
    port,
    protocol,
    base,
    creation,
    management,
    credentials,
    login,
    loginTokenReturnString,
    course,
    group,
    groupAssignment,
}