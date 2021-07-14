<template>
  <b-container id="PerUserMachineConfiguration">
    <!--Per User - Upload or create groups-->
    <b-row align-content="center" id="PerUser_MachineName">
      <b-col md="11">
        <b-form-group>
          <b-form-input
              v-model="userMachineSettings.machineNamingDirective"
              placeholder="Enter the name for the machine."
              type="text"
          ></b-form-input> <!--TODO: Implement validation of input, and invalid feedback-->
        </b-form-group>
      </b-col>
      <b-col md="1">
        <b-icon icon="question-circle" id="machine-name-info-popover-target"></b-icon>
        <b-popover target="machine-name-info-popover-target" triggers="hover" placement="bottomleft">
          <template #title>Virtual Machine Name</template>
          Customize the virtual machine name.<br/>
          Machine names can be given custom automatic name changes using the following customizers.<br/>
          <dl>
            <dt>%i</dt>
            <dd>&emsp;Represents a number to differentiate machines.
              <br>&emsp;The number will be between 0(inclusive) and
              up to the number of machines created at once(exclusive)
            </dd>

            <dt>%s</dt>
            <dd>&emsp;Represents the current semester.<br/>
              &emsp;Will be presented as fx E21 or F21, where E21 would be fall 2021 and F21 for spring 2021.<br/>
              &emsp;The tag will denote a spring semester if created from January 01 to June 30.<br/>
              &emsp;The tag will denote a fall semester if created from July 01 to December 31.
            </dd>

            <dt>%u</dt>
            <dd>&emsp;Represents the username of the assigned user for the specific machine</dd>

          </dl>
        </b-popover>
      </b-col>
    </b-row>
    <b-row align-content="center" id="PerUser_UsernamesAndGroups">
      <b-col md="6">
        <b-form-group label="Enter or upload list of usernames">
          <b-form-textarea
              id="textarea"
              v-model="userMachineSettings.enteredUsersField"
              placeholder="Enter a list of usernames separated by linebreaks."
              rows="3"
              max-rows="6"
              v-if="!userMachineSettings.useUsersFile"
              :key="userMachineSettings.useUsersFile"
          ></b-form-textarea>
          <b-form-file
              v-if="userMachineSettings.useUsersFile"
              :key="userMachineSettings.useUsersFile"
              v-model="userMachineSettings.usersFile"
              :state="Boolean(userMachineSettings.usersFile)"
              accept="text/csv"
              v-on:input="debugText=2"
          ></b-form-file><!--TODO: Implement validation of users file on change of file-->

          <b-form-radio-group
              v-model="userMachineSettings.useUsersFile"
              :options="userMachineSettings.useUsersFileOptions"
          >
          </b-form-radio-group>

        </b-form-group>
      </b-col>
      <b-col md="6">
        <!--TODO: Allow for user, group, permission configuration-->
        <!--Per User: Enter Groups for machine that the user will be added to-->
        <b-form-group
            label="Enter list of linux groups">
          <b-form-textarea
              id="textarea"
              v-model="userMachineSettings.groups"
              placeholder="List of groups seperated by line breaks"
              rows="3"
              max-rows="6"
          ></b-form-textarea>
        </b-form-group>
      </b-col>
    </b-row>

    <!--TODO: Select apt ppa for installation-->
    <!--Per User: Enter List of ppa links to install-->
    <b-row align-content="center" id="PerUser_PPALinks">
      <b-col md="12">
        <b-form-group label="Enter list of additional personal package archives(PPA)">
          <b-form-textarea
              id="textarea"
              v-model="userMachineSettings.ppa"
              placeholder="Enter list of PPAs separated by linebreaks."
              rows="3"
              max-rows="6"
          ></b-form-textarea>
        </b-form-group>
      </b-col>
    </b-row>
    <b-row align-content="center" id="PerUser_AptPackagesAndPorts">
      <!--TODO: Select programs for installation-->
      <!--Per User: Enter List of apt packages to install-->
      <b-col md="6">
        <b-form-group label="Enter apt packages to install">
          <b-form-textarea
              id="textarea"
              v-model="userMachineSettings.apt"
              placeholder="Enter a list of apt package names separated by linebreaks."
              rows="3"
              max-rows="6"
          ></b-form-textarea>
        </b-form-group>
      </b-col>
      <!--Per User: Enter List of ports to forward-->
      <b-col md="6">
        <b-form-group label="Enter list of ports to forward">
          <b-form-textarea
              id="textarea"
              v-model="userMachineSettings.portsField"
              placeholder="Enter list of ports to forward, split by spaces, commas or linebreaks"
              rows="3"
              max-rows="6"
          ></b-form-textarea>
        </b-form-group>
      </b-col>
    </b-row>
  </b-container>
</template>

<script>
export default {
  name: "PerUserMachineCreation",
  data() {
    return {
      userMachineSettings: {
        portsValidation:null,
        enteredUsersField: "",
        useUsersFile: true,
        machineNamingDirective: "",
        portsField: "",
        ports: [],
        apt: "",
        ppa: "",
        groups: "",
        useUsersFileOptions: [
          {text: "Enter usernames", value: false},
          {text: "Upload file containing usernames", value: true}
        ],
        usersFile: null,
      },
    }
  },
  methods: {
    validateMachineName(){
      let name = this.userMachineSettings.machineNamingDirective
      let regex = /^(([a-zA-Z]|[a-zA-Z][a-zA-Z0-9-]*[a-zA-Z0-9])\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9-]*[A-Za-z0-9])$/
      return name.search(regex) !== -1
    },
    validateUserFile(){

    },
    validateUserList(){

    },
    validateGroups(){

    },
    validatePPA(){

    },
    validateAPT(){

    },
    validatePorts(){ //TODO: Implement functional validation for all fields
    //   if(this.userMachineSettings.portsField.length === 0) return null
    //   let initialSplit = this.userMachineSettings.portsField.split(/[\s,]/);
    //   this.userMachineSettings.ports = []
    //   for(let token in initialSplit){
    //     token = token.replace(/[\s]/, "")
    //     if(token.match(/[0-9]{1,5}/) != null && (parseInt(token) > 0 && parseInt(token) < 65535))
    //       this.userMachineSettings.ports.push(parseInt(token))
    //     else
    //       return false
    //   }
    //   return true
    }
  }
}
</script>

<style scoped>

</style>