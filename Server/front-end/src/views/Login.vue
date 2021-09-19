<template>
  <div></div>
</template>
<script>
import AuthService from '@/services/AuthService'
import router from "@/router/router";
import store from "@/store/store";
import urlconfig from "@/config/urlconfig";
import StorageHelper from "@/helpers/StorageHelper";

export default {
  props: ['ticket'],
  name: 'login',
  mounted() {
    this.doRedirect();
  },
  methods: {
    doRedirect() {
      const loginComponent = this;

      /*If the user is not signed in, go through signin procedures.
      * If the user is signed in then route the to Machines*/
      if (!store.state.isSignedIn) {
        /*If no ticket is provided, attempt to acquire ticket
        * If ticket is available, attempts login*/
        if (this.ticket === undefined) {
          window.location.href = `https://sso.sdu.dk/login?service=${urlconfig.loginTokenReturnString}`;
        } else {
          (async () => {
            if (await AuthService.login(loginComponent.ticket)) {
              let attemptLocation = StorageHelper.get(StorageHelper.names.attemptedLocation)
              if(attemptLocation !== null){
                router.push({name: attemptLocation});
              }
              router.push({name: 'Machines'});
            } else {
              console.log("Login Error")
              router.push({name: 'InitialSpace'})
            }
          })();
        }
      } else {
        router.push({name: 'Machines'});
      }
    }
  },
  computed: {
    isSignedInSimple() {
      return store.state.isSignedIn;
    }
  }
}
</script>