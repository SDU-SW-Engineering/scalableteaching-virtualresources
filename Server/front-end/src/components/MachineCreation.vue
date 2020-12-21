<template>
<div>
  <!--Reset Machine Modal-->
<!--  <b-modal
      id="reset-modal"
      title="Do you really want to reset"
      @cancel="$bvModal.hide('reset-modal')">
    <template #modal-footer="{ hide }">
      &lt;!&ndash; Emulate built in modal footer ok and cancel button actions &ndash;&gt;
      <b-button size="md" variant="danger" @click="hide('resetVerification()')">
        Yes Clear Forms
      </b-button>
      &lt;!&ndash; Button with custom close trigger value &ndash;&gt;
      <b-button size="md" variant="outline-secondary" @click="$bvModal.hide('reset-modal')">
        Cancel
      </b-button>
    </template>

    <p>This action will clear all machine creation fields, and cannot be undone</p>
  </b-modal>-->
  <!--Top of screen buttons-->
  <b-row>
    <b-col>
      <b-button class="mb4 mr4" :disabled="creationStep <= 1" v-on:click="creationStep--" >Previous</b-button>
      <b-button class="mb4 mr2" variant="warning" v-on:click="resetVerification">Reset Machine Creation Forms</b-button>
      <b-button class="mb4 mr2" variant="info">Advanced Mode</b-button>
      <b-button class="mb4 ml2" :disabled="creationStep >= finalCreationStep" v-on:click="creationStep++" >Next</b-button>
    </b-col>
  </b-row>
  <!--Machine amount creation-->


<p>Debug Text: {{resetBox}}</p>
</div>
</template>

<script>
  export default {
    name: "MachineCreation",
    data(){
      return {
        creationStep: 1,
        finalCreationStep:15,
        isDisabled: false,
        resetBox: '',
        debugText: 'No Debug Text Yet',
      }
    },
    methods: {
      resetVerification(){
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
            this.debugText = "ResetValidation: "+value;
            this.resetFields();
          })
        .catch(err => {
          this.resetBox = "ResetValidationErr: "+err;
        })
      },
      resetFields(){
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
      .m#{$i}  {margin: $margin;}
      .ml#{$i} {margin-left: $margin;}
      .mr#{$i} {margin-right: $margin;}
      .mt#{$i} {margin-top: $margin;}
      .mb#{$i} {margin-bottom: $margin;}
      .mx#{$i} {margin-left: $margin; margin-right: $margin;}
      .my#{$i} {margin-top: $margin; margin-bottom: $margin;}
    }
  }
  @include margin-classes;


  @mixin padding-classes {
    @for $i from 1 through $sizes {
      $padding: $i * 0.25rem;
      /* padding #{$padding} */
      .p#{$i} {padding: $padding;}
      .pl#{$i} {padding-left: $padding;}
      .pr#{$i} {padding-right: $padding;}
      .pt#{$i} {padding-top: $padding;}
      .pb#{$i} {padding-bottom: $padding;}
      .px#{$i} {padding-left: $padding; padding-right: $padding;}
      .py#{$i} {padding-top: $padding; padding-bottom: $padding;}
    }
  }
  @include padding-classes;
</style>