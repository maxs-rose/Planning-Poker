import type { Player } from '@/lib/model/player.interface.ts'

export interface Room {
  friendlyName: string
  owner: boolean
  players: Player[]
  votes: Record<string, number>
}
