<script lang="ts" setup>
import { onMounted, ref } from 'vue'
import { toast } from 'vue-sonner'
import { Button } from '@/components/ui/button'
import { getJiraUser } from '@/lib/jira'

const isLoggedIn = ref(false)

onMounted(async () => {
  if (await getJiraUser()) {
    isLoggedIn.value = true
    toast('Logged in with Jira')
  } else {
    isLoggedIn.value = false
  }
})
</script>

<template>
  <a v-if="!isLoggedIn" href="/api/jira/login">
    <Button class="w-full h-full bg-blue-800 cursor-pointer"> Log into Jira </Button>
  </a>
  <Button v-else disabled class="w-full h-full bg-blue-800"> You are logged in to Jira </Button>
</template>

<style scoped></style>
