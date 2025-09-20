<script lang="ts" setup>
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import JoinRoom from '@/components/JoinRoom.vue'
import CreateRoom from '@/components/CreateRoom.vue'
</script>

<template>
  <div class="w-full flex justify-center items-center pt-25">
    <Tabs class="w-[400px]" default-value="join">
      <TabsList class="grid w-full grid-cols-2">
        <TabsTrigger value="join">Join Room</TabsTrigger>
        <TabsTrigger value="create">Create Room</TabsTrigger>
      </TabsList>

      <TabsContent value="join">
        <JoinRoom />
      </TabsContent>
      <TabsContent value="create">
        <CreateRoom />
      </TabsContent>
    </Tabs>
  </div>
</template>
