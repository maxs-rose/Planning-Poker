import type { JiraResource, JiraTicketSearchResults } from '@/lib/model/jira.interface.ts'

export const getJiraUser = async (): Promise<number> => {
  try {
    const result = await fetch(`/api/jira/user`, { method: 'HEAD' })
    return result.status
  } catch {
    return 999
  }
}

export const getJiraResources = async (): Promise<JiraResource[]> => {
  try {
    const result = await fetch(`/api/jira/resources`, { method: 'GET' })
    return await result.json()
  } catch {
    return []
  }
}

export const searchJiraTickets = async (
  resourceId: string,
  resourceUrl: string,
  query: string,
): Promise<JiraTicketSearchResults> => {
  try {
    const params = new URLSearchParams()
    params.append('resourceId', resourceId)
    params.append('resourceUrl', resourceUrl)
    params.append('query', query)
    const result = await fetch(`/api/jira/issues?${params}`, { method: 'GET' })
    return await result.json()
  } catch {
    return {
      results: [],
      noSuggestionsFoundMessage: 'Error calling the tickets API',
    }
  }
}
