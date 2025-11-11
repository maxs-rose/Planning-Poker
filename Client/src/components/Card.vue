<script lang="ts" setup>
import { Card, CardContent } from '@/components/ui/card'
import { currentVote } from '@/lib/room.ts'

const props = defineProps<{
  value: number
  onVote: (value: number) => Promise<void>
}>()

const vote = async () => {
  await props.onVote(props.value)
}
</script>

<template>
  <Card
    :class="{ 'border-blue-900': currentVote.vote === props.value }"
    class="group w-48 h-72 select-none background hover:border-blue-900 border-8 transition"
    @click="vote"
  >
    <CardContent class="w-full h-full flex items-center justify-center text-6xl group-hover:scale-110 transition">
      {{ props.value === 0 ? '?' : props.value }}
    </CardContent>
  </Card>
</template>

<style scoped>
.background {
  background: radial-gradient(circle, rgba(63, 94, 251, 0.1) 0%, rgba(252, 70, 107, 0.1) 100%);
}
</style>
