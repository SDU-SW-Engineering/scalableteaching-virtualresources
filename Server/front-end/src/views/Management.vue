<template>
  <div>
    <b-tabs content-class="mt-3">
      <b-tab title="Machine Management"><MachineManagement/></b-tab>
      <b-tab title="Machine Creation"><MachineCreation/></b-tab>
      <b-tab title="Groups" active><GroupManagement/></b-tab>
    </b-tabs>
  </div>
</template>

<script>
import MachineCreation from "@/components/MachineCreation";
import MachineManagement from "@/components/MachineManagement";
import GroupManagement from "@/components/GroupManagement";
export default {
  name: "Management",
  components: {MachineManagement, MachineCreation, GroupManagement}
}

</script>

<style scoped>

</style>