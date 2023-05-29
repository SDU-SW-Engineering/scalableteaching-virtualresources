<template>
  <b-container fluid>
    <!-- User Interface controls -->
    <b-row>
      <b-col lg="6" class="my-1">
        <b-form-group
                label="Filter"
                label-for="filter-input"
                label-cols-sm="3"
                label-align-sm="right"
                label-size="sm"
                class="mb-0"
        >
          <b-input-group size="sm">
            <b-form-input
                    id="filter-input"
                    v-model="filter"
                    type="search"
                    placeholder="Type to Search"
            ></b-form-input>

            <b-input-group-append>
              <b-button :disabled="!filter" @click="filter = ''">Clear</b-button>
            </b-input-group-append>
          </b-input-group>
        </b-form-group>
      </b-col>

      <b-col lg="6" class="my-1">
        <b-form-group
                v-model="sortDirection"
                label="Filter On"
                description="Leave all unchecked to filter on all data"
                label-cols-sm="3"
                label-align-sm="right"
                label-size="sm"
                class="mb-0"
                v-slot="{ ariaDescribedby }"
        >
          <b-form-checkbox-group
                  v-model="filterOn"
                  :aria-describedby="ariaDescribedby"
                  class="mt-1"
          >
            <b-form-checkbox value="machine_name">Machine Name</b-form-checkbox>
            <b-form-checkbox value="class_name">Class Name</b-form-checkbox>
            <b-form-checkbox value="isActive">State</b-form-checkbox>
          </b-form-checkbox-group>
        </b-form-group>
      </b-col>
    </b-row>
    <b-row>
      <b-col sm="5" md="6" class="my-1">
        <b-form-group
                label="Number of machines shown"
                label-for="per-page-select"
                label-cols-sm="6"
                label-cols-md="4"
                label-cols-lg="3"
                label-align-sm="right"
                label-size="sm"
                class="mb-0"
        >
          <b-form-select
                  id="per-page-select"
                  v-model="perPage"
                  :options="pageOptions"
                  size="sm"
          ></b-form-select>
        </b-form-group>
      </b-col>

      <b-col sm="5" md="6">
        <b-button
                :disabled="selected == null"
                title="Turn the selected machines on and off again"
                v-b-tooltip.hover
                class="mr-1 mb-1"
                variant="warning"
                @click="onRebootSelectedClicked"
        >Reboot Selected Machines
        </b-button>
        <b-button
                :disabled="selected == null"
                title="Deselect the selected machines."
                class="mr-1 mb-1"
                @click="onDeselectClicked"
                variant="outline-secondary"
        >Deselect Selected Machines
        </b-button>
        <b-button
                :disabled="selected == null"
                title="Schedule the selected machines for deletion"
                v-b-tooltip.hover
                class="mr-1 mb-1"
                variant="danger"
                @click="onDeleteSelectedClicked"
        >Delete Selected Machines
        </b-button>
        <b-button
                :disabled="selected == null"
                title="Immediately delete and recreate the selected machines."
                v-b-tooltip.hover
                class="mr-1 mb-1"
                variant="danger"
                @click="onResetSelectedClicked"
        >Reset Selected Machines
        </b-button>
      </b-col>
    </b-row>
    <b-row>
      <b-col>
        <b-button variant="outline-secondary" @click="loadTableData" class="mb-1">Reload table</b-button>
      </b-col>
      <b-col>
        <label v-if="selected === null">Machines Selectes: 0</label>
        <label v-if="selected !== null">Machines Selectes: {{ selected.length }}</label>
      </b-col>

    </b-row>

    <!-- Main table element -->
    <b-table
            ref="ManagementMachinesTable"
            :items="items"
            :fields="fields"
            :current-page="currentPage"
            :per-page="perPage"
            :filter="filter"
            :filter-included-fields="filterOn"
            :sort-by.sync="sortBy"
            :sort-desc.sync="sortDesc"
            :sort-direction="sortDirection"
            :busy="tableIsBusy"
            stacked="md"
            show-empty
            small
            selectable
            select-mode="range"
            @filtered="onFiltered"
            @row-selected="onRowSelected"
    >
      <template #table-busy>
        <div class="text-center text-danger my-2">
          <b-spinner class="align-middle"></b-spinner>
          <strong> Loading...</strong>
        </div>
      </template>

      <template #cell(actions)="row">
        <!--<b-button size="sm" @click="info(row.item, row.index, $event.target)" class="mr-1">-->
        <!--  Reboot-->
        <!--</b-button>-->
        <b-button variant="warning" size="sm" @click="reboot(row.item.id)" class="mr-1">
          Reboot
        </b-button>
        <b-button variant="outline-info" size="sm" @click="promptResize(row.item)" class="mr-1">
          Resize
        </b-button>
        <b-button variant="outline-secondary" size="sm" @click="row.toggleDetails">
          {{ row.detailsShowing ? 'Hide' : 'Show' }} Details
        </b-button>
      </template>

      <template #row-details="row">
        <b-card>
          <ul>
            <li v-for="(value, key) in row.item" :key="key">{{ key }}: {{ value }}</li>
          </ul>
          <b-button v-if="isScheduledForDeletion(row.item)" variant="warning" size="sm"
                    @click="cancelDeletion(row.item.id)">
            Cancel Deletion
          </b-button>
        </b-card>
      </template>
    </b-table>
    <b-modal
            id="resize_modal_id"
            @ok="resize"
            @cancel="cancelResize"
    >
      <template #modal-title>
        Resize Machine Storage
      </template>
      <div>
        <b-form-input
                type="range"
                v-model="resizeModal.machine_new_size"
                :min="resizeModal.machine.size"
                :max="resizeModal.machine_max_size"
                :step="resizeModal.machine_step_size"></b-form-input>
      </div>
      Current Size: [{{resizeModal.machine.size}}] Selected Size: [{{resizeModal.machine_new_size}}]
      <template #modal-footer="{ ok, cancel }">
        <b-button size="sm" variant="warning" @click="ok()">Resize</b-button>
        <b-button size="sm" variant="secondary" @click="cancel()">Cancel</b-button>
      </template>
    </b-modal>


    <!--&lt;!&ndash; Reboot verification &ndash;&gt;-->
    <!--<b-modal :id="infoModal.id" :title="infoModal.title" ok-only @hide="resetInfoModal">-->
    <!--  <pre>{{ infoModal.content }}</pre>-->
    <!--  <p>Can i have the cake</p>-->
    <!--</b-modal>-->
  </b-container>
</template>
<script>
import MachinesAPI from "@/api/MachinesAPI";

export default {
  name: "MachineManagement",
  mounted() {
    this.loadTableData();
    // Set the initial number of items
    this.totalRows = this.items.length;
  },
  data() {
    return {
      items: [],
      fields: [
        {key: 'machine_name', label: 'Machine Name', sortable: true, sortDirection: 'desc'},
        {key: 'class_name', label: 'Class Name', sortable: true, class: 'text-center'},
        {
          key: 'isActive',
          label: 'Running',
          formatter: (value) => {
            return value ? 'Yes' : 'No';
          },
          sortable: true,
          sortByFormatted: true,
          filterByFormatted: true
        },
        {key: 'actions', label: 'Actions'}
      ],
      totalRows: 1,
      currentPage: 1,
      perPage: 50,
      pageOptions: [10, 25, 50, 100],
      sortBy: '',
      sortDesc: false,
      sortDirection: 'asc',
      selected: null,
      filter: null,
      filterOn: [],
      infoModal: {
        id: 'info-modal',
        title: '',
        content: ''
      },
      resizeModal: {
        machine: "",
        machine_original_size: 16384,
        machine_new_size: 16384,
        machine_max_size: 51200,
        machine_step_size: 1024,
        selected_value: 16384
      },
      tableIsBusy: false,
      isDevelopment: process.env.NODE_ENV === 'development',
    };
  },
  computed: {
    sortOptions() {
      // Create an options list from our fields
      return this.fields
        .filter(f => f.sortable)
        .map(f => {
          return {text: f.label, value: f.key};
        });
    }
  },
  methods: {
    isScheduledForDeletion(machine) {
      return machine.status.includes("deletion");
    },
    async reboot(machineId) {
      let response = await MachinesAPI.postRebootMachine(machineId);
      if (this.isDevelopment) window.console.log("Rebooted machine - Response: ", response);
    },
    async cancelDeletion(machineId) {
      let response = await MachinesAPI.undo_delete(machineId);
      if (response.status >= 200 && response.status < 300) {
        await this.loadTableData();
      }
    },
    async loadTableData() {
      this.tableIsBusy = true;
      let response = await MachinesAPI.getUsersMachines();
      if (this.isDevelopment) window.console.log("Loaded Table Data - Response: ", response);
      if (response.status === 200) {
        let machines = response.body;
        this.items = machines.map(m => {
          return {
            isActive: m.status === "ACTIVE",
            status: m.status,
            machine_name: m.hostname,
            ip: m.ipAddress,
            class_name: m.course.courseName,
            ports: m.ports,
            id: m.machineID,
            users: m.users,
            size: m.size
            //TODO: Fix backend return users
            //TODO: Fix backend return mac
          };
        });
      }
      this.tableIsBusy = false;
    },
    promptResize(machineItem) {
      console.log("promptResize");
      this.resizeModal.machine = machineItem;
      this.resizeModal.machine_new_size = machineItem.size
      this.$bvModal.show("resize_modal_id");

    },
    cancelResize() {
      console.log("promptResize");
      this.$bvModal.hide("resize_modal_id");
      this.resizeModal.machine = "";
    },
    resize() {
      console.log("promptResize");
      this.$bvModal.hide("resize_modal_id");
      if(this.resizeModal.machine_new_size === this.resizeModal.machine.size){
        this.resizeModal.machine = "";
        return;
      }
      if(this.resizeModal.machine_new_size > this.resizeModal.machine_max_size){
        this.resizeModal.machine = "";
        return;
      }
      MachinesAPI.resizeMachine(this.resizeModal.machine.id, this.resizeModal.machine_new_size)
      this.resizeModal.machine = "";
    },
    info(item, index, button) {
      this.infoModal.title = `Row index: ${index}`;
      this.infoModal.content = JSON.stringify(item, null, 2);
      this.$root.$emit('bv::show::modal', this.infoModal.id, button);
    },
    resetInfoModal() {
      this.infoModal.title = '';
      this.infoModal.content = '';
    },
    onFiltered(filteredItems) {
      // Trigger pagination to update the number of buttons/pages due to filtering
      this.totalRows = filteredItems.length;
      this.currentPage = 1;
    },
    onRowSelected(items) {
      this.selected = items;
      if (this.selected.length < 1) this.selected = null;
      if (this.isDevelopment) window.console.log("Selected Items: ", this.selected);
    },
    onRebootSelectedClicked() {
      if (this.selected.length < 1) return;
      for (let i = 0; i < this.selected.length; i++) {
        this.reboot(this.selected[i].id);
      }
      this.loadTableData();
    },
    onResetSelectedClicked() {
      if (this.selected.length < 1) return;
      for (let i = 0; i < this.selected.length; i++) {
        MachinesAPI.postResetMachine(this.selected[i].id);
      }
      this.loadTableData();
    },
    onDeleteSelectedClicked() {
      if (this.selected.length < 1) return;
      for (let i = 0; i < this.selected.length; i++) {
        MachinesAPI.deleteMachine(this.selected[i].id);
      }
      this.loadTableData();
    },
    onDeselectClicked() {
      this.selected = null;
      this.$refs.ManagementMachinesTable.clearSelected();
    },
  }
};
</script>

<style scoped>

</style>