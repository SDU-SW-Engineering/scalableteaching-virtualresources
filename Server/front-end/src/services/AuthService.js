// eslint-disable-next-line no-unused-vars
import store from '@/store/store'
import jwt from 'jsonwebtoken'
import authAPI from '@/api/authAPI'
import {clearJWT, saveJWT} from '@/helpers/tokenHelper'
//import {clearJWT, loadJWT, saveJWT} from '@/helpers/tokenHelper'
export default {
    login,
    logout,
    validateIsSignedIn
}

async function logout() {
    // eslint-disable-next-line no-useless-catch
    try {
        clearJWT();
        store.commit('logout')
    } catch (e) {
        throw e
    }
}

/**
 *
 * @param SSOToken
 * @returns {Promise<boolean>}
 */
async function login(SSOToken) {
    // eslint-disable-next-line no-useless-catch
    try {
        const response = await authAPI.login(SSOToken);
        const token = response.jwt;

        if(!token){
            return false
        }

        try{
            jwt.verify(token)
        }catch(error){
            if(error instanceof jwt.JsonWebTokenError){
                window.console.log(error);
                clearJWT();
                store.commit('logout');
                return false;
            }else if(error instanceof jwt.NotBeforeError){
                window.console.log("Token Not yet valid: REJECTED");
                clearJWT();
                store.commit('logout');
                return false;
            }else if(error instanceof jwt.TokenExpiredError){
                window.console.log("Token Expired: REJECTED");
                clearJWT();
                store.commit('logout');
                return false;
            }else{
                window.console.log(error);
            }
        }
        //Cant find evidence that this will produce an error, but it might so try-catch it is.
        const user = jwt.decode(token);
        store.commit('login', parseUser(user))
        saveJWT(token);
    } catch (error) {
        return false
    }
    return true
}

function parseUser(userToken) {
    return {
        gn: userToken.gn,
        sn: userToken.sn,
        cn: userToken.cn,
        uname: userToken.uname,
        account_type: userToken.account_type,
        email: userToken.email
    }
}

// function validateTokenExpiration() {
//     try {
//         const token = loadJWT();
//         return jwt.verify(token) === null;
//     }catch (e) {
//         return false;
//     }
// }

function validateIsSignedIn() {
    return true;//store.state.isSignedIn && validateTokenExpiration() TODO: Return to functional
}
