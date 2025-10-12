<script lang="ts" setup>
import Card from '@/components/Card.vue'
import { getFibbonacciNumber } from '@/lib/utils.ts'
import { vote } from '@/lib/room.ts'

const cardValues = [0, 1, 3, 4, 5, 6, 7, 8]
</script>

<template>
  <ul class="flex flex-row flex-wrap gap-2 items-center justify-center">
    <li v-for="index in cardValues">
      <Card :onVote="vote" :value="getFibbonacciNumber(index)" />
    </li>
  </ul>
</template>
