<template>
  <div v-if="isSignedInSimple === false">
    <div v-on:load="doRedirect()">
      <p>On Login page</p>
    </div>
  </div>
  <div v-else>
    <p>You are all ready signed in</p>
  </div>
</template>

<script>
import AuthService from '@/services/AuthService'
import router from "@/router/router";
import store from "@/store/store";

export default {

  name: 'login',
  mounted() {
    this.doRedirect();
  },
  methods: {
    async doRedirect() {
      if (!store.state.isSignedIn) {
        if (await AuthService.login(this.$route.query.ticket)) {
          router.push('Machines')
        } else {
          this.doRedirect();
        }
      } else {
        router.push('Machines');
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