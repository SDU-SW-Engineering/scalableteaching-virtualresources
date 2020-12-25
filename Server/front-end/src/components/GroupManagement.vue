<template>
  <b-container fluid>

  </b-container>
</template>

<script>
import store from "@/store/index.js"
export default {
  name: "GroupManagement",
  data: function(){
    return {

      items: [
        /*        { isActive: true, age: 40, name: { first: 'Dickerson', last: 'Macdonald' } },
                { isActive: false, age: 21, name: { first: 'Larsen', last: 'Shaw' } },
                {
                  isActive: false,
                  age: 9,
                  name: { first: 'Mini', last: 'Navarro' },
                  _rowVariant: 'success'
                },
                { isActive: false, age: 89, name: { first: 'Geneva', last: 'Wilson' } },
                { isActive: true, age: 38, name: { first: 'Jami', last: 'Carney' } },
                { isActive: false, age: 27, name: { first: 'Essie', last: 'Dunlap' } },
                { isActive: true, age: 40, name: { first: 'Thor', last: 'Macdonald' } },
                {
                  isActive: true,
                  age: 87,
                  name: { first: 'Larsen', last: 'Shaw' },
                  _cellVariants: { age: 'danger', isActive: 'warning' }
                },
                { isActive: false, age: 26, name: { first: 'Mitzi', last: 'Navarro' } },
                { isActive: false, age: 22, name: { first: 'Genevieve', last: 'Wilson' } },
                { isActive: true, age: 38, name: { first: 'John', last: 'Carney' } },
                { isActive: false, age: 29, name: { first: 'Dick', last: 'Dunlap' } }*/
        {}
      ],
      fields: [
        { key: 'machine_name', label: 'Machine Name', sortable: true, sortDirection: 'desc' },
        { key: 'class_name', label: 'Class Name', sortable: true, class: 'text-center' },
        {
          key: 'isActive',
          label: 'Is Active',
          formatter: (value) => {
            return value ? 'Yes' : 'No'
          },
          sortable: true,
          sortByFormatted: true,
          filterByFormatted: true
        },
        { key: 'actions', label: 'Actions' }
      ],
      totalRows: 1,
      currentPage: 1,
      perPage: 5,
      pageOptions: [5, 10, 25, 50, 100],
      sortBy: '',
      sortDesc: false,
      sortDirection: 'asc',
      filter: null,
      filterOn: [],
      infoModal: {
        id: 'info-modal',
        title: '',
        content: ''
      }
    }
  },
  methods: {
    getGroups: function(){
      //TODO: Implement returning groups
    },
    getUser: function(){
      return store.state.user
    }
  }
}
</script>

<style scoped>

</style>