<template>
  <b-tabs>
    <b-tab id="user" title="Manage User"><UserAdministration/></b-tab>
    <b-tab id="course" active title="Manage Courses"><CourseAdministration/></b-tab>
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