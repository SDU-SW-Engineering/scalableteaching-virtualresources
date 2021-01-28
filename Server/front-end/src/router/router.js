import Vue from 'vue'
import VueRouter from 'vue-router'

import Machines from "@/views/Machines";
import Administration from "@/views/Administration";
import Management from "@/views/Management";
import Login from "@/views/Login";
import Index from "@/views/Index";
//import store from "@/store/store";
//import urlconfig from "@/config/urlconfig";



import AuthService from "@/services/AuthService";

import urlConfig from "@/config/urlconfig";

Vue.use(VueRouter)


const routes = [
    {
        path: '/Machines',
        name: 'Machines',
        component: Machines,
        //meta: {}
    },
    {
        path: '/Management',
        name: 'Management',
        component: Management
    },
    {
        path: '/Administration',
        name: 'Administration',
        component: Administration
    },
    {
        path: '/Login',
        name: 'Login',
        component: Login,
        props: route => ({token: route.query.ticket}),
    },
    {
        path: '/',
        name: 'InitialSpace',
        component: Index,
    }
]


const router = new VueRouter({
    mode: 'history',
    base: process.env.BASE_URL,
    routes
})
export default router



// eslint-disable-next-line no-unused-vars
router.beforeEach((to, from, next) => {
    if(from.name=== null){//TODO: Might cause exception
        AuthService.re_login(); //Vuex dumpes state on direct navigation.
    }
    if (AuthService.validateIsSignedIn()) {
        if (to.name === 'Login' || to.name === 'InitialSpace') {
            next({name: 'Machines'})
        }
        next()
    } else if (to.name === 'Login') {
        next()
    } else {
        if(from.name === 'Login'){
            console.log("Loop error");
        }
        next()
    }
});
