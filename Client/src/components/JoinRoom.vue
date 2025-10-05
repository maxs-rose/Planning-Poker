<script lang="ts" setup>
import { toTypedSchema } from '@vee-validate/zod'
import { z } from 'zod'
import { useForm } from 'vee-validate'
import { FormControl, FormDescription, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { currentPlayer } from '@/lib/room.ts'
import { useRouter } from 'vue-router'

const joinCode = useRouter().currentRoute.value.query.joinCode

const joinSchema = toTypedSchema(
  z.object({
    roomId: z
      .string()
      .min(1, 'Join code is required')
      .default(typeof joinCode === 'string' ? joinCode || '' : ''),
    playerName: z.string().min(1, 'Player name is required').default(''),
  }),
)

const router = useRouter()
const joinForm = useForm({ validationSchema: joinSchema })

const joinRoomSubmit = joinForm.handleSubmit(async (values) => {
  currentPlayer.name = values.playerName
  currentPlayer.id = undefined

  await router.push(`/${values.roomId}`)
})
</script>

<template>
  <form @submit="joinRoomSubmit">
    <Card>
      <CardHeader>
        <CardTitle>Join a Room</CardTitle>
      </CardHeader>
      <CardContent>
        <FormField v-slot="{ componentField }" name="roomId">
          <FormItem class="w-full">
            <FormLabel>Join Code</FormLabel>
            <FormControl>
              <Input placeholder="quantify-roadster-southchesley" type="text" v-bind="componentField" />
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
        <Button class="w-full" type="submit">Join Room</Button>
      </CardFooter>
    </Card>
  </form>
</template>

<style scoped></style>
