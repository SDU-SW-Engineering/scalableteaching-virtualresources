<template>
  <div>
    <!--Top of screen buttons-->
    <b-row>
      <b-col>
        <b-button class="mb4 mr4" :disabled="creationStep < 1" v-on:click="creationStep--">←
        </b-button>
        <b-button class="mb4 mr2" variant="warning" v-on:click="resetVerification">Reset Machine Creation Forms
        </b-button>
        <b-button class="mb4 mr2" v-if="creationStep===3" variant="primary" v-on:click="finish">Create Machines
        </b-button>
        <b-button class="mb4 ml2" :disabled="!canNextCreationStep()"
                  v-on:click="creationStep++"
        >→
        </b-button>
      </b-col>
    </b-row>
    <!--Class Name Selection And Input-->
    <b-row v-if="creationStep === 0">
      <b-col md="6" offset-md="3" align-self="center">
        <b-form-group label="Select or input class name">
          <b-form-select
              v-model="settings.classname.selected"
              :options="classnameOptions"
              :state="settings.classname.selected !== null"
          >
          </b-form-select>
        </b-form-group>
      </b-col>
    </b-row>
    <!--Machine Replication Directive-->
    <b-row v-if="(creationStep === 1)">
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

    <SingleMachineCreation
        v-if="creationStep === 2 && settings.replicationDirective.selected === 0"
        ref="SingleMachineCreation"
    />

    <PerGroupMachineCreation
        v-if="creationStep === 2 && settings.replicationDirective.selected === 1"
        ref="PerGroupMachineCreation"
        v-bind:classObject="settings.classname.selected"
    />

    <PerUserMachineCreation
        v-if="creationStep === 2 && settings.replicationDirective.selected === 2"
        ref="PerUserMachineCreation"
    />

    <EndOfCreationTable
        v-if="creationStep === 3"
        ref="EndOfCreationTable"
        v-bind:machinesToBeCreated="getMachinesToBeCreated()"
    />

  </div>
</template>

<script>
import PerGroupMachineCreation from "@/components/MachineCreationComponents/PerGroupMachineCreation";
import PerUserMachineCreation from "@/components/MachineCreationComponents/PerUserMachineCreation";
import EndOfCreationTable from "@/components/MachineCreationComponents/EndOfCreationTable";
import CourseAPI from "@/api/CourseAPI";
import SingleMachineCreation from "@/components/MachineCreationComponents/SingleMachineCreation";

export default {
  name: "MachineCreation",
  components: {SingleMachineCreation, PerUserMachineCreation, PerGroupMachineCreation},
  mounted() {
    this.updateClassSelectionList()
  },
  data() {
    return {
      creationStep: 0,
      maxCreationStep: 3,
      resetBox: '',
      debugText: 'No Debug Text Yet',
      classnameOptions: [],
      settings: {
        machinesToBeCreated: {
          items: [
            {
              machineName: 'ONK-frhou18-01-E21',
              courseName: 'Onk Not Known',
              users: ['frhou18'],
              groups: ['sudo', 'onk'],
              ports: [1234, 8456],
              PPAs: ['ppa:kubuntu-ppa/staging-plasma'],
              APTs: ['vim', 'git']
            }
          ],
          fields: [
            {key: 'machineName', label: 'Machine Name', sortable: true},
            {key: 'users', label: 'Users', sortable: false},
            {key: 'details', label: 'Details', sortable: false}
          ],
        },
        replicationDirective: {
          options: [
            {text: "Single Machine", value: 0},
            {text: "Per Group", value: 1},
            {text: "Per User", value: 2},
          ],
          selected: null,
        },
        sharedMachineAmount: {
          options: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10],
          selected: 1,
        },
        classname: {
          selected: null,
        }
      },

    }
  },
  methods: {
    canNextCreationStep() {
      if (this.creationStep === 0 && this.settings.classname.selected !== null) return true
      if (this.creationStep === 1 && this.settings.replicationDirective !== null) return true
      if (this.creationStep === 2) return true
    },
    machineCustomizationValid() {
      if (this.settings.replicationDirective.selected === 0) {
        this.$refs.SingleMachineCreation.isValidAndComplete()
      }
      if (this.settings.replicationDirective.selected === 1) {
        this.$refs.PerGroupMachineCreation.isValidAndComplete()
      }
      if (this.settings.replicationDirective.selected === 2) {
        this.$refs.PerUserMachineCreation.isValidAndComplete()
      }
    },
    getMachinesToBeCreated() {
      if (this.settings.replicationDirective.selected === 0) {
        this.$refs.SingleMachineCreation.getMachinesToBeCreated()
      }
      if (this.settings.replicationDirective.selected === 1) {
        this.$refs.PerGroupMachineCreation.getMachinesToBeCreated()
      }
      if (this.settings.replicationDirective.selected === 2) {
        this.$refs.PerUserMachineCreation.getMachinesToBeCreated()
      }
    },
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
      this.creationStep = 0;
      this.debugText = "No Debug Text Yet";
      this.resetBox = "";
      this.settings.replicationDirective.selected = 0;
      this.settings.sharedMachineAmount.selected = 1;
      this.settings.classname = {
        selected: "null",
        newName: ""
      }
    },
    finish() {
      //TODO: Verify all data entered correctly and submit to backend.
    },
    async updateClassSelectionList() {
      this.classnameOptions = []
      let retrievedClassNames = await CourseAPI.getCourses()
      retrievedClassNames.body.forEach(course => {
        this.classnameOptions.push({
          value: course,
          text: course.courseName
        })
      })
    },
  },
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