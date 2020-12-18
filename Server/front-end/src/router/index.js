import Vue from 'vue'
import VueRouter from 'vue-router'
import Home from '../views/Home.vue'
import Machines from "@/components/Machines";
import Administration from "@/components/Administration";
import Management from "@/components/Management";

Vue.use(VueRouter)

const routes = [
  {
    path: '/',
    name: 'Home',
    component: Home
  },
  {
    path: '/Machines',
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
