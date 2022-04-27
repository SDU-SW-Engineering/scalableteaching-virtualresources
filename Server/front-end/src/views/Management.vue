<template>
  <div>
    <b-tabs content-class="mt-3">
      <b-tab active title="Machine Management">
        <MachineManagement/>
      </b-tab>
      <b-tab title="Machine Creation">
        <MachineCreation/>
      </b-tab>
      <b-tab title="Groups">
        <GroupManagement/>
      </b-tab>
    </b-tabs>
  </div>
</template>

<script>
import MachineCreation from "@/components/MachineCreationComponents/MachineCreation";
import MachineManagement from "@/components/MachineManagement";
import GroupManagement from "@/components/GroupManagement";

export default {
  name: "Management",
  components: {MachineManagement, MachineCreation, GroupManagement}
};

</script>

<style scoped>

</style>