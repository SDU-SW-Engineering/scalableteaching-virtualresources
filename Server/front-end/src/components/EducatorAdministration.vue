<template>
  <div>
    <b-container fluid="sm">

      <b-form>
        <b-form-group
            id="input-group-1"
            label="Email of machine manager:"
            label-for="input-1"
            description="Remaining data is populated on first sign in of user"
        >
          <b-form-input
              id="input-1"
              v-model="email"
              type="email"
              placeholder="Email of user"
              required
          >
          </b-form-input>
        </b-form-group>
        <b-button-group>
          <b-button pill @click="onSubmit" variant="success">Submit</b-button>
          <b-button pill @click="onReset" variant="secondary">Reset</b-button>
        </b-button-group>
      </b-form>

      <hr>
      <!--TODO: Implement action on button-->
      <b-button pill variant="secondary" @click="loadTableData">Refresh</b-button>
      <b-button pill variant="danger" v-if="selectedRow.length !== 0" @click="deprivilegeSelectedUser">Remove Selected
        User
      </b-button>
      <hr>
      <b-table
          sticky-header="true"
          striped
          selectable
          select-mode="single"
          :items="users"
          :fields="fields"
          @row-selected="onRowSelected"
          :busy="tableIsLoading"
      ></b-table>
    </b-container>
  </div>
</template>

<script>
import EducatorAPI from "@/api/EducatorAPI";

export default {
  name: "EducatorAdministration",
  mounted() {
    this.loadTableData()
  },
  data() {
    return {
      email: '',
      selectedRow: [],
      tableIsLoading: true,
      fields: [
        {
          key: 'sn',
          label: 'Surname',
          sortable: true
        },
        {
          key: 'gn',
          label: 'First Name',
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
      users: []
    }
  },
  methods: {
    async loadTableData() {
      this.resetFields()
      this.tableLoading(true)
      this.users = []
      //Get all groups
      const result = await EducatorAPI.getEducators();
      //If successful
      if (result.status === 200) {
        for (let i = 0; i < result.body.length; i++) {
          this.users.push({
            uname: result.body[i].username,
            sn: result.body[i].surname,
            gn: result.body[i].generalName,
            email: result.body[i].mail,
          })
        }
      }
      this.tableLoading(false)
    },
    onRowSelected(items) {
      this.selectedRow = items
    },
    async onSubmit() {
      console.log("OnSubmit: " + this.email)
      if (this.email.match(/^[A-Za-z0-9]{1,10}@[a-zA-Z0-9]*\.sdu\.dk$/g) !== null)
        console.log("APIReturn: " + (await EducatorAPI.postEducator(this.email)))
    },
    resetFields() {
      this.email = ''
      this.selectedRow = []
    },
    onReset(event) {
      event.preventDefault()
      this.resetFields()
    },
    deprivilegeSelectedUser() {
      if (this.selectedRow.length < 1) {
        EducatorAPI.deleteEducator(this.selectedRow[0].email)
        this.resetFields()
      }
    },
    tableLoading(state) {
      if (state === true) {
        this.tableIsLoading = true
      } else if (state === false) {
        this.tableIsLoading = false
      } else {
        this.tableIsLoading = !this.tableIsLoading
      }
    },
  }
}
</script>

<style scoped>

</style>