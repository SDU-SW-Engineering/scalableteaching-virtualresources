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
                  v-on:click="nextCreationStep"
        >→
        </b-button>
      </b-col>
    </b-row>
    <!--Class Name Selection And Input-->
    <b-row v-if="creationStep === 0">
      <b-col md="6" offset-md="3" align-self="center">
        <b-form-group label="Select or input class name">
          <b-form-select
              v-model="classname.selected"
              :options="classnameOptions"
              :state="classname.selected !== null"
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
              v-model="replicationDirective.selected"
              :options="replicationDirective.options"
              name="CreationDirectiveSelection"
          ></b-form-radio-group>
        </b-form-group>
      </b-col>
    </b-row>

    <SingleMachineCreation
        v-if="creationStep === 2 && replicationDirective.selected === 0"
        ref="SingleMachineCreation"
        v-bind:classObject="classname.selected"
    />

    <PerGroupMachineCreation
        v-if="creationStep === 2 && replicationDirective.selected === 1"
        ref="PerGroupMachineCreation"
        v-bind:classObject="classname.selected"
    />

    <PerUserMachineCreation
        v-if="creationStep === 2 && replicationDirective.selected === 2"
        ref="PerUserMachineCreation"
        v-bind:classObject="classname.selected"
    />

    <EndOfCreationTable
        v-if="creationStep === 3"
        ref="EndOfCreationTableRef"
        v-bind:machineSettings="parseToTable"
    />

  </div>
</template>

<script>
import PerGroupMachineCreation from "@/components/MachineCreationComponents/PerGroupMachineCreation";
import PerUserMachineCreation from "@/components/MachineCreationComponents/PerUserMachineCreation";
import EndOfCreationTable from "@/components/MachineCreationComponents/EndOfCreationTable";
import CourseAPI from "@/api/CourseAPI";
import SingleMachineCreation from "@/components/MachineCreationComponents/SingleMachineCreation";
import MachineCreationAPI from "@/api/MachineCreationAPI";

export default {
  name: "MachineCreation",
  components: {SingleMachineCreation, PerUserMachineCreation, PerGroupMachineCreation, EndOfCreationTable},
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
      parseToTable: {
        machinesToBeCreatedList: [],
        isGroupBased: false,
      },
      replicationDirective: {
        options: [
          {text: "Single Machine", value: 0},
          {text: "Per Group", value: 1},
          {text: "Per User", value: 2},
        ],
        selected: null,
      },
      classname: {
        selected: null,
      }
    }
  },
  methods: {
    nextCreationStep() {
      if (this.creationStep < 2) this.creationStep++
      else if (this.creationStep === 2 && this.machineCustomizationValid()) {
        this.parseToTable.machinesToBeCreatedList = this.getMachinesToBeCreated()
        this.parseToTable.isGroupBased = this.replicationDirective.selected === 1
        this.creationStep++
      }
    },
    canNextCreationStep() {
      if (this.creationStep === 0 && this.classname.selected !== null) return true
      if (this.creationStep === 1 && this.replicationDirective.selected !== null) return true
      if (this.creationStep === 2) return true
    },
    machineCustomizationValid() {
      if (this.replicationDirective.selected === 0) {
        return this.$refs.SingleMachineCreation.isValidAndComplete()
      }
      if (this.replicationDirective.selected === 1) {
        return this.$refs.PerGroupMachineCreation.isValidAndComplete()
      }
      if (this.replicationDirective.selected === 2) {
        return this.$refs.PerUserMachineCreation.isValidAndComplete()
      }
    },
    getMachinesToBeCreated() {
      if (this.replicationDirective.selected === 0) {
        return this.$refs.SingleMachineCreation.getMachinesToBeCreated()
      }
      if (this.replicationDirective.selected === 1) {
        return this.$refs.PerGroupMachineCreation.getMachinesToBeCreated()
      }
      if (this.replicationDirective.selected === 2) {
        return this.$refs.PerUserMachineCreation.getMachinesToBeCreated()
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
      this.replicationDirective.selected = 0;
      this.classname = {
        selected: "null",
        newName: ""
      }
    },
    async finish() {
      let machinesToBeCreated = this.$refs.EndOfCreationTableRef.getMachinesToBeCreated()
      let result = await MachineCreationAPI.createMachines(machinesToBeCreated)

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