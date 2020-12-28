import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

export default new Vuex.Store({
  state: {
    user: {
      gn: "Test Man",
      sn: "User",
      cn: "Test Man User",
      uname: "temus20",
      account_type: "Administrator",
      email: "temus20@student.sdu.dk"
    },
    isSignedIn: true,
  },
  mutations: {
    login(state, user){
      state.user = user;
      state.isSignedIn = true;
    },
    logout(state){
      state.user = {};
      state.isSignedIn = false;
    }
  },
  actions: {
  },
  modules: {
  }
})
