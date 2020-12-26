<template>
  <div>
    <!--Top of screen buttons-->
    <b-row>
      <b-col>
        <b-button class="mb4 mr4" :disabled="creationStep <= 1" v-on:click="creationStep--">←
        </b-button>
        <b-button class="mb4 mr2" variant="warning" v-on:click="resetVerification">Reset Machine Creation Forms
        </b-button>
        <b-button class="mb4 mr2" variant="info" v-on:click="advanced = !advanced">Advanced Mode</b-button>
        <b-button class="mb4 ml2" :disabled="creationStep >= finalCreationStep"
                  v-on:click="creationStep++">→
        </b-button>
      </b-col>
    </b-row>
    <!--Class Name Selection And Input-->
    <b-row v-if="creationStep===0 || advanced">
      <b-col xl="6" offset-xl="3" align-self="center">
        <b-form-group label="Select or input class name">
          <b-form-select
              v-model="settings.classname.selected"
              :options="getClassnameOptions"
          >
            <b-form-select-option value="null">Class not on list</b-form-select-option>
          </b-form-select>
          <div id="ClassnameInput">
            <b-form-input
                v-model="settings.classname.newName"
                placeholder="Enter new class name here"
                :disabled.sync="settings.classname.selected !== 'null' "
            ></b-form-input>
          </div>
          <b-tooltip target="ClassnameInput" triggers="hover" :disabled.sync="settings.classname.selected === 'null'">To enable select "Class not on list" from above</b-tooltip>
        </b-form-group>
      </b-col>
    </b-row>
    <!--Machine Creation Directive-->
    <b-row v-if="(creationStep=== 1 || advanced)">
      <b-col>
        <b-form-group label="Select replication directive">
          <b-form-radio-group
              id="CreationDirectiveSelection"
              v-model="settings.replicationDirective.selected"
              :options="settings.replicationDirective.options"
              name="CreationDirectiveSelection"
          ></b-form-radio-group>
        </b-form-group>
      </b-col>
    </b-row>
    <!--Machine amount-->
    <!--Multiple Shared-->
    <b-row v-if="(creationStep === 2 || advanced) && settings.replicationDirective.selected === 1"
           align-content="center">
      <b-col xl="6" offset-xl="3" align-self="center">
        <b-form-group label="Select amount of share machines">
          <b-form-select
              v-model="settings.sharedMachineAmount.selected"
              :options="settings.sharedMachineAmount.options"
          ></b-form-select>
        </b-form-group>
      </b-col>
    </b-row>
    <!--Per Group - Upload or create groups-->
    <b-row v-if="(creationStep === 2 || advanced) && settings.replicationDirective.selected === 2"
           align-content="center">
      <b-col xl="6" offset-xl="3" align-self="center">
        <b-form-group label="Select groups or upload a file containing groups">
          <b-form-radio-group v-model="settings.groupMachines.useFile" :options="settings.groupMachines.options">
          </b-form-radio-group>
          <b-form-file
              v-if="settings.groupMachines.useFile"
              :key="settings.groupMachines.useFile"
              v-model="settings.groupMachines.file"
              :state="Boolean(settings.groupMachines.file)"
              accept="application/activity+json"
              v-on:input="debugText=2"
          ></b-form-file><!--TODO: Implement validation of group file on change of file-->
          <b-form-select
              v-if="!settings.groupMachines.useFile"
              :key="settings.groupMachines.useFile"
              v-model="settings.groupMachines.selectedGroups"
              :options="getGroupNames"
              multiple
          >
          </b-form-select>
        </b-form-group>
      </b-col>
    </b-row>
    <!--Per User - Upload or enter manually-->
    <b-row v-if="(creationStep === 2 || advanced) && settings.replicationDirective.selected === 3"
           align-content="center">
      <b-col xl="6" offset-xl="3" align-self="center">
        <b-form-group label="Upload list of students">
          <b-form-file
              v-model="settings.userMachines.file"
              :state="Boolean(settings.userMachines.file)"
              accept="application/activity+json"
          ></b-form-file><!--TODO: Implement validation of group file on change of file-->
        </b-form-group>
      </b-col>
    </b-row>
    <!--TODO: Select programs for installation-->

    <!--Bottom of screen buttons-->
    <b-row>
      <b-col>
        <b-button class="mb4 mr4" :disabled="creationStep <= 1" v-on:click="creationStep--">←
        </b-button>
        <b-button class="mb4 mr2" variant="warning" v-on:click="resetVerification">Reset Machine Creation Forms
        </b-button>
        <b-button class="mb4 mr2" v-if="creationStep===3" variant="primary" v-on:click="finish">Create Machines</b-button>
        <b-button class="mb4 mr2" variant="info" v-on:click="advanced = !advanced">Advanced Mode</b-button>
        <b-button class="mb4 ml2" :disabled="creationStep >= finalCreationStep"
                  v-on:click="creationStep++">→
        </b-button>
      </b-col>
    </b-row>
  </div>
</template>

<script>
export default {
  name: "MachineCreation",
  data() {
    return {
      creationStep: 0,
      advanced: false,
      finalCreationStep: 3,
      isDisabled: false,
      resetBox: '',
      debugText: 'No Debug Text Yet',
      settings: {
        replicationDirective: {
          options: [
            {text: "Single Machine", value: 0},
            {text: "Multiple Shared", value: 1},
            {text: "Per Group", value: 2},
            {text: "Per User", value: 3},
          ],
          selected: 0,
        },
        sharedMachineAmount: {
          options: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10],
          selected: 1,
        },
        groupMachines: {
          file: "",
          selectedGroups: [],
          useFile: false,
          options: [
            {text: "Select Existing Groups", value:false},
            {text: "Upload File for New Groups", value:true}
          ]
        },
        userMachines: {},
        classname: {
          selected: "null",
          newName: "",
        }
      }
    }
  },
  methods: {
    resetVerification() {
      //Reset forms
      this.resetBox = '';
      this.$bvModal.msgBoxConfirm('Confirm deletion of every value in the machine creation fields', {
        title: 'Reset all values',
        size: 'md',
        buttonSize: 'md',
        okVariant: 'danger',
        okTitle: 'Delete Values',
        cancelTitle: 'Don\'t Delete Values',
        cancelVariant: 'outline-secondary',
        footerClass: 'p-2',
        hideHeaderClose: false,
        centered: true
      })
          .then(value => {
            this.debugText = "ResetValidation: " + value;
            this.resetFields();
          })
          .catch(err => {
            this.resetBox = "ResetValidationErr: " + err;
          })
    },
    resetFields() {
      this.creationStep = 1;
      //TODO: Implement field resets
    },
    finish(){
      //TODO: Verify all data entered correctly and submit to backend.
    }
  },
  computed: {
    console: () => console,
    getClassnameOptions() {
      //TODO: Implement class return
      return [
        {text: "ONK", value: "ONK"},
        {text: "ADT", value: "ADT"},
        {text: "YAO", value: "YAO"},
        {text: "MUU", value: "MUU"}
      ]
    },
    getGroupNames() {
      //TODO: Implement The return of groups
      return [
        {text: "Group 1", value: "411c4f02-04a0-45fb-ba3b-fbe50784f592"},
        {text: "Group 2", value: "b3f5aaa7-8747-4c07-8f5b-3db6c833b90a"},
        {text: "Group 3", value: "2dff4822-79b1-40ff-85b9-185d5439384b"},
        {text: "Group 4", value: "734da026-013e-4b3c-9c35-62729fe60832"}
      ];
    },
  }
}

</script>

<style scoped lang="scss">
$sizes: 12;

@mixin margin-classes {
  @for $i from 1 through $sizes {
    $margin: $i * 0.25rem;
    /* margin #{$margin} */
    .m#{$i} {
      margin: $margin;
    }
    .ml#{$i} {
      margin-left: $margin;
    }
    .mr#{$i} {
      margin-right: $margin;
    }
    .mt#{$i} {
      margin-top: $margin;
    }
    .mb#{$i} {
      margin-bottom: $margin;
    }
    .mx#{$i} {
      margin-left: $margin;
      margin-right: $margin;
    }
    .my#{$i} {
      margin-top: $margin;
      margin-bottom: $margin;
    }
  }
}

@include margin-classes;


@mixin padding-classes {
  @for $i from 1 through $sizes {
    $padding: $i * 0.25rem;
    /* padding #{$padding} */
    .p#{$i} {
      padding: $padding;
    }
    .pl#{$i} {
      padding-left: $padding;
    }
    .pr#{$i} {
      padding-right: $padding;
    }
    .pt#{$i} {
      padding-top: $padding;
    }
    .pb#{$i} {
      padding-bottom: $padding;
    }
    .px#{$i} {
      padding-left: $padding;
      padding-right: $padding;
    }
    .py#{$i} {
      padding-top: $padding;
      padding-bottom: $padding;
    }
  }
}

@include padding-classes;
</style>