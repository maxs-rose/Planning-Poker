<script lang="ts" setup>
import { RouterView } from 'vue-router'
import { Toaster } from '@/components/ui/sonner'
import Navbar from '@/components/Navbar.vue'
</script>

<template>
  <Toaster />
  <Navbar />

  <div class="overflow-x-hidden mx-auto">
    <RouterView />
  </div>
</template>
