<template>
  <b-container id="EndOfCreationTable">
    <b-table
        :items="settings.machinesToBeCreated.items"
        :fields="settings.machinesToBeCreated.fields"
    >
      <template #cell(details)="row">
        <b-button size="sm" @click="row.toggleDetails">
          {{ row.detailsShowing ? 'Hide' : 'Show' }} Details
        </b-button>
      </template>
      <template #row-details="row">
        <b-card>
          <b-row no-gutters align-h="start" align-content="start">
            <b-col>
              <b-row align-h="start" no-gutters>
                <b-col md="4" align-self="start"><b>Machine Name:</b></b-col>
                <b-col md="4">{{ row.item.machineName }}</b-col>
              </b-row>
              <b-row align-h="start" no-gutters>
                <b-col md="4" align-self="start"><b>Course Name:</b></b-col>
                <b-col md="4">{{ row.item.courseName }}</b-col>
              </b-row>
            </b-col>
          </b-row>
          <b-row no-gutters align-h="start">
            <b-col md="4" align-self="start">
              <b>Machine Users:</b>
              <p>{{ row.item.users }}</p>
            </b-col>
            <b-col md="4">
              <b>User Groups:</b>
              <p>{{ row.item.groups }}</p>
            </b-col>
            <b-col md="4">
              <b>Ports:</b>
              <p>{{ row.item.ports }}</p>
            </b-col>
          </b-row>
          <b-row no-gutters align-h="start">
            <b-col md="8">
              <b>PPAs:</b>
              <p>{{ row.item.PPAs }}</p>
            </b-col>
            <b-col md="4">
              <b>Apt Packages:</b>
              <p>{{ row.item.APTs }}</p>
            </b-col>
          </b-row>
        </b-card>
      </template>
    </b-table>
  </b-container>
</template>

<script>
export default {
name: "EndOfCreationTable"
}
</script>

<style scoped>

</style>