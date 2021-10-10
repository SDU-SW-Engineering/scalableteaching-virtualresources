<template>
  <b-container id="PerUserMachineConfiguration">
    <!--Per User - Upload or create groups-->
    <b-row align-content="center" id="PerUser_MachineName">
      <b-col md="11">
        <b-form-group>
          <b-form-input
              v-model="machineNamingDirective"
              placeholder="Enter the name for the machine."
              type="text"
              :state="validateMachineName()"
          ></b-form-input>
        </b-form-group>
      </b-col>
    </b-row>
    <b-row align-content="center" id="PerUser_UsernamesAndGroups">
      <b-col md="6">
        <b-form-group label="Enter or upload list of usernames">
          <b-form-textarea
              id="textarea"
              v-model="enteredUsersField"
              placeholder="Enter a list of usernames separated by linebreaks, commas or spaces."
              rows="3"
              max-rows="6"
              v-if="!useUsersFile"
              :key="useUsersFile"
              :state="validateUsers()"
              debounce="300"
          ></b-form-textarea>
          <b-form-file
              v-if="useUsersFile"
              :key="useUsersFile"
              v-model="usersFile"
              :state="Boolean(usersFile) && validateUsers()"
              accept="text/csv"
              v-on:input="debugText=2"
          ></b-form-file><!--TODO: Implement validation of users file on change of file-->

          <b-form-radio-group
              v-model="useUsersFile"
              :options="useUsersFileOptions"
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
              v-model="linuxGroupsField"
              placeholder="List of groups separated by linebreaks, commas or spaces."
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
        <label>Memory Amount: {{ MemoryRangeValue }}</label>
        <input
            type="range"
            min="1024"
            max="8192"
            v-model="MemoryRangeValue"
            step="1024"
        >
      </b-col>
      <b-col>
        <lable>VCPU Count: {{ VCPURangeValue }}</lable>
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
import StringHelper from "@/helpers/StringHelper";
import MachineCreationValidationHelper from "../../helpers/MachineCreationValidationHelper.ts";

export default {
  name: "SingleMachineCreation",
  props: ['classObject'],
  data() {
    return {
      MemoryRangeValue: "1024",
      VCPURangeValue: "1",
      StorageRangeValue: "30720",
      linuxGroupsField: "",
      ppaField: "",
      aptField: "",
      portsField: "",
      enteredUsersField: "",
      useUsersFile: true,
      usersFile: null,
      useUsersFileOptions: [
        {text: "Enter usernames", value: false},
        {text: "Upload file containing usernames", value: true}
      ],
      machineNamingDirective: "",
    };
  },
  methods: {
    getMachinesToBeCreated() {
      //Machines to be returned
      let machines = [];
      //Intermediate variable extraction
      let ports = [];
      StringHelper.breakStringIntoTokenList(this.portsField).forEach(portToken => ports.push(parseInt(portToken)));
      //Machine list population
      machines.push({
        hostname: this.machineNamingDirective,
        users: StringHelper.breakStringIntoTokenList(this.useUsersFile ? this.usersFile : this.enteredUsersField),
        apt: StringHelper.breakStringIntoTokenList(this.aptField),
        ppa: StringHelper.breakStringIntoTokenList(this.ppaField),
        ports: ports,
        linuxgroups: StringHelper.breakStringIntoTokenList(this.linuxGroupsField),
        courseid: this.classObject.courseID,
        memory: parseInt(this.MemoryRangeValue, 10),
        vcpu: parseInt(this.VCPURangeValue, 10),
        storage: parseInt(this.StorageRangeValue, 10)
      });
      return machines;
    },
    isValidAndComplete() {
      return MachineCreationValidationHelper.isValidAndComplete(
          this.validateMachineName(),
          this.validateUsers(),
          this.portsField,
          this.linuxGroupsField,
          this.aptField,
          this.ppaField
      );
    },
    validateMachineName() {
      let name = this.machineNamingDirective;
      name = name.replace("%i", "00").replace("%g", "abcde01").replace("%s", "e01");
      let regex = /^(([a-zA-Z]|[a-zA-Z][a-zA-Z0-9-]*[a-zA-Z0-9])\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9-]*[A-Za-z0-9])$/;
      return name.search(regex) !== -1;
    },
    validateUsers() {
      let usersString = "";
      if (this.useUsersFile) {
        usersString = this.usersFile;
      } else {
        usersString = this.enteredUsersField;
      }
      return MachineCreationValidationHelper.validateUsers(usersString);
    },
    validateGroups() {
      return MachineCreationValidationHelper.validateGroups(this.linuxGroupsField);
    },
    validatePPA() {
      return MachineCreationValidationHelper.validatePPA(this.ppaField);
    },
    validateAPT() {
      return MachineCreationValidationHelper.validateAPT(this.aptField);
    },
    validatePorts() {
      return MachineCreationValidationHelper.validatePorts(this.portsField);
    }
  }
};
</script>

<style scoped>

</style>