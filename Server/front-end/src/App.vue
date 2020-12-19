<template>
  <div id="app">
    <b-navbar sticky toggleable="md" type="dark" variant="primary">
      <b-navbar-brand href="/">VM Deployment</b-navbar-brand>

      <b-navbar-toggle target="nav-collapse"></b-navbar-toggle>

      <b-collapse id="nav-collapse" is-nav>
        <b-navbar-nav>
<!--          <b-nav-item to="/" exact>Accessible Machines</b-nav-item>
          <b-nav-item to="/Management" exact>Machine Management</b-nav-item>
          <b-nav-item to="/Administration" exact>User Administration</b-nav-item>-->
          <b-nav-item href="/">My Machines</b-nav-item>
          <b-nav-item href="/Management" v-if="user.account_type === 'Educator' || user.account_type === 'Administrator'">Machine Management</b-nav-item>
          <b-nav-item href="/Administration" v-if="user.account_type === 'Administrator'">User Administration</b-nav-item>
        </b-navbar-nav>

        <!-- Right aligned nav items -->
        <b-navbar-nav class="ml-auto">
<!--          <b-nav-form>-->
<!--            <b-form-input size="sm" class="mr-sm-2" placeholder="Search"></b-form-input>-->
<!--            <b-button size="sm" class="my-2 my-sm-0" type="submit">Search</b-button>-->
<!--          </b-nav-form>-->

          <b-nav-item-dropdown right>
            <!-- Using 'button-content' slot -->
            <template #button-content>
              <em>{{user.uname}}</em>
            </template>
            <b-dropdown-item href="#">Sign Out</b-dropdown-item>
          </b-nav-item-dropdown>
        </b-navbar-nav>
      </b-collapse>
    </b-navbar>
    <router-view/>
  </div>

</template>

<style lang="scss">
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
}
</style>

<script>
  import store from "@/store/index.js";
  //window.console.log(this.props.user.uname)
  export default {
    computed: {
      user(){
        return store.state.user
      }
    }
  }
</script>
