<template>
  <b-container id="EndOfCreationTable">
    <b-table
        responsive
        :items="machinesToBeCreated.items"
        :fields="machinesToBeCreated.fields"
        select-mode="range"
        @row-selected="rowSelected"
    >
      <template #cell(name)="data">{{ data.value }}</template>
      <template v-if="!machineSettings.isGroupBased" #cell(users)="data">{{ joinWithBreaks(data.value) }}</template>
      <template v-if="machineSettings.isGroupBased" #cell(group)="data">{{ data.value }}</template>
      <template #cell(linuxgroups)="data">{{ joinWithBreaks(data.value) }}</template>
      <template #cell(apt)="data">{{ joinWithBreaks(data.value) }}</template>
      <template #cell(ppa)="data">{{ joinWithBreaks(data.value) }}</template>
      <template #cell(courseid)="data">{{ data.value }}</template>
    </b-table>
  </b-container>
</template>

<script>
export default {
  //TODO: Implement individual machine editing.
  //TODO: Implement proper group and course names
  name: "EndOfCreationTable",
  props: ['machineSettings'],
  mounted() {
    console.log("Machines recieved by creation table", this.machineSettings.machinesToBeCreatedList)
    this.populateMachines(this.machineSettings.machinesToBeCreatedList, this.machineSettings.isGroupBased);
  },
  data() {
    return {
      selectedRows: [],
      isGroupBased: false,
      machinesToBeCreated: {
        items: [],
        fields: [],
      },
      groupFieldDefaults: [
        {key: 'hostname', label: 'Hostname', sortable: true},
        {key: 'group', label: 'Group', sortable: true},
        {key: 'apt', label: 'Apt Packages', sortable: false},
        {key: 'ppa', label: 'PPA`s', sortable: false},
        {key: 'ports', label: 'Ports', sortable: true},
        {key: 'linuxgroups', label: 'Linux Groups', sortable: true},
        {key: 'courseid', label: 'Course ID', sortable: false},
      ], //Default field types based on the machines being group based.
      userFieldDefaults: [
        {key: 'hostname', label: 'Hostname', sortable: true},
        {key: 'users', label: 'User', sortable: true},
        {key: 'apt', label: 'Apt Packages', sortable: false},
        {key: 'ppa', label: 'PPA`s', sortable: false},
        {key: 'ports', label: 'Ports', sortable: true},
        {key: 'linuxgroups', label: 'Linux Groups', sortable: true},
      ],

    };
  },
  methods: {
    rowSelected(items) {
      this.selectedRows = items;
    },
    populateMachines(machines, isGroupBased) {
      this.machinesToBeCreated.fields = isGroupBased ? this.groupFieldDefaults : this.userFieldDefaults;
      this.machinesToBeCreated.items = machines;
    },
    clearMachines() {
      this.machinesToBeCreated.items = [];
    },
    deselect() {
      //TODO: Implement machine deselction

    },
    getMachinesToBeCreated() {
      //TODO: Return machines that are not de-selected in the table
      return {machinesToBeCreated: this.machinesToBeCreated.items, isGroupBased: this.machineSettings.isGroupBased};
    },
    joinWithBreaks(listOfValues) {
      return listOfValues.join('\n');
    }
  }
};
</script>

<style scoped>

</style>