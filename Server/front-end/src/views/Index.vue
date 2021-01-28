<template>
  <div>
    <b-button size="lg" v-on:click="loginReturnString">Login</b-button>
  </div>
</template>

<script>
import urlconfig from "@/config/urlconfig";
import store from "@/store/store";


export default {

  name: "Index",
  computed: {
    isSignedIn() {
      return store.state.isSignedIn;
    }
  },
  methods: {
    loginReturnString(){
      return window.location.href = `https://sso.sdu.dk/login?service=${urlconfig.loginTokenReturnString}`;
    },
  }
}
</script>

<style scoped>

</style>