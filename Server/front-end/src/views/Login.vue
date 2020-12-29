<template>
  <div class="home" v-on:load="redirect">
  </div>
</template>

<script>
import AuthService from '@/services/AuthService'
export default {

  name: 'login',
  methods: {
    redirect() {
      AuthService.login(this.props.token);
    },
  }
}
</script>