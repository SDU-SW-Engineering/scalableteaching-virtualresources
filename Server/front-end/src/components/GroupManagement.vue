<template>
  <div>
    <b-container fluid="sm">

      <b-form @submit="submit" @reset="resetFields" autocomplete="false">

        <!--Associated Course-->
        <b-form-group
            id="input-group-1"
            label="Associated Course"
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
              :state="form.selectedCourse !== null && form.selectedCourse !== undefined"
          ></b-form-select>

          <b-form-invalid-feedback id="coursename-live-feedback">
            You must select a course
          </b-form-invalid-feedback>
        </b-form-group>

        <!--Group Name-->
        <b-form-group
            id="input-group-2"
            label="Group Name"
        >
          <b-form-input
              autocomplete="false"
              id="input-groupname"
              v-model="form.groupname"
              type="text"
              placeholder="Name of the group"
              v-b-tooltip.hover title="Name of the group"
              :state="form.groupname.length > 0"
              aria-describedby="groupname-live-feedback"
              required
          ></b-form-input>


          <b-form-invalid-feedback id="groupname-live-feedback">
            You must enter a group name
          </b-form-invalid-feedback>
        </b-form-group>

        <!--Group Users-->
        <b-form-group
            id="input-group-3"
            label="Group Users"
        >
          <b-form-textarea
              v-model="form.groupUsers"
              :state="validateUsersInput()"
          ></b-form-textarea>
        </b-form-group>

        <!--Submission buttons-->
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
        <b-button variant="outline-danger" v-on:click="removeSelectedGroup">Yes Remove Group</b-button>
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
      ></b-table>
    </b-container>
  </div>
</template>

<script>
//TODO: Implement file based multi upload of groups and users
import CourseAPI from "@/api/CourseAPI";
import GroupAPI from "@/api/GroupAPI";
import StringHelper from "@/helpers/StringHelper";

export default { //TODO: Getting errors when using page
  name: "GroupManagement",
  mounted() {
    this.getSelectableCourses();
    this.loadTableData();
  },
  data() {
    return {
      tableFilter: null,
      tableIsLoading: true,
      form: {
        groupname: '',
        selectedCourse: null,
        groupUsers: '',
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
        },
        {
          key: 'groupIndex',
          label: 'Group Number',
          sortable: true
        }
      ],
      groups: [],
      selectedRow: [],
    };
  },
  methods: {
    tableLoading(state) {
      if (state === true) {
        this.tableIsLoading = true;
      } else if (state === false) {
        this.tableIsLoading = false;
      } else {
        this.tableIsLoading = !this.tableIsLoading;
      }
    },
    async loadTableData() {
      this.resetFields();
      this.tableLoading(true);
      this.groups = [];
      //Get all groups
      const result = await GroupAPI.getGroups();
      //If successfull
      if (result.status === 200) {
        for (let i = 0; i < result.body.length; i++) {
          let singleResult = {
            groupName: result.body[i].groupName,
            courseID: result.body[i].courseID,
            groupID: result.body[i].groupID,
            groupIndex: result.body[i].groupIndex,
            courseName: '',
            members: []
          };
          //Add coursename to each group
          const resultCourse = await CourseAPI.getCourse(singleResult.courseID);
          singleResult.courseName = resultCourse.body.courseName;
          //Add group members to each group
          const resultMembers = await GroupAPI.getGroupMembers(singleResult.groupID);
          if (result.status === 200) {
            singleResult.members = resultMembers.body.members;
          }
          this.groups.push(singleResult);
        }
      }
      this.tableLoading(false);
    },
    onRowSelected(items) {
      this.selectedRow = items;
      if (this.selectedRow.length > 0) {
        this.form.selectedCourse = this.selectedRow[0].courseID;
        this.form.groupname = this.selectedRow[0].groupName;
        this.form.groupUsers = this.selectedRow[0].members.join();
      } else {
        this.resetFields();
      }
    },
    async removeSelectedGroup() {
      await GroupAPI.deleteGroup(this.selectedRow[0].groupID);
      this.$bvModal.hide('remove-group-modal')
      await this.loadTableData();
    },
    validateUsersInput() {
      let usersField = this.form.groupUsers;
      if (usersField === null || usersField === undefined || usersField.length === 0) return null;

      let cleanedUserTokens = StringHelper.breakStringIntoTokenList(this.form.groupUsers);
      for (let i = 0; i < cleanedUserTokens.length; i++) {
        let token = cleanedUserTokens[i];
        if (token.length < 3 || token.length > 7) return false;
      }

      return true;
    },
    async submit(event) {
      let respOk = false;
      event.preventDefault();
      let newUserList = StringHelper.breakStringIntoTokenList(this.form.groupUsers).sort();
      if (this.selectedRow.length > 0) {
        //Update member list
        if (newUserList.join() !== this.selectedRow[0].members.sort().join) {
          await GroupAPI.putMembersInGroup(newUserList, this.selectedRow[0].groupID);
        }

        //Update Group Name
        let resp = await GroupAPI.putGroup(this.form.groupname, this.form.selectedCourse, this.selectedRow[0].groupID);
        respOk = resp === 204;
      } else {
        //Create Group
        let resp = await GroupAPI.postEntireGroup(this.form.groupname, this.form.selectedCourse, newUserList);
        respOk = resp === 201;
      }
      this.formInvalidResponse = !respOk;
      if (respOk) {
        this.resetFields();
        await this.loadTableData();
      }
    },
    resetFields() {
      this.form = {
        groupname: '',
        selectedCourse: null
      };
    },
    getSelectableCourses: async function () {
      this.selectableCourses = this.defaultSelectableCourses;
      const result = await CourseAPI.getCourses();
      if (result.status === 200) {
        for (let i = 0; i < result.body.length; i++) {
          this.selectableCourses.push({value: result.body[i].courseID, text: result.body[i].courseName});
        }
      }
    },
    updateFilter() {
      this.tableFilter = this.form.selectedCourse;
    }
  }
};
</script>

<style scoped>

</style>