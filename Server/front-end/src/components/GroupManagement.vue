<template>
  <div>
    <b-container fluid="sm">

      <b-form @submit="onSubmit" @reset="resetFields" autocomplete="false">

        <b-form-group
            id="input-group-1"
            label="Associated Course"
            label-for="input-1"
        >
          <b-form-select
              v-on:change="updateFilter"
              autocomplete="false"
              v-model="form.selectedCourse"
              :options="selectableCourses"
              onload="getSelectableCourses()"
              placeholder="Associated Course"
              v-b-tooltip.hover title="Associated Course"
              aria-describedby="coursename-live-feedback"
              :state="validateFormItem('course')"
          ></b-form-select>

          <b-form-invalid-feedback id="coursename-live-feedback">
            You must select a course
          </b-form-invalid-feedback>
        </b-form-group>

        <b-form-group
            id="input-group-1"
            label="Group Name"
            label-for="input-1"
        >
          <b-form-input
              autocomplete="false"
              id="input-groupname"
              v-model="form.groupname"
              type="text"
              placeholder="Name of the group"
              v-b-tooltip.hover title="Name of the group"
              :state="validateFormItem('groupname')"
              aria-describedby="groupname-live-feedback"
              required
          ></b-form-input>


          <b-form-invalid-feedback id="groupname-live-feedback">
            You must enter a group name
          </b-form-invalid-feedback>
        </b-form-group>

        <b-button-group>
          <b-button pill type="submit" variant="success">Submit</b-button>
          <b-button pill type="reset" variant="secondary">Reset</b-button>
        </b-button-group>
        <p style="color:#ff0000" v-if="formInvalidResponse">An error occurred</p>
      </b-form>

      <hr>
      <b-row>
        <b-col>
          <b-button pill v-on:click="loadTableData">Refresh Table</b-button>
        </b-col>
        <b-col>
          <b-button pill variant="danger" v-b-modal.remove-group-modal v-if="selectedRow.length >= 1">Remove Selected
            Group
          </b-button>
        </b-col>
      </b-row>
      <b-modal id="remove-group-modal" title="Confirm Removal" hide-footer>
        <p>IF YOU REMOVE A GROUP <span style="color:red; font-weight: bold;">ALL MACHINES</span> ASSOCIATED WITH THE
          GROUP WILL <span style="color:red; font-weight: bold;">IMMEDIATELY BE DELETED </span></p>
        <b-button style="margin: 0 5px 0 0" variant="secondary" @click="$bvModal.hide('remove-group-modal')">Cancel
        </b-button>
        <b-button variant="outline-danger" v-on:click="removeSelectedCourse">Yes Remove Course</b-button>
      </b-modal>

      <hr>
      <b-table
          sticky-header="true"
          striped
          selectable
          select-mode="single"
          :items="groups"
          :fields="fields"
          @row-selected="onRowSelected"
          v-on:load="loadTableData"
          :busy="tableIsLoading"
          :filter="tableFilter"
          :filter-included-fields="courseName"
      ></b-table>
    </b-container>
  </div>
</template>

<script>
import CourseAPI from "@/api/CourseAPI";
import GroupAPI from "@/api/GroupAPI";

export default {
  name: "GroupManagement",
  mounted() {
    this.getSelectableCourses()
    this.loadTableData()
  },
  data() {
    return {
      tableFilter: null,
      tableIsLoading: true,
      form: {
        groupname: '',
        selectedCourse: null,
      },
      defaultSelectedCourse: null,
      defaultSelectableCourses: [{value: null, text: 'Select a course'}],
      selectableCourses: [{value: null, text: 'Select a course'}],
      formInvalidResponse: false,
      fields: [
        {
          key: 'courseName',
          label: 'Course Name',
          sortable: true
        },
        {
          key: 'groupName',
          label: 'Group Name',
          sortable: true
        }
      ],
      groups: [],
      selectedRow: [],
    }
  },
  methods: {
    tableLoading(state) {
      if (state === true) {
        this.tableIsLoading = true
      } else if (state === false) {
        this.tableIsLoading = false
      } else {
        this.tableIsLoading = !this.tableIsLoading
      }
    },
    async loadTableData() {
      this.resetFields()
      this.tableLoading(true)
      this.groups = []
      //Get all groups
      const result = await GroupAPI.getGroups();
      //If successfull
      if (result.status === 200) {
        for (let i = 0; i < result.body.length; i++) {
          console.log("Group: ", result.body[i])
          //Add coursename to each group
          const resultCourse = await CourseAPI.getCourse(result.body[i].courseID)
          result.body[i].courseName = resultCourse.body.courseName;
          //Add group members to each group
          const resultMembers = await GroupAPI.getGroupMembers(result.body[i].groupID);
          if (result.status === 200) {
            result.body[i].members = resultMembers.body
            this.groups.push(result.body[i])
          } else {
            result.body[i].members = []
            this.groups.push(result.body[i])
          }
        }
      }
      this.tableLoading(false)
    },
    onRowSelected(items) {
      this.selectedRow = items
      if (this.selectedRow.length > 0) {
        this.form.selectedCourse = this.selectedRow[0].courseID
        this.form.groupname = this.selectedRow[0].groupName
      } else {
        this.resetFields()
      }
    },
    async removeSelectedCourse() {
      await GroupAPI.deleteGroup(this.selectedRow[0].groupID)
    },
    validateFormItem(item) {
      console.log(this.form)
      switch (item) {
        case 'course':
          return this.form.selectedCourse !== null && this.form.selectedCourse !== undefined
        case 'groupname':
          return this.form.groupname.length > 0
        default:
          return false;
      }
    },
    async onSubmit(event) {
      let respOk = 0;
      event.preventDefault()

      if (this.selectedRow.length > 0) {
        console.log("Put", this.form.groupname, this.form.selectedCourse, this.selectedRow[0].groupID)
        let resp = await GroupAPI.putGroup(this.form.groupname, this.form.selectedCourse, this.selectedRow[0].groupID)
        respOk = resp === 204
      } else {
        let resp = await GroupAPI.postGroup(this.form.groupname, this.form.selectedCourse)
        respOk = resp === 200
      }
      this.formInvalidResponse = !respOk
      if (respOk) {
        this.resetFields()
        await this.loadTableData()
      }
    },
    resetFields() {
      this.form = {
        groupname: '',
        selectedCourse: null
      }
    },
    getSelectableCourses: async function () {
      this.selectableCourses = this.defaultSelectableCourses
      const result = await CourseAPI.getCourses();
      if (result.status === 200) {
        for (let i = 0; i < result.body.length; i++) {
          this.selectableCourses.push({value: result.body[i].courseID, text: result.body[i].courseName})
        }
      }
    },
    updateFilter(){
      this.tableFilter = this.form.selectedCourse
    }
  },

}
</script>

<style scoped>

</style>