const tokenName = "login-token";

export function saveJWT(token) {
    sessionStorage.setItem(tokenName, token);
}

export function loadJWT() {
    return sessionStorage.getItem(tokenName);
}

export function clearJWT() {
    return sessionStorage.removeItem(tokenName);
}