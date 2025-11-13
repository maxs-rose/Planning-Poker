export interface JiraResource {
  id: string
  name: string
  url: string
  scopes: string[]
  avatarUrl: string
}

export interface JiraTicketResult {
  id: number
  type: string
  typeAvatarUrl: string
  key: string
  matchSummary: string
}

export interface JiraTicketSearchResults {
  noSuggestionsFoundMessage?: string
  results: JiraTicketResult[]
}
