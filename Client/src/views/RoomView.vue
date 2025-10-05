<script lang="ts" setup>
import { currentPlayer, joinRoom, vote } from '@/lib/room.ts'
import { useRouter } from 'vue-router'
import type { Room } from '@/lib/model/room.interface.ts'
import { onUnmounted, reactive, ref } from 'vue'
import VotingTable from '@/components/VotingTable.vue'
import PlayerList from '@/components/PlayerList.vue'
import type { Vote } from '@/lib/model/vote.interface.ts'
import type { Player } from '@/lib/model/player.interface.ts'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import VotingResult from '@/components/VotingResult.vue'
import { Icon } from '@iconify/vue'
import { toast } from 'vue-sonner'

const props = defineProps<{
  roomId: string
}>()

const router = useRouter()

if (props.roomId === '' || !currentPlayer.name) router.replace(`/?joinCode=${props.roomId}`)

const roomConnection = joinRoom(props.roomId, currentPlayer.name!)

roomConnection.onerror = (e) => {
  toast.error('Failed to connect to room')
  console.error(e)
  router.replace('/')
}

onUnmounted(() => {
  roomConnection.close()
})

const room = reactive<Room>({
  friendlyName: '',
  owner: false,
  players: [],
  votes: {},
})

const reveal = reactive({ state: false })
const currentState = ref('Voting')

roomConnection.addEventListener('Init', (event) => {
  console.log('init event', event)
  const init: Room & { playerId: string; votes: Vote[] } = JSON.parse(event.data)
  room.friendlyName = init.friendlyName
  room.owner = init.owner
  room.players = init.players
  room.votes = Object.fromEntries(init.votes.map((vote) => [vote.voter, vote.value]))
  currentPlayer.id = init.playerId
  currentPlayer.isSpectator = false
})

roomConnection.addEventListener('Vote', (event) => {
  console.log('vote event', event)
  const vote: Vote = JSON.parse(event.data)

  room.votes[vote.voter] = vote.value
})

roomConnection.addEventListener('OwnerChange', (event) => {
  console.log('owner change', event)
  const player: Player = JSON.parse(event.data)

  room.owner = player.id === currentPlayer.id
})

roomConnection.addEventListener('Join', (event) => {
  console.log('owner change', event)
  const player: Player = JSON.parse(event.data)

  room.players.push(player)
})

roomConnection.addEventListener('Leave', (event) => {
  console.log('owner change', event)
  const player: Player = JSON.parse(event.data)

  room.players = room.players.filter((p) => p.id !== player.id)
})

roomConnection.addEventListener('Reveal', (event) => {
  console.log('reveal', event)
  currentState.value = 'Revealing'
  reveal.state = true
  vote.vote = undefined
})

roomConnection.addEventListener('Reset', (event) => {
  console.log('reset', event)

  currentState.value = 'Voting'
  room.votes = {}
  reveal.state = false
})

roomConnection.addEventListener('PlayerUpdate', (event) => {
  console.log('player update', event)
  const player: Player = JSON.parse(event.data)

  room.players = [...room.players.filter((p) => p.id !== player.id), player]

  if (player.id === currentPlayer.id) {
    currentPlayer.isSpectator = player.isSpectator
  }
})

const updateRoom = async (action: 'reveal' | 'reset') => {
  await fetch(`/api/rooms/${props.roomId}/${action}`)
}

const copyToClipboard = (text: string) => {
  navigator.clipboard.writeText(text)
  toast('Copied to clipboard')
}

const setSpectator = async (isSpectator: boolean) => {
  return fetch(`/api/rooms/${props.roomId}/players/${currentPlayer.id}/${isSpectator ? 'spectate' : 'participate'}`)
}
</script>

<template>
  <div class="p-6 flex flex-col gap-2 items-start justify-center lg:flex-row">
    <div class="flex flex-col items-center justify-center gap-2 w-xl min-w-lg">
      <Card v-if="room.owner" class="w-full">
        <CardHeader>
          <CardTitle>Host Tools</CardTitle>
        </CardHeader>
        <CardContent>
          <Button v-if="!reveal.state" variant="outline" @click="updateRoom('reveal')">Reveal Scores</Button>
          <Button v-if="reveal.state" variant="outline" @click="updateRoom('reset')">Next Round</Button>
        </CardContent>
      </Card>

      <Card v-else class="w-full">
        <CardHeader>
          <CardTitle>Player Options</CardTitle>
        </CardHeader>
        <CardContent>
          <Button v-if="!currentPlayer.isSpectator" variant="outline" @click="setSpectator(true)">Spectate</Button>
          <Button v-if="currentPlayer.isSpectator" variant="outline" @click="setSpectator(false)">Participate</Button>
        </CardContent>
      </Card>

      <PlayerList
        :owner="room.owner"
        :players="room.players"
        :reveal="reveal.state"
        :roomId="props.roomId"
        :votes="room.votes"
        class="w-full"
      >
        <div class="flex flex-col gap-2">
          <p class="flex gap-3 text-nowrap">
            Join Code:
            <span class="flex gap-2 items-center cursor-pointer" data-testid="JoinCode" @click="copyToClipboard(props.roomId)">
              {{ props.roomId }}
              <Icon icon="radix-icons:copy" />
            </span>
          </p>

          <h3 class="font-bold">{{ currentState }}</h3>
        </div>
      </PlayerList>
    </div>

    <div class="w-full max-w-7xl">
      <VotingTable v-if="!reveal.state" :roomId="props.roomId" />
      <VotingResult v-if="reveal.state" :players="room.players" :votes="room.votes" />
    </div>
  </div>
</template>

<style scoped></style>
