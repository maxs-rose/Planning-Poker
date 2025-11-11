export interface Ticket {
  id: string
  key: string
  typeName: string
  title: string
  icon: string
  description: string
  url: string
  labels: string[]
}

export interface ModifyTicketQueueResult {
  success: boolean
  tickets: Ticket[]
}
