<script lang="ts" setup>
import { RouterView } from 'vue-router'
import { Toaster } from '@/components/ui/sonner'
import Navbar from '@/components/Navbar.vue'

const version = import.meta.env.VITE_APP_VERSION ?? 'Development'
</script>

<template>
  <Toaster />
  <Navbar />

  <div class="overflow-x-hidden mx-auto">
    <RouterView />
  </div>

  <footer class="absolute bottom-0 max-h-20 w-full flex justify-start items-center border-t p-6 text-primary/50">
    Build {{ version }}
  </footer>
</template>
