import urlconfig from "@/config/urlconfig";
import StorageHelper from "@/helpers/StorageHelper";
import axios from "axios";

async function getUsersMachines() {
    return await fetch(`${urlconfig.protocol}://${urlconfig.getBase()}${urlconfig.machine}`, {
        method: "GET",
        headers: new Headers({
            'Authorization': 'Bearer ' + StorageHelper.get("login-token"),
            'Content-Type': 'application/json',
            'Access-Control-Allow-Origin': 'https://virtualresources.sdu.dk',
        })
    }).then(async response => {
        return {status: response.status, body: await response.json()};
    });
}

async function postRebootMachine(machineId) {
    return await fetch(`${urlconfig.protocol}://${urlconfig.getBase()}${urlconfig.machine}/control/reboot/${machineId}`, {
        method: "POST",
        headers: new Headers({
            'Authorization': 'Bearer ' + StorageHelper.get("login-token"),
            'Content-Type': 'application/json',
            'Access-Control-Allow-Origin': 'https://virtualresources.sdu.dk',
        })
    }).then(async response => response.status);
}

async function postResetMachine(machineId) {
    return await fetch(`${urlconfig.protocol}://${urlconfig.getBase()}${urlconfig.machine}/control/reset/${machineId}`, {
        method: "POST",
        headers: new Headers({
            'Authorization': 'Bearer ' + StorageHelper.get("login-token"),
            'Content-Type': 'application/json',
            'Access-Control-Allow-Origin': 'https://virtualresources.sdu.dk',
        })
    }).then(async response => response.status);
}

async function deleteMachine(machineId) {
    return await fetch(`${urlconfig.protocol}://${urlconfig.getBase()}/api/Machine/control/delete/${machineId}`, {
        method: "DELETE",
        headers: new Headers({
            'Authorization': 'Bearer ' + StorageHelper.get("login-token"),
            'Content-Type': 'application/json',
            'Access-Control-Allow-Origin': 'https://virtualresources.sdu.dk',
        })
    }).then(async response => {
        return { status: response.status, body:await response.body };
    });
}

function getZip() {
    return axios({
        url: `${urlconfig.protocol}://${urlconfig.getBase()}/api/machinecredential/file/zip`,
        method: 'GET',
        responseType: 'blob',
        headers: {
            'Authorization': 'Bearer ' + StorageHelper.get("login-token"),
            'Access-Control-Allow-Origin': 'https://virtualresources.sdu.dk',
        }
    });
}


export default {
    getUsersMachines,
    postRebootMachine,
    getZip,
    deleteMachine,
    postResetMachine,
};