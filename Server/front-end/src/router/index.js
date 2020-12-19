import Vue from 'vue'
import VueRouter from 'vue-router'

import Machines from "@/components/Machines";
import Administration from "@/components/Administration";
import Management from "@/components/Management";

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
