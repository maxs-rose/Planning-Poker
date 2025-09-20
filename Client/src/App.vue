<script lang="ts" setup>
import { RouterView } from 'vue-router'
import Navbar from '@/components/Navbar.vue'
</script>

<template>
  <Navbar />

  <div class="overflow-x-hidden mx-auto">
    <RouterView />
  </div>
</template>
