import urlconfig from "@/config/urlconfig";
import StorageHelper from "@/helpers/StorageHelper";

/**
 * Gets everyone of manager level permission
 * @returns {Promise<{body: *, status: *}>} body representing the body of the response,
 * and status being the http response code
 */
async function getManagers() {
    return await fetch(`${urlconfig.protocol}://${urlconfig.base}${urlconfig.manager}`,{
        method: "GET",
        headers: new Headers({
            'Authorization': 'Bearer ' + StorageHelper.get("login-token"),
            'Content-Type': 'application/json'
        })
    }).then(async response => {return {status: response.status, body:await response.json()}})
}

/**
 * Upgrades a user from user level to manager level
 * @param email
 * @returns {Promise<number>}
 */
async function postManager(email) {
    return await fetch(`${urlconfig.protocol}://${urlconfig.base}${urlconfig.manager}`, {
        method: "POST",
        headers: new Headers({
            'Authorization': 'Bearer ' + StorageHelper.get("login-token"),
            'Content-Type': 'application/json'
        }),
        body: JSON.stringify({
            email: email,
        })
    }).then(response => response.status)
}

/**
 * Downgrades a user from manager to user level
 * @param email The email of the manager in question
 * @returns {Promise<number>} Http request response
 */
async function deleteManager(email) {
    return await fetch(`${urlconfig.protocol}://${urlconfig.base}${urlconfig.manager}/${email}`, {
        method: "DELETE",
        headers: new Headers({
            'Authorization': 'Bearer ' + StorageHelper.get("login-token"),
            'Content-Type': 'application/json'
        })
    }).then(response => response.status)
}


export default {
    getManagers,
    postManager,
    deleteManager,
}