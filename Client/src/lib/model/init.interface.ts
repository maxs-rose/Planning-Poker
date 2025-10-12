import type { Room } from '@/lib/model/room.interface.ts'
import type { Vote } from '@/lib/model/vote.interface.ts'

export type Init = Room & { playerId: string; votes: Vote[] }
