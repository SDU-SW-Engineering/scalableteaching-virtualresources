<template>
  <div>
    <b-container fluid="sm">

      <b-form @submit="onSubmit" @reset="resetFields">

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
          <b-button pill type="Create" variant="success" v-on:click="onSubmit">Submit</b-button>
          <b-button pill type="Reset Form" variant="secondary" >Reset</b-button>
        </b-button-group>
        <p style="color:#ff0000" v-if="formInvalidResponse">Insert text</p>
      </b-form>

      <hr>
      <b-row>
        <b-col><b-button pill v-on:click="loadTableData">Refresh Table</b-button></b-col>
        <b-col><b-button pill variant="danger" v-b-modal.remove-course-modal v-if="selectedRow.length >= 1">Remove Selected Course</b-button></b-col>
      </b-row>
      <b-modal id="remove-course-modal" title="Confirm Removal" hide-footer>
        <p>IF YOU REMOVE A COURSE <span style="color:red; font-weight: bold;">ALL MACHINES</span> ASSOSIATED WITH THE COURSE WILL <span style="color:red; font-weight: bold;">IMMEDIATELY BE DELETED </span></p>
        <b-button style="margin: 0 5px 0 0" variant="secondary" @click="$bvModal.hide('remove-course-modal')">Cancel</b-button>
        <b-button variant="outline-danger" v-on:click="removeSelectedCourse">Yes Remove Course</b-button>
      </b-modal>

      <hr>
      <!--TODO: Implement busy state https://bootstrap-vue.org/docs/components/table#table-busy-state-->
      <b-table
          sticky-header
          striped
          selectable
          select-mode="single"
          :items="courses"
          :fields="fields"
          @row-selected="onRowSelected"
          v-on:load="loadTableData"
      ></b-table>
    </b-container>
  </div>
</template>

<script>
import CourseAPI from "@/api/CourseAPI";
export default {
  name: "CourseAdministration",
  data() {
    return {
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
      courses: [
        // {
        //   gn: "Test Man",
        //   sn: "User",
        //   cn: "Test Man User",
        //   uname: "temus20",
        //   account_type: "Administrator",
        //   email: "temus20@student.sdu.dk"
        // },
        // {
        //   gn: "Test Whoman",
        //   sn: "User",
        //   cn: "Test Man User",
        //   uname: "tewus20",s
        //   account_type: "Administrator",
        //   email: "tewus20@student.sdu.dk"
//        }
      ]
    }
  },
  methods: {
    async loadTableData(){
      this.courses = []
      const result = await CourseAPI.getCourses();
      if(result.status === 200){
        for(let i = 0; i < result.body.length; i++){
          this.courses.push(result.body[i])
        }
      }
    },
    onRowSelected(items){
      this.selectedRow = items
    },
    removeSelectedCourse(){
      console.log(this.selectedRow[0])
      alert(`Selected Index ${this.selectedRow[0].courseName}`) //TODO: Actually delete item
      // if(selected)
      // selected[0]
    },
    validateFormItem(item){
      switch (item){
        case 'shortCourseName':
          return this.form.shortCourseName.length >= 3 && this.form.shortCourseName.length <= 6
        case 'ownerUsername':
          return this.form.ownerUsername.length >= 3
        case 'courseName':
          return this.form.courseName.length >= 1
        case 'courseID':
          return this.form.SDUCourseID.length >= 1
        default:
          return false;
      }

    },
    async onSubmit(event) {
      event.preventDefault()
      let resp = await CourseAPI.postCourse(this.form.ownerUsername, this.form.courseName, this.form.shortCourseName, this.form.SDUCourseID)
      let respoOk = resp === 200
      this.formInvalidResponse = respoOk
      if(respoOk){
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
    }
  }
}
</script>

<style scoped>

</style>