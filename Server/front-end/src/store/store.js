import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)
/*TODO: Set this as the initial state
const initialState = {
    user: null,
    isSignedIn: false,
}*/

const initialState = {
    user: {
        gn: "Test Man",
        sn: "User",
        cn: "Test Man User",
        uname: "temus20",
        account_type: "Administrator",
        email: "temus20@student.sdu.dk"
    },
    isSignedIn: true,
}

export default new Vuex.Store({
    state: initialState,
    mutations: {
        login(state, user) {
            state.user = user;
            state.isSignedIn = true;
        },
        logout() {
            this.state = initialState;
        }
    },
    actions: {},
    modules: {}
})
