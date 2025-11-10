import type { Player } from '@/lib/model/player.interface.ts'
import type { Ticket } from '@/lib/model/ticket.interface.ts'

export interface Room {
  friendlyName: string
  owner: boolean
  players: Player[]
  votes: Record<string, number>
  revealed: boolean
  ticketQueue?: Ticket[]
  ticketIndex?: number
  ticket: Ticket
}
