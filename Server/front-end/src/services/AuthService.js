// eslint-disable-next-line no-unused-vars
import store from '@/store/store.js';
import jwt from 'jsonwebtoken';
import authAPI from '@/api/authAPI';

import StorageHelper, {names} from '@/helpers/StorageHelper';
import router from "@/router/router";

function logout() {
    // eslint-disable-next-line no-useless-catch
    try {
        StorageHelper.clear(names.jwtName);
        store.commit('logout');
        router.push({name: 'InitialSpace'});
    } catch (e) {
        throw e;
    }
}

/**
 *
 * @param SSOToken
 * @returns {Promise<boolean>}
 */
async function login(SSOToken) {
    try {
        const response = await authAPI.login(SSOToken);
        const token = response.jwt;
        const key = await getRemotePublicKey();
        StorageHelper.set(names.tokenPublicKey, key);
        if (!token) {
            return false;
        }
        validateToken(token);
        //Cant find evidence that this will produce an error, but it might so try-catch it is.
        const userTokenObject = jwt.decode(token);
        const parsedUser = parseUser(userTokenObject);
        store.commit('login', parsedUser);
        StorageHelper.set(names.jwtName, token);
    } catch (error) {
        if(process.env.NODE_ENV === 'development') window.console.log("Login error: ", error)
        return false;
    }
    return true;
}

function re_login() {
    const key = StorageHelper.get(names.tokenPublicKey);
    const token = StorageHelper.get(names.jwtName);
    if (token !== null && key !== null) {
        const userTokenObject = jwt.decode(token);
        if (validateToken(token)) {
            const parsedUser = parseUser(userTokenObject);
            store.commit('login', parsedUser);
        }
    }
}

function parseUser(userToken) {
    return {
        gn: userToken.gn,
        sn: userToken.sn,
        cn: userToken.cn,
        uname: userToken.username,
        account_type: userToken.account_type,
        email: userToken.mail
    };
}

function validateToken(token) {
    if (!token) {
        console.log("ValidateToken - No token provided for validate token function");
        return false;
    }
    try {
        let key = StorageHelper.get(names.tokenPublicKey);
        if (key == null) {
            console.log("ValidateToken - No key in store ie, the user has not logged in in this session");
            return false;
        }
        jwt.verify(token, key);
    } catch (error) {
        if (error instanceof jwt.JsonWebTokenError) {
            window.console.log(error);//TODO: Remove Error logging from class
            StorageHelper.clear(names.jwtName);
            store.commit('logout');
            return false;
        } else if (error instanceof jwt.NotBeforeError) {
            window.console.log("Token Not yet valid: REJECTED");
            StorageHelper.clear(names.jwtName);
            store.commit('logout');
            return false;
        } else if (error instanceof jwt.TokenExpiredError) {
            window.console.log("Token Expired: REJECTED");
            StorageHelper.clear(names.jwtName);
            store.commit('logout');
            return false;
        } else {
            console.log(error.message);
            window.console.log("Unknown JWT Error occurred");
            StorageHelper.clear(names.jwtName);
            store.commit('logout');
            return false;
        }
    }
    return true;
}

function validateIsSignedIn() {
    return store.state.isSignedIn && validateToken(StorageHelper.get(names.jwtName));
}

async function getRemotePublicKey() {
    let key = StorageHelper.get(names.tokenPublicKey);
    if (key === null) {
        key = (await authAPI.getPublicKey()).Key;
        StorageHelper.set(names.tokenPublicKey, key);
    }
    return key;
}

export default {
    login,
    logout,
    validateIsSignedIn,
    re_login
};