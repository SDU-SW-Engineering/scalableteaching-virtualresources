<template>
  <b-tabs>
    <b-tab id="user" title="Manage Educators"><EducatorAdministration/></b-tab>
    <b-tab id="course" active title="Manage Courses"><CourseAdministration/></b-tab>
  </b-tabs>
</template>


<script>
import CourseAdministration from "@/components/CourseAdministration";
import EducatorAdministration from "@/components/EducatorAdministration";


export default {
  name: "Administration",
  components: {CourseAdministration, EducatorAdministration},
}

</script>

<style scoped>

</style>