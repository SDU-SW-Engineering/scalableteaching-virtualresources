<template>
  <div class="home" v-on:load="redirect">
  </div>
</template>

<script>
import AuthService from '@/services/AuthService'
import router from "@/router/router";
export default {

  name: 'login',
  methods: {
    redirect() {
      if(AuthService.login(this.props.token)){
        router.push({name: 'Machines'})
      }else{
        this.redirect();
      }
    }
  }
}
</script>