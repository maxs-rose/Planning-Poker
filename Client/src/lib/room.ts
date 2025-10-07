import { reactive } from 'vue'
import { useRouter } from 'vue-router'
import type { Room } from '@/lib/model/room.interface.ts'
import type { Vote } from '@/lib/model/vote.interface.ts'

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
  const result = await fetch(`/api/rooms/${joinCode}`, { method: 'HEAD' })

  return result.status === 404
}

export const roomConnect = (joinCode: string, name: string) => {
  const url = new URL(`${window.origin}/api/rooms/${joinCode}`)

  return new EventSource(url)
}

export const joinRoom = async (
  joinCode: string,
  playerId: string,
  name: string,
  owner: boolean,
): Promise<Room & { playerId: string; votes: Vote[] }> => {
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

export const vote = reactive<{ vote: undefined | number }>({ vote: undefined })
