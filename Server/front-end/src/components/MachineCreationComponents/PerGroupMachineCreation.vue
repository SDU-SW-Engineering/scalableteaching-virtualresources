<template>
  <b-container id="PerUserMachineConfiguration">
    <!--Per User - Upload or create groups-->
    <b-row align-content="center" id="PerGroup_MachineName">
      <b-col md="11">
        <b-form-group>
          <b-form-input
              v-model="settings.machineNamingDirective"
              placeholder="Enter the name for the machine."
              type="text"
              :state="validateMachineName()"
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

            <dt>%g</dt>
            <dd>&emsp;Represents the groupnumber of the assigned group for the specific machine</dd>

          </dl>
        </b-popover>
      </b-col>
    </b-row>

    <b-row align-content="center" id="PerGroup_GroupNamesAndLinuxGroups">
      <b-col md="6">
        <b-form-group label="Select groups">
          <b-form-select
              :state="validateSelectedGroups()"
              :options="groupSelectionOptions"
              multiple
              :select-size=3
              v-model="settings.selectedGroups"
          >

          </b-form-select>
        </b-form-group>
      </b-col>
      <b-col md="6">
        <!--Per User: Enter Groups for machine that the user will be added to-->
        <b-form-group
            label="Enter list of linux groups">
          <b-form-textarea
              id="textarea"
              v-model="linuxGroupsField"
              placeholder="List of groups separated by line breaks, commas or spaces."
              rows="3"
              max-rows="6"
              :state="validateGroups()"
              debounce="300"
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
              v-model="ppaField"
              placeholder="Enter list of PPAs separated by linebreaks, commas or spaces."
              rows="3"
              max-rows="6"
              :state="validatePPA()"
              debounce="300"
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
              v-model="aptField"
              placeholder="Enter a list of apt package names separated by linebreaks, commas or spaces."
              rows="3"
              max-rows="6"
              :state="validateAPT()"
              debounce="300"
          ></b-form-textarea>
        </b-form-group>
      </b-col>
      <!--Per User: Enter List of ports to forward-->
      <b-col md="6">
        <b-form-group label="Enter list of ports to forward">
          <b-form-textarea
              id="textarea"
              v-model="portsField"
              placeholder="Enter list of ports to forward separated by linebreaks, commas or spaces."
              rows="3"
              max-rows="6"
              :state="validatePorts()"
              debounce="300"
          ></b-form-textarea>
        </b-form-group>
      </b-col>
    </b-row>
  </b-container>
</template>

<script>
import GroupAPI from "@/api/GroupAPI";
import StringHelper from "@/helpers/StringHelper";

export default {
  name: "PerGroupMachineCreation",
  props: ['classObject'],
  data() {
    return {
      groupSelectionOptions: [],
      portsField: "",
      aptField: "",
      linuxGroupsField: "",
      ppaField: "",
      settings: {
        selectedGroups: [null],
        portsValidation: null,
        enteredUsersField: "",
        useGroupsFile: true,
        machineNamingDirective: "",
        ports: [],
        apt: [],
        ppa: [],
        groups: [],
        useUsersFileOptions: [
          {text: "Enter usernames", value: false},
          {text: "Upload file containing usernames", value: true}
        ],
        groupsFile: null,
      },
    }
  },
  methods: {
    validateMachineName() {
      let name = this.settings.machineNamingDirective
      name = name.replace("%i", "00").replace("%g", "abcde01").replace("%s", "e01")
      let regex = /^(([a-zA-Z]|[a-zA-Z][a-zA-Z0-9-]*[a-zA-Z0-9])\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9-]*[A-Za-z0-9])$/g
      return name.search(regex) !== -1
    },
    validateSelectedGroups() {
      let groups = this.settings.selectedGroups
      if (groups.length < 1 || (groups.length === 1 && groups[0] === null)) return false
    },
    validateGroups() {
      if (this.linuxGroupsField.length === 0) return null
      let cleanTokens = StringHelper.breakStringIntoTokenList(this.linuxGroupsField)
      for(let i = 0; i < cleanTokens.length; i++) {
        let token = cleanTokens[i]
        if (token.length > 0) {
          if (token.match(/^(([a-zA-Z]|[a-zA-Z][a-zA-Z0-9-]*[a-zA-Z0-9])\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9-]*[A-Za-z0-9])$/) === null) {
            return false
          }
        }
      }
      return true
    },
    validatePPA() {
      this.settings.ppa = []
      if (this.ppaField.length === 0) return null
      let cleanTokens = StringHelper.breakStringIntoTokenList(this.ppaField)
      for(let i = 0; i < cleanTokens.length; i++) {
        let token = cleanTokens[i]
        if (token.length > 0) {
          if (token.match(/^(ppa:([a-z-]+)\/[a-z-]+)$/) === null) {
            return false
          }
        }
      }
      return true
    },
    validateAPT() {
      this.settings.apt = []
      if (this.aptField.length === 0) return null
      let cleanTokens = StringHelper.breakStringIntoTokenList(this.aptField)
      for(let i = 0; i < cleanTokens.length; i++) {
        let token = cleanTokens[i]
        if (token.length > 0) {
          if (token.match(/[0-9A-Za-z.+-]+/) === null) {
            return false
          }
        }
      }
      return true
    },
    validatePorts() {
      this.settings.ports = []
      if (this.portsField.length === 0) return null
      let cleanTokens = StringHelper.breakStringIntoTokenList(this.portsField)
      for(let i = 0; i < cleanTokens.length; i++) {
        let token = cleanTokens[i]
        if (token.length > 0) {
          if (!(token.match(/[0-9]{1,5}/) !== null && (parseInt(token) > 0 && parseInt(token) <= 65535)))
            return false
        }
      }
      return true
    },
    async updateGroupList() {
      this.groupSelectionOptions = []
      this.settings.selectedGroups = []
      let groupsResponse = await GroupAPI.getGroupsByCourseID(this.classObject.courseID)
      if (groupsResponse.body === undefined) return
      for(let i = 0; i < groupsResponse.length; i++) {
        let group = groupsResponse[i]
        this.groupSelectionOptions.push({
          value: group,
          text: group.groupName
        })
      }
    }

  },
  watch: {
    // eslint-disable-next-line no-unused-vars
    classObject: function (newVal, oldVal) {
      this.updateGroupList()
    }
  }
}
</script>

<style scoped>

</style>