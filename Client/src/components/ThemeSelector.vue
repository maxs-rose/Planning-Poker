<script lang="ts" setup>
import { Icon } from '@iconify/vue'
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from '@/components/ui/dropdown-menu'
import { Button } from '@/components/ui/button'
import { useColorMode } from '@vueuse/core'

const mode = useColorMode()
</script>

<template>
  <DropdownMenu>
    <DropdownMenuTrigger as-child>
      <Button variant="outline">
        <Icon
          class="h-[1.2rem] w-[1.2rem] rotate-0 scale-100 transition-all dark:-rotate-90 dark:scale-0"
          icon="radix-icons:moon"
        />
        <Icon
          class="absolute h-[1.2rem] w-[1.2rem] rotate-90 scale-0 transition-all dark:scale-100 dark:rotate-0"
          icon="radix-icons:sun"
        />
        <span class="sr-only">Toggle Theme</span>
      </Button>
    </DropdownMenuTrigger>
    <DropdownMenuContent>
      <DropdownMenuItem @click="mode = 'light'">Light</DropdownMenuItem>
      <DropdownMenuItem @click="mode = 'dark'">Dark</DropdownMenuItem>
      <DropdownMenuItem @click="mode = 'auto'">Auto</DropdownMenuItem>
    </DropdownMenuContent>
  </DropdownMenu>
</template>
