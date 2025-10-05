<script lang="ts" setup>
import { Card, CardContent } from '@/components/ui/card'
import { vote as currentVote } from '@/lib/room.ts'

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
    class="group w-64 h-100 select-none background hover:border-blue-900 border-8"
    @click="vote"
  >
    <CardContent class="w-full h-full flex items-center justify-center text-6xl group-hover:scale-110">
      {{ props.value === 0 ? '?' : props.value }}
    </CardContent>
  </Card>
</template>

<style scoped>
.background {
  background: radial-gradient(circle, rgba(63, 94, 251, 0.1) 0%, rgba(252, 70, 107, 0.1) 100%);
}
</style>
