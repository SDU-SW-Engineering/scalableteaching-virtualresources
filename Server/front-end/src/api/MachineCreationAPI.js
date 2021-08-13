import urlconfig from "@/config/urlconfig";
import StorageHelper from "@/helpers/StorageHelper";


async function createMachines(machineList) {
    return await fetch(`${urlconfig.protocol}://${urlconfig.base}${urlconfig.creation}`, {
        method: "POST",
        headers: new Headers({
            'Authorization': 'Bearer ' + StorageHelper.get("login-token"),
            'Content-Type': 'application/json'
        }),
        body: JSON.stringify({
            machineList
        })
    }).then(async response => {return {status: response.status, body:await response.json()}})

}

export default {
    createMachines,
}