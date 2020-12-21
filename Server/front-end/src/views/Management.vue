<template>
  <div>
    <b-tabs content-class="mt-3">
      <b-tab title="Machine Management"><MachineManagement/></b-tab>
      <b-tab title="Machine Creation" active><MachineCreation/></b-tab>
    </b-tabs>
  </div>
</template>

<script>
import MachineCreation from "@/components/MachineCreation";
import MachineManagement from "@/components/MachineManagement";
export default {
name: "Management",
  components: {MachineManagement, MachineCreation}
}

</script>

<style scoped>

</style>