<script setup lang="ts">
import { room } from '@/lib/room.ts'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { onMounted, ref } from 'vue'
import { getJiraResources, getJiraUser } from '@/lib/jira.ts'
import { Icon } from '@iconify/vue'

const showTicketDetails = ref<boolean>(false)

const isLoggedIn = ref(true)
const cancelWarningKey = 'cancelLoginWarning'
const dismissLoginWarning = ref((localStorage.getItem(cancelWarningKey) ?? '') === 'true')

const saveLoginWarningPreference = () => {
  localStorage.setItem(cancelWarningKey, 'true')
  dismissLoginWarning.value = true
}

onMounted(async () => {
  isLoggedIn.value = await getJiraUser()
})
</script>

<template>
  <Card v-if="room.ticket !== undefined && room.ticket.id.length > 0">
    <CardContent class="flex flex-col w-full gap-2">
      <div
        v-if="!isLoggedIn && !dismissLoginWarning"
        class="flex items-center p-2 lg:px-4 font-semibold bg-yellow-200 dark:bg-yellow-950 border-s-3 border-yellow-500 dark:border-yellow-700"
      >
        <div class="grow">
          Note: you are not logged in to Jira, so you may experience issues with resources such as images on this view.
        </div>
        <button @click="saveLoginWarningPreference" class="cursor-pointer p-1">
          <Icon icon="lucide:x" />
        </button>
      </div>
      <div class="inline-flex w-full gap-2">
        <div class="min-w-5 pt-1">
          <img
            :src="room.ticket.icon"
            :alt="room.ticket.typeName"
            :title="room.ticket.typeName"
            class="text-[0px] w-5 h-5"
            onerror="this.onerror=null;this.src='/src/assets/empty_icon.svg'"
          />
        </div>

        <h4 class="text-xl grow align-middle">
          <span class="font-semibold">{{ room.ticket.key }}:</span> {{ room.ticket.title }}
        </h4>
        <a :href="room.ticket.url" target="_blank" rel="noopener noreferrer">
          <Button variant="outline" class="cursor-pointer"> More </Button>
        </a>
      </div>
      <div v-if="room.ticket.labels.length > 0" class="inline-flex w-full gap-2">
        <span v-for="label in room.ticket.labels" class="bg-blue-600 text-white rounded-full px-2.5 py-0.5">{{
          label
        }}</span>
      </div>
      <div>
        <div class="overflow-y-hidden" :class="{ 'max-h-20': !showTicketDetails }">
          <div
            class="wrap-break-word [&_p]:mb-1.5 [&_a]:text-blue-700 dark:[&_a]:text-blue-400 [&_hr]:my-2 [&_ul>li]:list-disc [&_ul>li]:list-inside [&_li]:mb-2"
            v-html="room.ticket.description"
          ></div>
        </div>
        <div class="w-full relative min-h-6">
          <div class="w-full absolute" :class="{ '-top-5': !showTicketDetails }">
            <div class="flex relative h-10 w-full justify-center">
              <div
                v-if="!showTicketDetails"
                class="w-full absolute h-5 bg-linear-to-b from-white/0 to-gray-200/60 dark:from-black/0 dark:to-gray-600/60"
              ></div>
              <hr class="w-full absolute top-5" />
              <Button
                variant="outline"
                class="cursor-pointer z-10 self-center dark:bg-background dark:hover:bg-gray-700"
                @click="showTicketDetails = !showTicketDetails"
              >
                Show {{ showTicketDetails ? 'less' : 'more' }}
              </Button>
            </div>
          </div>
        </div>
      </div>
    </CardContent>
  </Card>
</template>

<style scoped></style>
