<script lang="ts" setup>
import Card from '@/components/Card.vue'
import { getFibbonacciNumber } from '@/lib/utils.ts'
import { currentPlayer, vote as currentVote } from '@/lib/room.ts'

const props = defineProps<{
  roomId: string
}>()

const cardValues = [0, 1, 3, 4, 5, 6, 7, 8]

const vote = async (number: number) => {
  await fetch(`/api/rooms/${props.roomId}/players/${currentPlayer.id!}/vote`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      value: number,
    }),
  })

  currentVote.vote = number
}
</script>

<template>
  <ul class="flex flex-row flex-wrap gap-2 items-center justify-center">
    <li v-for="index in cardValues">
      <Card :onVote="vote" :value="getFibbonacciNumber(index)" />
    </li>
  </ul>
</template>
