import Vue from 'vue'
import VueRouter from 'vue-router'

import Machines from "@/views/Machines";
import Administration from "@/views/Administration";
import Management from "@/views/Management";
import Login from "@/views/Login";


import AuthService from "@/services/AuthService";

import urlConfig from "@/config/urlconfig";

Vue.use(VueRouter)

const routes = [
    {
        path: '/Machines',
        name: 'Machines',
        component: Machines,
        meta: {}
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
        props: route => ({token: route.query.token}),
    },
    {
        path: '/',
        name: 'InitialSpace',
        component: Login,
        beforeRouteEnter: (to, from, next) => {
            if (AuthService.validateIsSignedIn()) {
                next({name: 'Machines'})
            } else {
                window.location.href("sso.sdu.dk/login?service=" + urlConfig.loginTokenReturnString)
            }
        }
    }
]

const router = new VueRouter({
    mode: 'history',
    base: process.env.BASE_URL,
    routes
})

// eslint-disable-next-line no-unused-vars
router.beforeEach((to, from, next) => {

    if (AuthService.validateIsSignedIn()) {
        if (to.name === 'Login') {
            next({name: 'Machines'})
        }
        next()
    } else if (to.name === 'Login') {
        next()
    } else {
        next({name: 'login'})
    }
})

export default router
