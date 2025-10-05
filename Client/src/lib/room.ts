import { reactive } from 'vue'

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

export const joinRoom = (joinCode: string, name: string) => {
  const url = new URL(`${window.origin}/api/rooms/${joinCode}/join`)
  url.searchParams.append('name', name)

  return new EventSource(url)
}

export const currentPlayer: { name?: string; id?: string; isSpectator?: boolean } = reactive({})

export const vote = reactive<{ vote: undefined | number }>({ vote: undefined })
