<script lang="ts" setup>
import type { Player } from '@/lib/model/player.interface.ts'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Icon } from '@iconify/vue'
import { Button } from '@/components/ui/button'
import { currentPlayer } from '@/lib/room.ts'

const props = defineProps<{
  players: Player[]
  votes: Record<string, number>
  reveal: boolean
  owner: boolean
  roomId: string
}>()

const setHost = async (playerId: string) => {
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
  <Card class="p-6">
    <CardHeader>
      <CardTitle class="font-normal"><slot /></CardTitle>
    </CardHeader>
    <CardContent>
      <div class="grid grid-cols-3 gap-2 items-center">
        <div class="font-bold">Players</div>

        <template v-for="player in props.players">
          <div class="col-start-1">{{ player.name }}</div>

          <div v-if="props.reveal">{{ votes[player.id!] || 'Abstained' }}</div>
          <div v-else>
            <div v-if="votes[player.id!] !== undefined">
              <Icon icon="radix-icons:check" />
            </div>
            <div v-else>
              <Icon icon="radix-icons:dots-horizontal" />
            </div>
          </div>

          <div v-if="props.owner && player.id !== currentPlayer.id">
            <Button @click="setHost(player.id)">Set Host</Button>
          </div>
        </template>
      </div>
    </CardContent>
  </Card>
</template>
