<template>
  <b-container fluid>

  </b-container>
</template>

<script>
import store from "@/store/store.js"
export default {
  name: "GroupManagement",
  data: function(){
    return {

    }
  },
  methods: {
    getGroups: function(){
      //TODO: Implement returning groups
    },
    getUser: function(){
      return store.state.user
    }
  }
}
</script>

<style scoped>

</style>