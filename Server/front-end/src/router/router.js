import Vue from 'vue';
import VueRouter from 'vue-router';
import Machines from "@/views/Machines";
import Login from "@/views/Login";

import AuthService from "@/services/AuthService";
import urlconfig from "@/config/urlconfig";
import store from "@/store/store";
import StorageHelper from "@/helpers/StorageHelper";

Vue.use(VueRouter);

const routes = [
    {
        path: '/Machines',
        name: 'Machines',
        component: Machines,
        meta: {
            requiresAuth: true,
            requiredType: ["User", "Educator", "Administrator"]
        }
    },
    {
        path: '/Management',
        name: 'Management',
        component: () => import("@/views/Management"),
        meta: {
            requiresAuth: true,
            requiredType: ["Educator", "Administrator"]
        }
    },
    {
        path: '/Administration',
        name: 'Administration',
        component: () => import("@/views/Administration"),
        meta: {
            requiresAuth: true,
            requiredType: ["Administrator"]
        }
    },
    {
        path: '/Login',
        name: 'Login',
        component: Login,
        props: route => ({ticket: route.query.ticket}),
    },
    {
        path: '/',
        name: 'InitialSpace'
    }
];


const router = new VueRouter({
    mode: 'history',
    base: process.env.BASE_URL,
    routes
});
export default router;


// eslint-disable-next-line no-unused-vars
router.beforeEach((to, from, next) => {
    if (from.name === null) {
        AuthService.re_login(); //Vuex dumps state on direct navigation and there for requires that the state be updated
    }
    if (AuthService.validateIsSignedIn()) {
        if (to.name === 'Login' || to.name === 'InitialSpace') {
            next({name: 'Machines'});
        }
        if (to.matched.some(record => record.meta.requiresAuth)) {
            if (to.matched.some(record => record.meta.requiredType.includes(store.state.user.account_type))) {
                next();
            } else {
                next({name: 'Machines'});
            }
        }
    } else if (to.name === 'Login' || to.name === 'InitialSpace') {
        next();
    } else {//If you are note signed in, and trying to go somewhere that requires authentication then initial login
        if (from.name === 'Login') {
            console.log("Loop error");
        } else {
            StorageHelper.set(StorageHelper.names.attemptedLocation, to.name);
            window.location.href = `https://sso.sdu.dk/login?service=${urlconfig.loginTokenReturnString}`;
        }
    }
});
