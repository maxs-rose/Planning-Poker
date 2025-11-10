<script lang="ts" setup>
import RoomSetup from '@/components/RoomSetup.vue'
import JiraLoginButton from '@/components/JiraLoginButton.vue'
</script>

<template>
  <main class="min-h-[50vh]">
    <RoomSetup />

    <div class="w-full flex mt-4 justify-center items-center">
      <JiraLoginButton class="w-[400px]" />
    </div>
  </main>
</template>
