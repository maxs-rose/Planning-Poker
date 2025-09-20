<script lang="ts" setup>
import ThemeSelector from '@/components/ThemeSelector.vue'
</script>

<template>
  <nav class="flex w-screen p-2 items-center justify-center">
    <div class="flex items-center justify-between w-full max-w-2/3">
      <span class="flex flex-row items-center justify-center gap-2">
        <img alt="Planning Poker Logo" class="h-10 mr-2" src="@/assets/logo.svg" />
        <h1 class="scroll-m-20 text-4xl tracking-tight lg:text-5xl">Planning Poker</h1>
      </span>

      <ThemeSelector />
    </div>
  </nav>
</template>

<style scoped></style>
