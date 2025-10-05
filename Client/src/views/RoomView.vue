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

const props = defineProps<{
  roomId: string
}>()

const router = useRouter()

if (props.roomId === '' || !currentPlayer.name) router.replace('/')

const roomConnection = joinRoom(props.roomId, currentPlayer.name!)

if (!roomConnection) router.replace('/')

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

const updateRoom = async (action: 'reveal' | 'reset') => {
  await fetch(`/api/rooms/${props.roomId}/${action}`)
}
</script>

<template>
  <div class="p-6 flex flex-col gap-2 items-start">
    <div class="w-full flex items-center justify-center flex-row gap-2">
      <Card v-if="room.owner">
        <CardHeader>
          <CardTitle>Host Tools</CardTitle>
        </CardHeader>
        <CardContent>
          <Button v-if="!reveal.state" variant="outline" @click="updateRoom('reveal')">Reveal Scores</Button>
          <Button v-if="reveal.state" variant="outline" @click="updateRoom('reset')">Next Round</Button>
        </CardContent>
      </Card>

      <PlayerList
        :owner="room.owner"
        :players="room.players"
        :reveal="reveal.state"
        :roomId="props.roomId"
        :votes="room.votes"
        class="min-w-[450px]"
      >
        <div class="flex flex-col gap-2">
          <p class="text-nowrap">Join Code: {{ props.roomId }}</p>

          <h3 class="font-bold">{{ currentState }}</h3>
        </div>
      </PlayerList>
    </div>

    <VotingTable v-if="!reveal.state" :roomId="props.roomId" />
    <VotingResult v-if="reveal.state" :players="room.players" :votes="room.votes" />
  </div>
</template>

<style scoped></style>
