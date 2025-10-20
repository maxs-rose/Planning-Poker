<script lang="ts" setup>
import { toTypedSchema } from '@vee-validate/zod'
import { z } from 'zod'
import { useForm } from 'vee-validate'
import { FormControl, FormDescription, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { createRoom, currentPlayer } from '@/lib/room.ts'
import { useRouter } from 'vue-router'

const createSchema = toTypedSchema(
  z.object({
    roomName: z.string().min(1, 'Room name is required').default(''),
    playerName: z.string().min(1, 'Player name is required').default(''),
  }),
)

const router = useRouter()
const createForm = useForm({ validationSchema: createSchema })

const createRoomSubit = createForm.handleSubmit(async (values) => {
  const room = await createRoom(values.roomName)
  currentPlayer.name = values.playerName

  console.log('Room creation result', room)
  await router.push(`/${room.joinCode}`)
})
</script>

<template>
  <form @submit="createRoomSubit">
    <Card>
      <CardHeader>
        <CardTitle>Create a Room</CardTitle>
      </CardHeader>

      <CardContent>
        <FormField v-slot="{ componentField }" name="roomName">
          <FormItem class="w-full mb-4">
            <FormLabel>Room Name</FormLabel>
            <FormControl>
              <Input placeholder="The Inventory" type="text" v-bind="componentField" />
            </FormControl>
            <FormMessage />
          </FormItem>
        </FormField>

        <FormField v-slot="{ componentField }" name="playerName">
          <FormItem class="w-full">
            <FormLabel>Player Name</FormLabel>
            <FormControl>
              <Input placeholder="Claptrap" type="text" v-bind="componentField" />
            </FormControl>
            <FormDescription>Your display name</FormDescription>
            <FormMessage />
          </FormItem>
        </FormField>
      </CardContent>

      <CardFooter>
        <Button class="w-full" data-testid="CreateRoom" type="submit">Create Room</Button>
      </CardFooter>
    </Card>
  </form>
</template>
