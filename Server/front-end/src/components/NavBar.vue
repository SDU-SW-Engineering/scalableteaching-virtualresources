<template>
    <b-navbar sticky toggleable="md" type="dark" variant="primary">
      <b-navbar-brand to="/">VM Deployment</b-navbar-brand>

      <b-navbar-toggle target="nav-collapse"></b-navbar-toggle>

      <b-collapse id="nav-collapse" is-nav>
        <!--Left aligned nav items-->
        <b-navbar-nav v-if="isSignedInSimple">

          <b-nav-item to="Machines" >My Machines</b-nav-item>
          <b-nav-item to="Management" v-if="(isEducator || isAdministrator)">Machine Management</b-nav-item>
          <b-nav-item to="Administration" v-if="(isAdministrator)">User Administration</b-nav-item>
        </b-navbar-nav>
        <!-- Right aligned nav items -->
        <b-navbar-nav class="ml-auto">
          <b-nav-item-dropdown right>
            <template #button-content>
              <em v-if="isSignedInSimple">{{username}}</em>
            </template>
            <b-dropdown-item v-on:click="signOut">Sign Out</b-dropdown-item><!--TODO: Validate that this works-->
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
    username(){
      return store.state.user.uname;
    },
    user(){
      return store.state.user;
    },
    isSignedInSimple(){
      return store.state.isSignedIn;
    },
    isSignedIn(){
      return AuthService.validateIsSignedIn();
    },
    isEducator(){
      return store.state.user.account_type === 'Educator';
    },
    isAdministrator(){
      return store.state.user.account_type === 'Administrator';
    }
  },
  methods: {
    signOut(){
      AuthService.logout();
    },
  }
}
</script>

<style scoped>

</style>