import urlconfig from "@/config/urlconfig";
import StorageHelper from "@/helpers/StorageHelper";


async function createMachines(machines, isGroupBased) {
    if (!isGroupBased) {
        return await fetch(`${urlconfig.protocol}://${urlconfig.getBase()}${urlconfig.creation}/userbased`, {
            method: "POST",
            headers: new Headers({
                'Authorization': 'Bearer ' + StorageHelper.get("login-token"),
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin':'https://virtualresources.sdu.dk',
            }),
            body: JSON.stringify({
                machines
            })
        }).then(async response => response.status);
    } else {
        return await fetch(`${urlconfig.protocol}://${urlconfig.getBase()}${urlconfig.creation}/groupbased`, {
            method: "POST",
            headers: new Headers({
                'Authorization': 'Bearer ' + StorageHelper.get("login-token"),
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin':'https://virtualresources.sdu.dk',
            }),
            body: JSON.stringify({
                machines
            })
        }).then(async response => response.status);
    }
}

export default {
    createMachines,
};