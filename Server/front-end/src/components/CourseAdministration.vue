<template>
  <div>
    <b-container fluid="sm">

      <b-form @submit="onSubmit" @reset="resetFields" autocomplete="false">

        <b-form-group
            id="input-group-1"
            label="Course Parameters:"
            label-for="input-1"
        >
          <b-form-input
              id="input-coursename"
              v-model="form.courseName"
              type="text"
              placeholder="Name of the course"
              v-b-tooltip.hover title="Name of the course"
              :state="validateFormItem('courseName')"
              aria-describedby="coursename-live-feedback"
              required
          ></b-form-input>
          <b-form-invalid-feedback id="coursename-live-feedback">
            Must not be empty
          </b-form-invalid-feedback>

          <b-form-input
              id="input-short-coursename"
              v-model="form.shortCourseName"
              type="text"
              placeholder="Short name of the course"
              v-b-tooltip.hover title="Short name of the course (Between 3 and 6 character inclusive)"
              :state="validateFormItem('shortCourseName')"
              aria-describedby="shortCourseName-live-feedback"
              required
          ></b-form-input>
          <b-form-invalid-feedback id="shortCourseName-live-feedback">
            Must be no less than 3 and no more than 6 characters
          </b-form-invalid-feedback>

          <b-form-input
              autocomplete="false"
              id="input-courseid"
              v-model="form.SDUCourseID"
              type="text"
              placeholder="SDU Course id"
              v-b-tooltip.hover title="SDU Course ID (Can be found in odin)"
              :state="validateFormItem('courseID')"
              aria-describedby="courseID-live-feedback"
              required
          ></b-form-input>
          <b-form-invalid-feedback id="courseID-live-feedback">
            Must not be empty
          </b-form-invalid-feedback>

          <b-form-input
              autocomplete="false"
              id="input-owner-username"
              v-model="form.ownerUsername"
              type="text"
              placeholder="Username of the teacher for the course"
              v-b-tooltip.hover title="Username of the teacher for the course"
              :state="validateFormItem('ownerUsername')"
              aria-describedby="ownerUsername-live-feedback"
              required
          ></b-form-input>
          <b-form-invalid-feedback id="ownerUsername-live-feedback">
            Must not be empty
          </b-form-invalid-feedback>

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
          <b-button pill variant="success" v-on:click="onSubmit">Submit</b-button>
          <b-button pill variant="secondary">Reset</b-button>
        </b-button-group>
        <p style="color:#ff0000" v-if="formInvalidResponse">An error occurred</p>
      </b-form>

      <hr>
      <b-row>
        <b-col>
          <b-button pill v-on:click="loadTableData">Refresh Table</b-button>
        </b-col>
        <b-col>
          <b-button pill variant="danger" v-b-modal.remove-course-modal v-if="selectedRow.length >= 1">Remove Selected
            Course
          </b-button>
        </b-col>
      </b-row>
      <b-modal id="remove-course-modal" title="Confirm Removal" hide-footer>
        <p>IF YOU REMOVE A COURSE <span style="color:red; font-weight: bold;">ALL MACHINES AND GROUPS</span> ASSOSIATED WITH THE
          COURSE WILL <span style="color:red; font-weight: bold;">IMMEDIATELY BE DELETED </span></p>
        <b-button style="margin: 0 5px 0 0" variant="secondary" @click="$bvModal.hide('remove-course-modal')">Cancel
        </b-button>
        <b-button variant="outline-danger" v-on:click="removeSelectedCourse">Yes Remove Course</b-button>
      </b-modal>

      <hr>
      <b-table
          sticky-header="true"
          striped
          selectable
          select-mode="single"
          :items="courses"
          :fields="fields"
          @row-selected="onRowSelected"
          v-on:load="loadTableData"
          :busy="tableIsLoading"
      ></b-table>
    </b-container>
  </div>
</template>

<script>
import CourseAPI from "@/api/CourseAPI";

export default {
  name: "CourseAdministration",
  mounted() {
    this.loadTableData()
  },
  data() {
    return {
      InvalidResponseReason: 'Unknown error',
      tableIsLoading: true,
      form: {
        SDUCourseID: '',
        ownerUsername: '',
        courseName: '',
        shortCourseName: ''
      },
      formInvalidResponse: false,
      fields: [
        {
          key: 'shortCourseName',
          label: 'Short Course Name',
          sortable: true
        },
        {
          key: 'courseName',
          label: 'Course Name',
          sortable: true
        },
        {
          key: 'sduCourseID',
          label: 'SDU Course ID',
          sortable: true
        },
        {
          key: 'ownerUsername',
          label: 'Owner Username',
          sortable: true
        }
      ],
      selectedRow: [],
      courses: []
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
      this.courses = []
      const result = await CourseAPI.getCourses();
      if (result.status === 200) {
        for (let i = 0; i < result.body.length; i++) {
          this.courses.push(result.body[i])
        }
      }
      this.tableLoading(false)
    },
    onRowSelected(items) {
      this.selectedRow = items
      if(this.selectedRow.length !== 0){
        this.form.SDUCourseID = this.selectedRow[0].sduCourseID
        this.form.shortCourseName = this.selectedRow[0].shortCourseName
        this.form.courseName = this.selectedRow[0].courseName
        this.form.ownerUsername = this.selectedRow[0].ownerUsername
      }else{
        this.resetFields()
      }
    },
    async removeSelectedCourse() {
      await CourseAPI.deleteCourse(this.selectedRow[0].courseID)
      await this.loadTableData()
      this.$bvModal.hide('remove-course-modal')

    },
    validateFormItem(item) {
      switch (item) {
        case 'shortCourseName':
          return this.form.shortCourseName.length >= 3 && this.form.shortCourseName.length <= 6
        case 'ownerUsername':
          return this.form.ownerUsername.length >= 3
        case 'courseName':
          return this.form.courseName.length >= 3
        case 'courseID':
          return this.form.SDUCourseID.length >= 4
        default:
          return false;
      }

    },
    async onSubmit(event) {
      let respOk = 0
      event.preventDefault()

      if (this.selectedRow.length > 0) {
        let resp = await CourseAPI.putCourse(this.form.ownerUsername, this.form.courseName, this.form.shortCourseName, this.form.SDUCourseID, this.selectedRow[0].courseID)
        respOk = resp === 204
      } else {
        let resp = await CourseAPI.postCourse(this.form.ownerUsername, this.form.courseName, this.form.shortCourseName, this.form.SDUCourseID)
        respOk = resp === 201
      }

      this.formInvalidResponse = !respOk
      if (respOk) {
        this.resetFields()
        await this.loadTableData()
      }

    },
    resetFields() {
      this.form = {
        ownerUsername: '',
        courseName: '',
        shortCourseName: '',
        SDUCourseID: ''
      }
    }
  }
}
</script>

<style scoped>

</style>