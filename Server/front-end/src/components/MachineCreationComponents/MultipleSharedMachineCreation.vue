<template>
  <b-row align-content="center">
    <b-col xl="6" offset-xl="3" align-self="center">
      <b-form-group label="Select amount of share machines">
        <b-form-select
            v-model="settings.sharedMachineAmount.selected"
            :options="settings.sharedMachineAmount.options"
        ></b-form-select>
      </b-form-group>
    </b-col>
  </b-row>
</template>

<script>
export default {
name: "MultipleSharedMachineCreation"
}
</script>

<style scoped>

</style>