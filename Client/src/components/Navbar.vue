<script lang="ts" setup>
import ThemeSelector from '@/components/ThemeSelector.vue'
import { useRouter } from 'vue-router'

const router = useRouter()
</script>

<template>
  <nav class="flex w-full p-2 items-center justify-center">
    <div class="flex items-center justify-between gap-2 w-full px-2 sm:px-4 md:px-0 md:max-w-2/3">
      <span
        class="select-none cursor-pointer flex flex-row items-center justify-center gap-2"
        @click="router.push('/')"
      >
        <img alt="Planning Poker Logo" class="h-10 mr-2" src="@/assets/logo.svg" />
        <h1 class="scroll-m-20 text-4xl tracking-tight lg:text-5xl">Planning Poker</h1>
      </span>

      <ThemeSelector />
    </div>
  </nav>
</template>

<style scoped></style>
