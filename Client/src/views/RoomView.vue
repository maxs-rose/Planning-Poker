<script lang="ts" setup>
import {
  currentPlayer,
  currentVote,
  joinRoom,
  processRoomState,
  room,
  roomConnect,
  roomExists,
  vote,
  clearPlayerData,
} from '@/lib/room.ts'
import { useRouter } from 'vue-router'
import { onMounted, onUnmounted, reactive, ref } from 'vue'
import VotingTable from '@/components/VotingTable.vue'
import PlayerList from '@/components/PlayerList.vue'
import type { Vote } from '@/lib/model/vote.interface.ts'
import type { Player } from '@/lib/model/player.interface.ts'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import VotingResult from '@/components/VotingResult.vue'
import { Icon } from '@iconify/vue'
import { toast } from 'vue-sonner'
import type { Init } from '@/lib/model/init.interface.ts'

const props = defineProps<{
  roomId: string
}>()

const router = useRouter()

if (!currentPlayer.name) {
  router.replace(`/?joinCode=${props.roomId}`)
}

const roomConnection = roomConnect(props.roomId)

onMounted(async () => {
  if (!(await roomExists(props.roomId))) {
    toast('Room does not exist')
    clearPlayerData()
    await router.replace('/')
  }
})

onUnmounted(() => {
  roomConnection.close()
})

const reveal = reactive({ state: false })
const currentState = ref('Voting')

roomConnection.addEventListener('Init', async (event) => {
  console.log('init event', event)
  const wasOwner = room.owner
  const wasConnected = !!currentPlayer.id

  const init: Init = JSON.parse(event.data)
  processRoomState(init)

  reveal.state = init.revealed
  currentState.value = init.revealed ? 'Revealing' : 'Voting'

  try {
    const joinResponse = await joinRoom(
      props.roomId,
      init.playerId,
      currentPlayer.name!,
      wasOwner || (wasConnected && room.players.length === 0),
    )
    processRoomState(joinResponse)

    reveal.state = joinResponse.revealed
    currentState.value = joinResponse.revealed ? 'Revealing' : 'Voting'

    if (
      wasConnected &&
      currentVote.vote !== undefined &&
      !joinResponse.revealed &&
      (!currentPlayer.id || room.votes[currentPlayer.id] === undefined)
    ) {
      await vote(currentVote.vote)
    }
  } catch (error) {
    console.error('Failed to join room:', error)
    toast.error('Failed to join room. Redirecting...')
    clearPlayerData()
    await router.replace('/')
  }
})

roomConnection.addEventListener('Heartbeat', async (event) => {
  console.log('heartbeat', event)

  const heartbeat: Init = JSON.parse(event.data)
  processRoomState(heartbeat)

  reveal.state = heartbeat.revealed
  currentState.value = heartbeat.revealed ? 'Revealing' : 'Voting'
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
  console.log('join', event)
  const player: Player = JSON.parse(event.data)

  const existingPlayerIndex = room.players.findIndex((p) => p.id === player.id)

  if (existingPlayerIndex !== -1) {
    room.players[existingPlayerIndex] = player
  } else {
    room.players.push(player)
  }
})

roomConnection.addEventListener('Leave', (event) => {
  console.log('leave', event)
  const player: Player = JSON.parse(event.data)

  room.players = room.players.filter((p) => p.id !== player.id)
})

roomConnection.addEventListener('Reveal', (event) => {
  console.log('reveal', event)
  currentState.value = 'Revealing'
  reveal.state = true
  currentVote.vote = undefined
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

  if (player.isOwner) {
    room.owner = false
  }

  if (player.id === currentPlayer.id) {
    currentPlayer.isSpectator = player.isSpectator
  }
})

const updateRoom = async (action: 'reveal' | 'reset') => {
  await fetch(`/api/rooms/${props.roomId}/${action}`)
}

const hasVotes = () => {
  return Object.keys(room.votes).length > 0
}

const copyToClipboard = (text: string) => {
  const host = window.location.host
  const protocol = window.location.protocol
  const fullUrl = `${protocol}//${host}/${text}`

  navigator.clipboard.writeText(fullUrl)
  toast('Copied to clipboard')
}

const setSpectator = async (isSpectator: boolean) => {
  return fetch(`/api/rooms/${props.roomId}/players/${currentPlayer.id}/${isSpectator ? 'spectate' : 'participate'}`)
}

const leaveRoom = async () => {
  const confirmLeave = confirm('Are you sure you want to leave the room?')
  if (!confirmLeave) {
    return
  }

  try {
    await fetch(`/api/rooms/${props.roomId}/players/${currentPlayer.id}/leave`, {
      method: 'POST',
    })
  } catch (error) {
    console.error('Failed to leave room:', error)
  }

  roomConnection.close()
  clearPlayerData()
  await router.push('/')
}
</script>

<template>
  <div class="p-6 flex flex-col gap-2 items-start justify-center lg:flex-row">
    <div class="flex flex-col items-center justify-center gap-2 w-xl min-w-lg">
      <Card v-if="room.owner" class="w-full">
        <CardHeader>
          <CardTitle>Host Tools</CardTitle>
        </CardHeader>
        <CardContent class="flex flex-col gap-2">
          <Button
            v-if="!reveal.state"
            :variant="hasVotes() ? 'default' : 'outline'"
            :disabled="!hasVotes()"
            @click="updateRoom('reveal')"
            class="cursor-pointer"
          >
            Reveal Scores
            {{
              !hasVotes()
                ? ' (Nobody has voted yet)'
                : `(${Object.keys(room.votes).length}/${room.players.filter((p) => !p.isSpectator && p.isConnected).length} voted)`
            }}
          </Button>
          <Button v-if="reveal.state" variant="default" @click="updateRoom('reset')" class="cursor-pointer">
            Next Round
          </Button>
        </CardContent>
      </Card>

      <Card v-else class="w-full">
        <CardHeader>
          <CardTitle>Player Options</CardTitle>
        </CardHeader>
        <CardContent class="flex flex-col gap-2">
          <Button
            v-if="!currentPlayer.isSpectator"
            variant="outline"
            @click="setSpectator(true)"
            class="cursor-pointer"
          >
            Spectate
          </Button>
          <Button
            v-if="currentPlayer.isSpectator"
            variant="outline"
            @click="setSpectator(false)"
            class="cursor-pointer"
          >
            Participate
          </Button>
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
            <span
              class="flex gap-2 items-center cursor-pointer"
              data-testid="JoinCode"
              @click="copyToClipboard(props.roomId)"
            >
              {{ props.roomId }}
              <Icon icon="radix-icons:copy" />
            </span>
          </p>

          <h3 class="font-bold">{{ currentState }}</h3>
        </div>
      </PlayerList>

      <Card class="w-full">
        <CardContent class="flex flex-col gap-2">
          <Button variant="secondary" @click="leaveRoom" class="cursor-pointer">Leave Room</Button>
        </CardContent>
      </Card>
    </div>

    <div class="w-full max-w-7xl">
      <VotingTable v-if="!reveal.state" :roomId="props.roomId" />
      <VotingResult v-if="reveal.state" :players="room.players" :votes="room.votes" />
    </div>
  </div>
</template>

<style scoped></style>
