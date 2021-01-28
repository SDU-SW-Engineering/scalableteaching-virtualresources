import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)
const initialState = {
    user: null,
    isSignedIn: false,
}

/*const initialState = {
    user: {
        gn: "Test Man",
        sn: "User",
        cn: "Test Man User",
        uname: "temus20",
        account_type: "Administrator",
        email: "temus20@student.sdu.dk"
    },
    isSignedIn: true,
}*/

export default new Vuex.Store({
    state: initialState,
    mutations: {
        login(state, user) {
            state.user = {
                gn: user.gn,
                sn: user.sn,
                cn: user.cn,
                uname: user.uname,
                account_type: user.account_type,
                email: user.email,
            };
            state.isSignedIn = true;
        },
        logout(state) {
            state.user = initialState.user;
            state.isSignedIn = initialState.isSignedIn;
        }
    },
    actions: {},
    modules: {}
})
