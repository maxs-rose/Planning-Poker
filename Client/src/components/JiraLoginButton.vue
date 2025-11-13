<script lang="ts" setup>
import { onMounted, ref } from 'vue'
import { toast } from 'vue-sonner'
import { Button } from '@/components/ui/button'
import { getJiraUser } from '@/lib/jira'

const isJiraEnabled = ref(false)
const isLoggedIn = ref(false)

onMounted(async () => {
  const status = await getJiraUser()
  isLoggedIn.value = status < 400
  isJiraEnabled.value = isLoggedIn.value || status === 401

  if (isLoggedIn.value) toast('Logged in with Jira')
})
</script>

<template>
  <div v-if="isJiraEnabled" v-bind="$attrs">
    <a v-if="!isLoggedIn" href="/api/jira/login">
      <Button class="w-full h-full bg-blue-800 cursor-pointer"> Log into Jira </Button>
    </a>
    <Button v-else disabled class="w-full h-full bg-blue-800"> You are logged in to Jira </Button>
  </div>
</template>

<style scoped></style>
