<script setup lang="ts">
import { getJiraResources, getJiraUser, searchJiraTickets } from '@/lib/jira.ts'
import { addToTicketQueue, modifyTicketQueue, room } from '@/lib/room.ts'
import { Icon } from '@iconify/vue'
import { ComboboxContent, ComboboxInput, ComboboxItem, ComboboxPortal, ComboboxRoot } from 'reka-ui'
import { onMounted, ref } from 'vue'
import type { JiraResource, JiraTicketResult, JiraTicketSearchResults } from '@/lib/model/jira.interface.ts'
import {
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogOverlay,
  DialogPortal,
  DialogRoot,
  DialogTitle,
  DialogTrigger,
} from 'reka-ui'
import type { Ticket } from '@/lib/model/ticket.interface.ts'
import { toast } from 'vue-sonner'
import { Button } from '@/components/ui/button'
import { type SortableEvent, VueDraggable } from 'vue-draggable-plus'

const isLoggedIn = ref(false)

const organisations = ref<JiraResource[]>()
const selectedOrganisation = ref<JiraResource>()

const searchSuggestions = ref<JiraTicketSearchResults>()
const hideSearchSuggestions = ref<boolean>(false)
const selectedSearchSuggestions = ref<JiraTicketResult[]>([])

onMounted(async () => {
  isLoggedIn.value = await getJiraUser()
  if (isLoggedIn.value) {
    organisations.value = await getJiraResources()
    if (organisations.value.length === 1) {
      selectedOrganisation.value = organisations.value[0]
    }
  }
})

const updateSearchSuggestions = async (searchText: string) => {
  searchSuggestions.value = await searchJiraTickets(
    selectedOrganisation.value!.id,
    selectedOrganisation.value!.url,
    searchText,
  )
}

const queueTickets = async () => {
  const ticketIds = selectedSearchSuggestions.value.map((ticket) => ticket.id.toString())
  try {
    const response = await addToTicketQueue(selectedOrganisation.value!.id, ticketIds)
    if (!response) {
      toast('Error adding tickets to the queue.')
    }
  } catch (error) {
    console.log(error)
    toast('Error adding tickets to the queue.')
  }
  resetSearchSuggestions()
}

const reorderTickets = async (fromIndex: number, toIndex?: number) => {
  try {
    const response = await modifyTicketQueue(fromIndex, toIndex)
    if (!response) {
      toast('Error modifying the queue. Verify you are not interfering with the currently active ticket.')
    }
  } catch (error) {
    console.log(error)
    toast('Error modifying the queue.')
  }
}

const resetSearchSuggestions = () => {
  selectedSearchSuggestions.value = []
  searchSuggestions.value = undefined
}

const handleSearchBoxFocus = () => {
  hideSearchSuggestions.value = false
  if (searchSuggestions.value === undefined) {
    updateSearchSuggestions('')
  }
}

const handleSearchBoxBlur = (event: FocusEvent) => {
  const searchInput = event.target as HTMLInputElement
  if (event.relatedTarget instanceof Element && (searchInput.parentElement?.contains(event.relatedTarget!) ?? false))
    return

  if ((searchInput?.value ?? undefined) === '') hideSearchSuggestions.value = true
}

const dragEndHandler = (event: SortableEvent) => {
  console.log('dragEndHandler: old ', event.oldIndex, ', new ', event.newIndex)
  if (event.oldIndex !== undefined && event.newIndex !== undefined && event.oldIndex !== -1) {
    reorderTickets(event.oldIndex, event.newIndex)
  }
}

const discarded: Ticket[] = [] // Prevent 2-way binding from modifying room.ticketQueue
</script>

<template>
  <div v-if="isLoggedIn || (room.ticketQueue?.length ?? 0) > 0" class="border rounded-md overflow-y-auto">
    <div class="p-2 sm:p-4">
      <div v-if="(organisations?.length ?? 0) !== 1">
        <h4 class="font-semibold">Select an organisation</h4>

        <ComboboxRoot v-model="selectedOrganisation">
          <ComboboxInput :display-value="(v) => (v ? `Searching in ${v.name}` : `Select an organisation`)" />
          <ComboboxPortal>
            <ComboboxContent>
              <ComboboxItem v-for="org in organisations" :key="org.id" :value="org">
                {{ org.name }}
              </ComboboxItem>
            </ComboboxContent>
          </ComboboxPortal>
        </ComboboxRoot>
      </div>
      <div v-else>
        <h4 class="font-semibold">Searching in {{ selectedOrganisation?.name }}</h4>
      </div>
      <div v-if="selectedOrganisation !== undefined">
        <DialogRoot>
          <DialogTrigger>
            <Button class="mt-2 cursor-pointer">Add ticket</Button>
          </DialogTrigger>
          <DialogPortal>
            <DialogOverlay class="bg-[#000000A9] data-[state=open]:animate-overlayShow fixed inset-0 z-30" />
            <DialogContent
              class="data-[state=open]:animate-contentShow fixed top-[50%] left-[50%] max-h-[85vh] w-[90vw] max-w-2xl translate-x-[-50%] translate-y-[-50%] rounded-[6px] bg-white p-[25px] focus:outline-none z-[100] overflow-y-auto"
            >
              <DialogTitle class="text-mauve12 m-0 text-[17px] font-semibold"> Add a ticket </DialogTitle>
              <DialogDescription class="text-mauve11 mt-[10px] mb-5 text-sm leading-normal">
                Add a new item to the list of tickets that will be shown to the team members.
              </DialogDescription>
              <div>
                <input
                  id="ticket-name"
                  class="w-full py-4 px-2 md:px-4 border-2 border-gray-600 rounded-t-lg outline-none"
                  :class="{ 'rounded-b-lg': hideSearchSuggestions }"
                  placeholder="Search like you would on Jira... :)"
                  @input="(event) => updateSearchSuggestions((event.target as HTMLInputElement)?.value)"
                  @focus="handleSearchBoxFocus"
                  @blur="handleSearchBoxBlur"
                />
                <div
                  v-if="!hideSearchSuggestions"
                  class="w-full border-2 border-t-0 rounded-b-lg max-h-100 overflow-y-auto"
                >
                  <div
                    v-for="suggestion in searchSuggestions?.results"
                    class="inline-flex w-full items-center gap-2 p-2 md:px-4 not-last:border-b cursor-pointer hover:bg-gray-200"
                    @click="selectedSearchSuggestions.push(suggestion)"
                    tabindex="0"
                  >
                    <img :src="suggestion.typeAvatarUrl" width="24" height="24" :alt="suggestion.type" />
                    <div class="w-full">
                      <span class="font-bold">{{ suggestion.key }}:</span> {{ suggestion.matchSummary }}
                    </div>
                  </div>
                  <div
                    v-if="(searchSuggestions?.results?.length ?? 0) === 0"
                    class="inline-flex w-full items-center p-2 md:px-4"
                  >
                    No search results found. Please try again.
                  </div>
                </div>
              </div>

              <h4 class="mt-4 text-md font-semibold">Selected tickets</h4>
              <div class="w-full mt-2 border-2 rounded-lg max-h-100" v-if="selectedSearchSuggestions?.length > 0">
                <div
                  v-for="(suggestion, index) in selectedSearchSuggestions"
                  class="inline-flex w-full items-center gap-2 p-2 md:px-4 not-last:border-b cursor-pointer hover:bg-gray-200"
                  @click="selectedSearchSuggestions.splice(index, 1)"
                  tabindex="0"
                >
                  <img :src="suggestion.typeAvatarUrl" width="24" height="24" :alt="suggestion.type" />
                  <div class="w-full">
                    <span class="font-bold">{{ suggestion.key }}:</span> {{ suggestion.matchSummary }}
                  </div>
                  <div>
                    <Icon icon="lucide:x" />
                  </div>
                </div>
              </div>
              <div class="mt-[25px] flex justify-end">
                <DialogClose as-child>
                  <Button @click="queueTickets" class="cursor-pointer">
                    Add selected ticket{{ (selectedSearchSuggestions?.length ?? 0) > 1 ? 's' : '' }}
                  </Button>
                </DialogClose>
              </div>
              <DialogClose
                class="text-grass11 hover:bg-green4 focus:shadow-green7 absolute top-[10px] right-[10px] inline-flex h-[25px] w-[25px] appearance-none items-center justify-center rounded-full focus:shadow-[0_0_0_2px] focus:outline-none"
                aria-label="Close"
              >
                <Icon icon="lucide:x" />
              </DialogClose>
            </DialogContent>
          </DialogPortal>
        </DialogRoot>
      </div>
    </div>
    <div class="border-t" v-if="(room.ticketQueue?.length ?? 0) > 0">
      <h4 class="p-2 sm:p-4 font-semibold">Queued tickets</h4>

      <VueDraggable v-model="discarded" ghost-class="ghost" @end="dragEndHandler">
        <div
          v-for="(ticket, index) in room.ticketQueue"
          :key="ticket.id"
          class="inline-flex w-full items-center gap-2 p-2 sm:px-4 not-last:border-b hover:bg-gray-200"
          :class="{
            'bg-blue-200': (room.ticketIndex ?? -1) === index,
            'odd:bg-gray-100': (room.ticketIndex ?? -1) !== index,
          }"
          tabindex="0"
        >
          <div class="text-sm font-semibold text-gray-500">{{ index + 1 }}</div>
          <img :src="ticket.icon" width="24" height="24" :alt="ticket.typeName" />
          <div class="w-full">
            <span class="font-bold">{{ ticket.key }}:</span> {{ ticket.title }}
          </div>
          <div @click="reorderTickets(index)" class="cursor-pointer" title="Remove item">
            <Icon icon="lucide:x" />
          </div>
        </div>
      </VueDraggable>
    </div>
  </div>
  <div v-else class="pt-2 italic text-center text-gray-500">Log into Jira from the Home page to select tickets.</div>
</template>

<style scoped></style>
