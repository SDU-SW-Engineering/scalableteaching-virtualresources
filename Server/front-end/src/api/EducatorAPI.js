import urlconfig from "@/config/urlconfig";
import StorageHelper from "@/helpers/StorageHelper";

/**
 * Gets everyone of educator level permission
 * @returns {Promise<{body: *, status: *}>} body representing the body of the response,
 * and status being the http response code
 */
async function getEducators() {
    return await fetch(`${urlconfig.protocol}://${urlconfig.base}${urlconfig.educator}`,{
        method: "GET",
        headers: new Headers({
            'Authorization': 'Bearer ' + StorageHelper.get("login-token"),
            'Content-Type': 'application/json'
        })
    }).then(async response => {return {status: response.status, body:await response.json()}})
}

/**
 * Upgrades a user from user level to educator level
 * @param email
 * @returns {Promise<number>}
 */
async function postEducator(email) {
    return await fetch(`${urlconfig.protocol}://${urlconfig.base}${urlconfig.educator}`, {
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
 * Downgrades a user from educator to user level
 * @param email The email of the educator in question
 * @returns {Promise<number>} Http request response
 */
async function deleteEducator(email) {
    return await fetch(`${urlconfig.protocol}://${urlconfig.base}${urlconfig.educator}/${email}`, {
        method: "DELETE",
        headers: new Headers({
            'Authorization': 'Bearer ' + StorageHelper.get("login-token"),
            'Content-Type': 'application/json'
        })
    }).then(response => response.status)
}


export default {
    getEducators,
    postEducator,
    deleteEducator,
}