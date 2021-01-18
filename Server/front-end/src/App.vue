<template>
  <div id="app">
    <!--TODO: Handle sign in verification v-if="isSignedIn()" -->
    <NavBar/>
    <router-view />
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
  import NavBar from "@/components/NavBar";
  import store from "@/store/store"
  export default {
    components: {NavBar},
    computed: {
      isSignedIn (){
        return store.state.isSignedIn;
      }
    }
  }
</script>
