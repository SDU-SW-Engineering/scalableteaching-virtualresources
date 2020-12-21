import Vue from 'vue'
import VueRouter from 'vue-router'

import Machines from "@/views/Machines";
import Administration from "@/views/Administration";
import Management from "@/views/Management";

Vue.use(VueRouter)

const routes = [
  {
    path: '/',
    name: 'Machines',
    component: Machines
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
  }

]

const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes
})

export default router
