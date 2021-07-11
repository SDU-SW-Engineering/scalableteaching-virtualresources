<template>
    <!--Per Group - Upload or create groups-->
    <b-row align-content="center">
      <b-col md="6" offset-md="3" align-self="center">
        <b-form-group label="Select groups or upload a file containing groups">
          <b-form-radio-group v-model="PerGroupMachineSettings.useFile" :options="PerGroupMachineSettings.options">
          </b-form-radio-group>
          <b-form-file
              v-if="PerGroupMachineSettings.useFile"
              :key="PerGroupMachineSettings.useFile"
              v-model="PerGroupMachineSettings.file"
              :state="Boolean(PerGroupMachineSettings.file)"
              accept="application/activity+json"
              v-on:input="debugText=2"
          ></b-form-file><!--TODO: Implement validation of group file on change of file-->
          <b-form-select
              v-if="!PerGroupMachineSettings.useFile"
              :key="PerGroupMachineSettings.useFile"
              v-model="PerGroupMachineSettings.selectedGroups"
              :options="getGroupNames"
              multiple
          >
          </b-form-select>
        </b-form-group>
        <!--Allow for multiple machines pr group-->
        <b-form-select
            v-model="PerGroupMachineSettings.amountPerGroup"
            :options="getGroupNames"
        >
        </b-form-select>
        <!--TODO: Allow for port configuration of the individual machines-->
        <!--TODO: Allow for the software configuration of individual machines-->
      </b-col>
    </b-row>
</template>

<script>
export default {
  name: "PerGroupMachineCreation",
  PerGroupMachineSettings: {
    useFile: false,
    file: null,

  }

  // ,
  // groupMachines: {
  //   file: "",
  //   selectedGroups: [],
  //   useFile: false,
  //   options: [
  //     {text: "Select Existing Groups", value: false},
  //     {text: "Upload File for New Groups", value: true}
  //   ],
  //   amountPerGroup: 1,
  //   amountPerGroupOptions: [
  //     {text: 1, value: 1},
  //     {text: 2, value: 2},
  //     {text: 3, value: 3},
  //     {text: 4, value: 4},
  //     {text: 5, value: 5},
  //   ]
  // }
}
</script>

<style scoped>

</style>