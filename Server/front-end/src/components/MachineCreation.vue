<template>
  <div>
    <!--Top of screen buttons-->
    <b-row>
      <b-col>
        <b-button class="mb4 mr4" v-if="!advanced" :disabled="creationStep <= 1" v-on:click="creationStep--">Previous
        </b-button>
        <b-button class="mb4 mr2" variant="warning" v-on:click="resetVerification">Reset Machine Creation Forms
        </b-button>
        <b-button class="mb4 mr2" variant="info" v-on:click="advanced = !advanced">Advanced Mode</b-button>
        <b-button class="mb4 ml2" v-if="!advanced" :disabled="creationStep >= finalCreationStep"
                  v-on:click="creationStep++">Next
        </b-button>
      </b-col>
    </b-row>

    <!--Machine Creation Directive-->
    <b-row v-if="(creationStep===1 || advanced)">
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
    <b-row v-if="(creationStep===2 || advanced) && settings.replicationDirective.selected === 1" align-content="center">
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

    <!--Per User - Upload or enter manually-->


    <p>Debug Text: {{ debugText }}</p>
    <p>Settings: {{ settings }}</p>
    <p>Step: {{ creationStep }}</p>
  </div>
</template>

<script>
export default {
  name: "MachineCreation",
  data() {
    return {
      creationStep: 1,
      advanced: false,
      finalCreationStep: 15,
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
          selected: "",
        },
        sharedMachineAmount: {
          options: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10],
          selected: 1,
        },
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
    }
  },
  computed: {
    console: () => console,
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