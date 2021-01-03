<template>
  <b-navbar sticky toggleable="md" type="dark" variant="primary">
    <b-navbar-brand href="/">VM Deployment</b-navbar-brand>

    <b-navbar-toggle target="nav-collapse"></b-navbar-toggle>

    <b-collapse id="nav-collapse" is-nav>
      <!--Left aligned nav items-->
      <b-navbar-nav v-if="isLoggedIn">

        <b-nav-item href="/Machines" >My Machines</b-nav-item>
        <b-nav-item href="/Management" v-if="(user.account_type === 'Educator' || user.account_type === 'Administrator')">Machine Management</b-nav-item>
        <b-nav-item href="/Administration" v-if="(user.account_type === 'Administrator')">User Administration</b-nav-item>
      </b-navbar-nav>
      <!-- Right aligned nav items -->
      <b-navbar-nav class="ml-auto">
        <b-nav-item-dropdown right>
          <template #button-content>
            <em>{{user.uname}}</em>
          </template>
          <b-dropdown-item href="#">Sign Out</b-dropdown-item><!--TODO: Create Sign out functionality-->
        </b-nav-item-dropdown>
      </b-navbar-nav>
    </b-collapse>
  </b-navbar>
</template>

<script>
import store from "@/store/store";
import AuthService from "@/services/AuthService";

export default {
  name: "NavBar",
  computed: {
    user(){
      return store.state.user
    },
    isLoggedIn(){
      return AuthService.validateIsSignedIn()
    }
  }
}
</script>

<style scoped>

</style>