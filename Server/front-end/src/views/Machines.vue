<template>
  <b-container fluid>
    <!-- User Interface controls -->
    <b-row>
      <b-col lg="6" class="my-1">
        <b-form-group
            label="Sort"
            label-for="sort-by-select"
            label-cols-sm="3"
            label-align-sm="right"
            label-size="sm"
            class="mb-0"
            v-slot="{ ariaDescribedby }"
        >
          <b-input-group size="sm">
            <b-form-select
                id="sort-by-select"
                v-model="sortBy"
                :options="sortOptions"
                :aria-describedby="ariaDescribedby"
                class="w-75"
            >
              <template #first>
                <option value="">-- none --</option>
              </template>
            </b-form-select>

            <b-form-select
                v-model="sortDesc"
                :disabled="!sortBy"
                :aria-describedby="ariaDescribedby"
                size="sm"
                class="w-25"
            >
              <option :value="false">Asc</option>
              <option :value="true">Desc</option>
            </b-form-select>
          </b-input-group>
        </b-form-group>
      </b-col>

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
    </b-row>
    <b-row>
      <b-col>
        <b-button variant="outline-secondary" @click="loadTableData" class="mb-1">Reload table</b-button>
      </b-col>
    </b-row>

    <!-- Main table element -->
    <b-table
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
        @filtered="onFiltered"
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
        <b-button size="sm" @click="reboot(row.item.id)" class="mr-1">
          Reboot
        </b-button>
        <b-button size="sm" @click="row.toggleDetails">
          {{ row.detailsShowing ? 'Hide' : 'Show' }} Details
        </b-button>
      </template>

      <template #row-details="row">
        <b-card>
          <ul>
            <li v-for="(value, key) in row.item" :key="key">{{ key }}: {{ value }}</li>
          </ul>
        </b-card>
      </template>
    </b-table>

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
  name: "Machines",
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
      perPage: 10,
      pageOptions: [10, 25, 50, 100],
      sortBy: '',
      sortDesc: false,
      sortDirection: 'asc',
      filter: null,
      filterOn: [],
      infoModal: {
        id: 'info-modal',
        title: '',
        content: ''
      },
      tableIsBusy: false,
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
    async reboot(machineId) {
      console.log(await MachinesAPI.postRebootMachine(machineId));
    },
    async loadTableData() {
      this.tableIsBusy= true;
      let response = await MachinesAPI.getUsersMachines();
      console.log(response);
      if (response.status === 200) {
        let machines = response.body;
        this.items = machines.map(m => {
          return {
            isActive: m.status === "ACTIVE",
            machine_name: m.hostname,
            ip: m.ipAddress,
            class_name: m.course.courseName,
            ports: m.ports,
            id: m.machineID,
            //TODO: Fix backend return users
            //TODO: Fix backend return mac
          };
        });
      }
      this.tableIsBusy= false;
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
    }
  }
};
</script>

<style scoped>

</style>