<template>
  <div>
    <b-container fluid="sm">

      <b-form @submit="onSubmit" @reset="onReset">
        <b-form-group
            id="input-group-1"
            label="Email of machine manager:"
            label-for="input-1"
            description="Remaining data is populated on first sign in of user"
        >
          <b-form-input
              id="input-1"
              v-model="form.email"
              type="email"
              placeholder="Email of user"
              required
          >
          </b-form-input>
        </b-form-group>

        <b-form-group id="input-group-4" v-slot="{ ariaDescribedby }">
          <b-form-checkbox-group
              v-model="form.checked"
              id="checkboxes-4"
              :aria-describedby="ariaDescribedby"
          >
          </b-form-checkbox-group>
        </b-form-group>
        <b-button-group>
          <b-button pill type="submit" variant="success">Submit</b-button>
          <b-button pill type="reset" variant="secondary">Reset</b-button>
        </b-button-group>
      </b-form>

      <hr>
      <!--TODO: Implement action on button-->
      <b-button pill variant="danger">Remove Selected User</b-button>
      <hr>
      <!--TODO: Implement busy state https://bootstrap-vue.org/docs/components/table#table-busy-state-->
      <b-table sticky-header striped selectable select-mode="single" :items="users" :fields="fields"></b-table>
      <!--TODO: Use the selction for -->
    </b-container>
  </div>
</template>

<script>
export default {
  name: "UserAdministration",
  data() {
    return {
      form: {
        email: ''
      },
      fields: [
        {
          key: 'gn',
          label: 'First Name',
          sortable: true
        },
        {
          key: 'sn',
          label: 'Surname',
          sortable: true
        },
        {
          key: 'cn',
          label: 'Full Name',
          sortable: true
        },
        {
          key: 'uname',
          label: 'Username',
          sortable: true
        },
        {
          key: 'email',
          label: 'Email',
          sortable: true
        }
      ],
      users: [
        {
          gn: "Test Man",
          sn: "User",
          cn: "Test Man User",
          uname: "temus20",
          account_type: "Administrator",
          email: "temus20@student.sdu.dk"
        },
        {
          gn: "Test Whoman",
          sn: "User",
          cn: "Test Man User",
          uname: "tewus20",
          account_type: "Administrator",
          email: "tewus20@student.sdu.dk"
        }
      ]
    }
  },
  methods: {
    onSubmit(event) {
      event.preventDefault()
      //TODO: Do something with the form data
      //JSON.stringify(this.form)
    },
    onReset(event) {
      event.preventDefault()
      // Reset our form values
      this.form.email = ''
      // Trick to reset/clear native browser form validation state
    }
  }
}
</script>

<style scoped>

</style>