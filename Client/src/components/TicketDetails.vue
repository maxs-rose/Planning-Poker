<script setup lang="ts">
import { room } from '@/lib/room.ts'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
</script>

<template>
  <Card v-if="room.ticket !== undefined && room.ticket.id.length > 0">
    <CardContent class="flex flex-col w-full gap-2">
      <div class="inline-flex w-full gap-2">
        <img :src="room.ticket.icon" :alt="room.ticket.typeName" width="20" height="20" />
        <h4 class="text-xl grow align-middle content-center">
          <span class="font-semibold">{{ room.ticket.key }}:</span> {{ room.ticket.title }}
        </h4>
        <Button variant="outline"> More </Button>
      </div>
      <div v-if="room.ticket.labels.length > 0" class="inline-flex w-full gap-2">
        <span v-for="label in room.ticket.labels" class="bg-blue-600 text-white rounded px-2">{{ label }}</span>
      </div>
      <div class="[&_p]:mb-1.5" v-html="room.ticket.description"></div>
    </CardContent>
  </Card>
</template>

<style scoped></style>
