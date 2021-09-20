const tokenName = "login-token";

const consentStorage = "consentStorage";
export const names = {
    jwtName: "login-token",
    tokenPublicKey: "API-PublicKey",
    attemptedLocation: "AttemptedLocation"
};


export function saveJWT(token) {
    sessionStorage.setItem(tokenName, token);
}

export function loadJWT() {
    return sessionStorage.getItem(tokenName);
}

export function clearJWT() {
    return sessionStorage.removeItem(tokenName);
}

//TODO: Implement storage consent

/**
 * Gets the value of the specific cookie from either session storage or local storage depending on user preference
 * @param {String} name
 * @throws Error if the name is not a string
 * @throws Error if name is not in Names
 */
function get(name) {
    validateName(name);
    if (localStorage.getItem(consentStorage) === null) {
        return sessionStorage.getItem(name);
    } else {
        return localStorage.getItem(name);
    }
}

/**
 * Sets the value of the specific cookie from either session storage or local storage depending on user preference
 * @param {String} name
 * @param {Object, String} payload
 * @throws Error if the name is not a string
 * @throws Error if the payload is not provided
 * @throws Error if name is not in Names
 */
function set(name, payload) {
    /*Fix Typing*/
    if (typeof payload === Object) {
        payload = JSON.stringify(payload);
    }
    /*Validate input*/
    validateName(name);
    if (payload === null || payload === undefined) throw new Error("payload cannot be null or undefined");

    /*Handle operation based on consent*/
    if (localStorage.getItem(consentStorage) === null) {
        return sessionStorage.setItem(name, payload);
    } else {
        return localStorage.setItem(name, payload);
    }
}

/**
 * Clears the value of the specific cookie from either session storage or local storage depending on user preference
 * @param {String} name
 * @throws Error if the name is not a string
 * @throws Error if name is not in Names
 */
function clear(name) {
    validateName(name);
    /*Handle operation based on consent*/
    if (localStorage.getItem(consentStorage) === null) {
        return sessionStorage.removeItem(name);
    } else {
        return localStorage.removeItem(name);
    }
}

/**
 * @param {String} name
 * @throws Error if the name is not a string
 * @throws Error if name is not in Names
 */
function validateName(name) {
    // eslint-disable-next-line no-prototype-builtins
    if (names.hasOwnProperty(name)) throw new Error("Name not in names");
    if (name === null || name === undefined) throw new Error("Name cannot be null or undefined");
    if (typeof name !== "string") throw new Error("Type of name is not string");
}

export default {
    set,
    get,
    clear,
    names
};
