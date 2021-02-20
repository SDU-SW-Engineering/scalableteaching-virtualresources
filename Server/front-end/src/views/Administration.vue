<template>
  <b-tabs>
    <b-tab title="Manage User"><UserAdministration/></b-tab>
    <b-tab title="Manage Courses"><CourseAdministration/></b-tab>
  </b-tabs>
</template>


<script>
import CourseAdministration from "@/components/CourseAdministration";
import UserAdministration from "@/components/UserAdministration";


export default {
  name: "Administration",
  components: {CourseAdministration, UserAdministration},
}

</script>

<style scoped>

</style>