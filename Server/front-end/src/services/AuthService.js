/*// eslint-disable-next-line no-unused-vars
import {store, parseUser} from '@/store/index.js'
import router from '@/router/index.js'
import jwt from 'jsonwebtoken'
import authAPI from '@/api/authAPI'
import {clearJWT, loadJWT, saveJWT} from '@/helpers/tokenHelper'

export const authService =
    {
        login,
        automaticReLogin,
    }

async function login(username, password) {
    // eslint-disable-next-line no-useless-catch
    try {
        const response = await authAPI.login(username, password);
        const token = response.jwt;
        // eslint-disable-next-line no-unused-var
        const user = jwt.decode(token);

        store.commit('login', 'parseUser(user)')

        saveJWT(token);

        router.push({path: '/machines'})
    } catch (error) {
        throw error
    }
}

function automaticReLogin() {
    const token = loadJWT();
    if (token) {
        const user = jwt.decode(token);

    }
}

function validateTokenDate(user) {
    const expDate = new Date(0);
    expDate.setSeconds(user.exp)
}*/

/*
function handleAutomaticLogin() {
    const token = getJWTTokenFromLocalStorage();
    if (token) {
        // TODO: Verify token endpoint?
        const user = jwt.decode(getJWTTokenFromLocalStorage());

        const expDate = new Date(0);
        expDate.setSeconds(user.exp);

        const isExpired = new Date().getTime() > expDate.getTime();

        if (isExpired) { // Token is invalid.
            clearJWTTokenFromLocalStorage();
            throw new Error("Login expired");
        }
        globalStore.commit('login', user);
        router.push({ path: '/' });
    }
}

function logout() {
    clearJWTTokenFromLocalStorage();
    globalStore.commit('logout');
    router.push({ path: '/login' });
}

export const authService = {
    login,
    handleAutomaticLogin,
    logout
}*/
