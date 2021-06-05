<template>
  <div>
    <b-container fluid="sm">

      <b-form @submit="onSubmit" @reset="resetFields" autocomplete="off">

        <b-form-group
            id="input-group-1"
            label="Associated Course"
            label-for="input-1"
        >
          <b-form-select
              autocomplete="false"
              v-model="form.selectedCourse"
              :options="selectableCourses"
              onload="getSelectableCourses()"
              placeholder="Associated Course"
              v-b-tooltip.hover title="Associated Course"
              aria-describedby="coursename-live-feedback"
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
        </b-form-group>

        <b-form-invalid-feedback id="groupname-live-feedback">
          You must enter a group name
        </b-form-invalid-feedback>


        <b-button-group>
          <b-button pill type="Create" variant="success" v-on:click="onSubmit">Submit</b-button>
          <b-button pill type="Reset Form" variant="secondary">Reset</b-button>
        </b-button-group>
        <p style="color:#ff0000" v-if="formInvalidResponse">Insert text</p>
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
      <!--TODO: Implement busy state https://bootstrap-vue.org/docs/components/table#table-busy-state-->
      <b-table
          sticky-header
          striped
          selectable
          select-mode="single"
          :items="groups"
          :fields="fields"
          @row-selected="onRowSelected"
          v-on:load="loadTableData"
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
  },
  data() {
    return {
      form: {
        groupname: '',
        selectedCourse: null,
      },
      defaultSelectedCourse: null,
      defaultSelectableCourses: [{value: null, text: 'Select a course'}],
      selectableCourses: [],
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
    async loadTableData() {
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
      console.log("Groups: ", this.groups)
    },
    onRowSelected(items) {
      this.selectedRow = items
    },
    removeSelectedCourse() {
      console.log(this.selectedRow[0])
      alert(`Selected Index ${this.selectedRow[0].courseName}`) //TODO: Actually delete item
      // if(selected)
      // selected[0]
    },
    // eslint-disable-next-line
    validateFormItem(item) { //TODO: fix Form Validation
      // switch (item){
      //   case 'shortCourseName':
      //     return this.form.shortCourseName.length >= 3 && this.form.shortCourseName.length <= 6
      //   case 'ownerUsername':
      //     return this.form.ownerUsername.length >= 3
      //   default:
      //     return false;
      // }
      return false;
    },
    async onSubmit(event) { //TODO: Implement for groups not courses
      event.preventDefault()
      let resp = await CourseAPI.postCourse(this.form.ownerUsername, this.form.courseName, this.form.shortCourseName, this.form.SDUCourseID)
      let respoOk = resp === 200
      this.formInvalidResponse = respoOk
      if (respoOk) {
        this.resetFields()
      }

      //TODO:  Print error response and clear screen
    },
    resetFields() {
      this.form = {
        courseID: '',
        ownerUsername: '',
        courseName: '',
        shortCourseName: ''
      }
    },
    getSelectableCourses: async function () {
      this.selectableCourses = this.defaultSelectableCourses
      const result = await CourseAPI.getCourses();
      if (result.status === 200) {
        for (let i = 0; i < result.body.length; i++) {
          this.selectableCourses.push({value: result.body[i].couseID, text: result.body[i].courseName})
        }
      }
    }
  },

}
</script>

<style scoped>

</style>