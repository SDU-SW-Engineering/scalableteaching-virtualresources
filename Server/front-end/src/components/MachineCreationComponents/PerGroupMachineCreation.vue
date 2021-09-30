<template>
  <b-container id="PerUserMachineConfiguration">
    <!--Per User - Upload or create groups-->
    <b-row align-content="center" id="PerGroup_MachineName">
      <b-col md="11">
        <b-form-group>
          <b-form-input
              v-model="machineNamingDirective"
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
              v-model="selectedGroups"
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
    <!--Machine resource customization-->
    <b-row>
      <b-col>
        <label>Memory Amount: {{MemoryRangeValue}}</label>
        <input
            type="range"
            min="1024"
            max="8192"
            v-model="MemoryRangeValue"
            step="1024"
          >
      </b-col>
      <b-col>
        <lable>VCPU Count: {{VCPURangeValue}}</lable>
        <input
            type="range"
            min="1"
            max="8"
            v-model="VCPURangeValue"
            step="1"
        >
      </b-col>
<!--      <b-col>
        <input
          type="range"
          min="30720"
          max="51200"
          step="1024"
          v-model="StorageRangeValue"
        >
      </b-col>-->
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
      MemoryRangeValue:"1024",
      VCPURangeValue:"1",
      StorageRangeValue:"30",
      linuxGroupsField: "",
      ppaField: "",
      selectedGroups: [null],
      machineNamingDirective: ""
    };
  },
  methods: {
    getMachinesToBeCreated() {
      //Machines to be returned
      let machines = [];
      //Intermediate variable extraction
      let groups = this.selectedGroups;
      let ports = [];
      StringHelper.breakStringIntoTokenList(this.portsField).forEach(portToken => ports.push(parseInt(portToken)));
      let apt = StringHelper.breakStringIntoTokenList(this.aptField);
      let ppa = StringHelper.breakStringIntoTokenList(this.ppaField);
      let linuxGroups = StringHelper.breakStringIntoTokenList(this.linuxGroupsField);

      for (let i = 0; i < groups.length; i++) {
        //Intermediate variable extraction
        let group = groups[i];
        //Machine list population
        machines.push({
          hostname: this.parseNamingDirectiveToMachineName(group.groupName, i, groups.length),
          group: group.groupId,
          apt: apt,
          ppa: ppa,
          ports: ports,
          linuxgroups: linuxGroups,
          courseid: this.classObject.courseID
          memory: parseInt(this.MemoryRangeValue, 10),
          vcpu: parseInt(this.VCPURangeValue, 10),
          storage: parseInt(this.StorageRangeValue, 10)
        });
      }
      return machines;
    },
    parseNamingDirectiveToMachineName(groupIndex, machineIndex, tokenCount) {
      //To keep names fixed length assuming names are a fixed length
      let number = ("000" + machineIndex.toString()).slice(-tokenCount.toString().length);
      let today = new Date();
      let letter = today.getMonth() < 6 ? "F" : "E";
      let year = today.getFullYear() % 100; // Get the two final digits of the year (yeah yeah epoch bla bla bla)
      let semesterValue = letter + year.toString();
      return this.machineNamingDirective.replaceAll("%i", number).replaceAll("%s", semesterValue).replaceAll("%g", ("g" + ("000" + groupIndex).slice(-2)));
    },
    isValidAndComplete() {
      let rv = true;
      rv = rv && this.validateMachineName();
      rv = rv && this.validateUsers();
      let portsValidity = this.validatePorts();
      let groupsValidity = this.validateGroups();
      let aptValidity = this.validateAPT();
      let ppaValidity = this.validatePPA();
      rv = rv && (portsValidity === null || portsValidity === true);
      rv = rv && (groupsValidity === null || groupsValidity === true);
      rv = rv && (aptValidity === null || aptValidity === true);
      rv = rv && (ppaValidity === null || ppaValidity === true);
      return rv;
    },
    validateMachineName() {
      let name = this.machineNamingDirective;
      name = name.replace("%i", "00").replace("%g", "g99").replace("%s", "e01");
      let regex = /^(([a-zA-Z]|[a-zA-Z][a-zA-Z0-9-]*[a-zA-Z0-9])\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9-]*[A-Za-z0-9])$/g;
      return name.search(regex) !== -1;
    },
    validateSelectedGroups() {
      let groups = this.selectedGroups;
      if (groups.length < 1 || (groups.length === 1 && groups[0] === null)) return false;
    },
    validateGroups() {
      if (this.linuxGroupsField.length === 0) return null;
      let cleanTokens = StringHelper.breakStringIntoTokenList(this.linuxGroupsField);
      for (let i = 0; i < cleanTokens.length; i++) {
        let token = cleanTokens[i];
        if (token.length > 0) {
          if (token.match(/^(([a-zA-Z]|[a-zA-Z][a-zA-Z0-9-]*[a-zA-Z0-9])\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9-]*[A-Za-z0-9])$/) === null) {
            return false;
          }
        }
      }
      return true;
    },
    validatePPA() {
      if (this.ppaField.length === 0) return null;
      let cleanTokens = StringHelper.breakStringIntoTokenList(this.ppaField);
      for (let i = 0; i < cleanTokens.length; i++) {
        let token = cleanTokens[i];
        if (token.length > 0) {
          if (token.match(/^(ppa:([a-z-]+)\/[a-z-]+)$/) === null) {
            return false;
          }
        }
      }
      return true;
    },
    validateAPT() {
      if (this.aptField.length === 0) return null;
      let cleanTokens = StringHelper.breakStringIntoTokenList(this.aptField);
      for (let i = 0; i < cleanTokens.length; i++) {
        let token = cleanTokens[i];
        if (token.length > 0) {
          if (token.match(/[0-9A-Za-z.+-]+/) === null) {
            return false;
          }
        }
      }
      return true;
    },
    validatePorts() {
      if (this.portsField.length === 0) return null;
      let cleanTokens = StringHelper.breakStringIntoTokenList(this.portsField);
      for (let i = 0; i < cleanTokens.length; i++) {
        let token = cleanTokens[i];
        if (token.length > 0) {
          if (!(token.match(/[0-9]{1,5}/) !== null && (parseInt(token) > 0 && parseInt(token) <= 65535)))
            return false;
        }
      }
      return true;
    },
    async updateGroupList() {
      this.groupSelectionOptions = [];
      this.selectedGroups = [];
      let groupsResponse = await GroupAPI.getGroupsByCourseID(this.classObject.courseID);
      if (groupsResponse.body === undefined) return;
      for (let i = 0; i < groupsResponse.length; i++) {
        let group = groupsResponse[i];
        this.groupSelectionOptions.push({
          value: group,
          text: group.groupName
        });
      }
    }

  },
  watch: {
    // eslint-disable-next-line no-unused-vars
    classObject: function (newVal, oldVal) {
      this.updateGroupList();
    }
  }
};
</script>

<style scoped>

</style>