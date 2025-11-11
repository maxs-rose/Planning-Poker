import { reactive, watch } from 'vue'
import type { Room } from '@/lib/model/room.interface.ts'
import type { Init } from '@/lib/model/init.interface.ts'
import type { ModifyTicketQueueResult, Ticket } from '@/lib/model/ticket.interface.ts'

const loadPlayerFromStorage = () => {
  try {
    const stored = localStorage.getItem('planningPoker_currentPlayer')
    return stored ? JSON.parse(stored) : {}
  } catch {
    return {}
  }
}

const loadRoomId = () => {
  try {
    return localStorage.getItem('planningPoker_roomId') || ''
  } catch {
    return ''
  }
}

const savePlayerToStorage = (player: { name?: string; id?: string; isSpectator?: boolean }) => {
  try {
    localStorage.setItem('planningPoker_currentPlayer', JSON.stringify(player))
  } catch (error) {
    console.error('Failed to save player to storage', error)
  }
}

export const currentPlayer: { name?: string; id?: string; isSpectator?: boolean } = reactive(loadPlayerFromStorage())

watch(
  currentPlayer,
  (newPlayer) => {
    savePlayerToStorage(newPlayer)
  },
  { deep: true },
)

export const room: Room & { id: string } = reactive({
  id: loadRoomId(),
  friendlyName: '',
  owner: false,
  players: [],
  votes: {},
  revealed: false,
  ticket: {
    id: '',
    key: '',
    typeName: '',
    title: '',
    icon: '',
    description: '',
    url: '',
    labels: [],
  },
})

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
  try {
    localStorage.setItem('planningPoker_roomId', joinCode)
  } catch (error) {
    console.error('Failed to save room ID to storage', error)
  }

  const url = new URL(`${window.origin}/api/rooms/${room.id}`)

  if (currentPlayer.id) {
    url.searchParams.set('playerId', currentPlayer.id)
  }

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

  if (result.status === 404) {
    throw new Error('Room not found')
  }

  if (result.status !== 200) {
    throw new Error('Failed to join room')
  }

  return result.json()
}

export const processRoomState = (roomState: Init) => {
  currentPlayer.id = roomState.playerId
  room.friendlyName = roomState.friendlyName
  room.owner = roomState.owner
  room.players = roomState.players
  room.ticketQueue = roomState.ticketQueue
  room.ticketIndex = roomState.ticketIndex
  room.ticket = roomState.ticket

  room.votes = Object.fromEntries(
    roomState.votes.filter((vote) => vote.value !== null).map((vote) => [vote.voter, vote.value]),
  )
  room.revealed = roomState.revealed

  if (currentPlayer.id && room.votes[currentPlayer.id] !== undefined && room.votes[currentPlayer.id] !== null) {
    currentVote.vote = room.votes[currentPlayer.id]
  }
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

export const addToTicketQueue = async (resourceId: string, ticketIds: string[]): Promise<boolean> => {
  const params = new URLSearchParams()
  params.append('resourceId', resourceId)
  ticketIds.forEach((ticketId: string) => params.append('ids', ticketId))

  const result = await fetch(`/api/rooms/${room.id}/queue?${params}`, { method: 'POST' })
  const queue: ModifyTicketQueueResult = await result.json()
  room.ticketQueue = queue.tickets
  return queue.success
}

export const modifyTicketQueue = async (fromIndex: number, toIndex?: number): Promise<boolean> => {
  const params = new URLSearchParams()
  params.append('fromIndex', fromIndex.toFixed())
  if (toIndex !== undefined) params.append('toIndex', toIndex.toFixed())

  const result = await fetch(`/api/rooms/${room.id}/modifyQueue?${params}`, { method: 'POST' })
  const queue: ModifyTicketQueueResult = await result.json()
  room.ticketQueue = queue.tickets
  return queue.success
}

export const clearPlayerData = () => {
  currentPlayer.name = undefined
  currentPlayer.id = undefined
  currentPlayer.isSpectator = undefined
  room.id = ''

  try {
    localStorage.removeItem('planningPoker_currentPlayer')
    localStorage.removeItem('planningPoker_roomId')
  } catch (error) {
    console.error('Failed to clear storage', error)
  }
}
