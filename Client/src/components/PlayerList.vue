<script lang="ts" setup>
import type { Player } from '@/lib/model/player.interface.ts'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Icon } from '@iconify/vue'
import { Button } from '@/components/ui/button'
import { currentPlayer, room } from '@/lib/room.ts'
import { computed } from 'vue'

const props = defineProps<{
  players: Player[]
  votes: Record<string, number>
  reveal: boolean
  owner: boolean
  roomId: string
}>()

const sortedPlayers = computed(() => {
  return props.players
    .filter((p) => !p.isSpectator)
    .sort((a, b) => {
      if (a.isOwner && !b.isOwner) return -1
      if (!a.isOwner && b.isOwner) return 1
      return 0
    })
})

const setHost = async (playerId: string) => {
  room.owner = false
  return fetch(`/api/rooms/${props.roomId}/set-host`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      user: playerId,
    }),
  })
}
</script>

<template>
  <Card class="py-6">
    <CardHeader>
      <CardTitle class="font-normal"><slot /></CardTitle>
    </CardHeader>
    <CardContent>
      <div class="grid grid-cols-[1fr_auto_auto] gap-2 items-center">
        <div class="font-bold">Players</div>

        <template v-for="player in sortedPlayers">
          <div
            :class="{
              'font-bold': currentPlayer.id === player.id,
              'text-gray-400': !player.isConnected,
              'ps-6': player.isOwner,
            }"
            class="col-start-1 relative wrap-anywhere"
            data-testid="PlayerNames"
          >
            <Icon v-if="player.isOwner" class="absolute left-0 top-1.25" icon="radix-icons:sketch-logo" />
            {{ player.name }}
            <span v-if="!player.isConnected" class="text-xs ml-1">(disconnected)</span>
          </div>

          <div v-if="props.reveal" class="text-end" :class="{ 'text-gray-400': !player.isConnected }">
            {{ votes[player.id!] || 'Abstained' }}
          </div>
          <div v-else :class="{ 'text-gray-400': !player.isConnected }">
            <div v-if="votes[player.id!] !== undefined">
              <Icon icon="radix-icons:check" />
            </div>
            <div v-else>
              <Icon icon="radix-icons:dots-horizontal" />
            </div>
          </div>

          <div v-if="props.owner && player.id !== currentPlayer.id">
            <Button @click="setHost(player.id)" :disabled="!player.isConnected">Set Host</Button>
          </div>
        </template>
      </div>
    </CardContent>
  </Card>
</template>
