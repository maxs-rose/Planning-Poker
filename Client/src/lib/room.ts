import { reactive } from 'vue'
import { useRouter } from 'vue-router'
import type { Room } from '@/lib/model/room.interface.ts'
import type { Init } from '@/lib/model/init.interface.ts'

export const createRoom = async (name: string): Promise<{ joinCode: string }> => {
  const result = await fetch('/api/rooms/create', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      name,
    }),
  })

  return result.json()
}

export const roomExists = async (joinCode: string): Promise<boolean> => {
  try {
    const result = await fetch(`/api/rooms/${joinCode}`, { method: 'HEAD' })

    return result.status !== 404
  } catch {
    return false
  }
}

export const roomConnect = (joinCode: string) => {
  room.id = joinCode
  const url = new URL(`${window.origin}/api/rooms/${room.id}`)

  return new EventSource(url)
}

export const joinRoom = async (joinCode: string, playerId: string, name: string, owner: boolean): Promise<Init> => {
  const result = await fetch(`/api/rooms/${joinCode}/join`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      id: playerId,
      name,
      spectator: false,
      owner,
    }),
  })

  if (result.status !== 200) {
    const router = useRouter()
    currentPlayer.name = undefined
    currentPlayer.id = undefined
    currentPlayer.isSpectator = undefined
    await router.push('/')

    throw new Error('Failed to join room')
  }

  return result.json()
}

export const currentPlayer: { name?: string; id?: string; isSpectator?: boolean } = reactive({})
export const room: Room & { id: string } = reactive({
  id: '',
  friendlyName: '',
  owner: false,
  players: [],
  votes: {},
})

export const processRoomState = (roomState: Init) => {
  currentPlayer.id = roomState.playerId
  room.friendlyName = roomState.friendlyName
  room.owner = roomState.owner
  room.players = roomState.players
  room.votes = Object.fromEntries(roomState.votes.map((vote) => [vote.voter, vote.value]))
}

export const currentVote = reactive<{ vote: undefined | number }>({ vote: undefined })

export const vote = async (value: number) => {
  await fetch(`/api/rooms/${room.id}/players/${currentPlayer.id}/vote`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      value,
    }),
  })

  currentVote.vote = value
}
