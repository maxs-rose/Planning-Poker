<script lang="ts" setup>
import type { Player } from '@/lib/model/player.interface.ts'
import { Pie } from 'vue-chartjs'
import { ArcElement, Chart as ChartJS, type ChartOptions, Colors, Tooltip } from 'chart.js'

ChartJS.register(ArcElement, Tooltip, Colors)

const props = defineProps<{
  players: Player[]
  votes: Record<string, number>
}>()

const data = Object.values(props.votes)
  .filter((v) => !!v)
  .reduce(
    (acc, x) => {
      acc[x] = acc[x] ? acc[x] + 1 : 1

      return acc
    },
    {} as Record<number, number>,
  )
const total = Object.values(data).reduce((acc, x) => acc + x, 0)

const chartData = {
  labels: Object.keys(data),
  datasets: [
    {
      data: Object.values(data),
    },
  ],
}

const chartOptions: ChartOptions<'pie'> = {
  responsive: true,
  plugins: {
    legend: {
      display: false,
    },
    colors: {
      enabled: true,
    },
  },
}
</script>

<template>
  <div class="flex flex-row w-full justify-center items-center gap-6">
    <div class="grid grid-cols-2 gap-2">
      <div class="font-bold">Score</div>
      <div class="font-bold">Share</div>

      <template v-for="entry in Object.entries(data)">
        <div class="font-bold">{{ entry[0] }}</div>
        <div>{{ Math.trunc((entry[1] / total) * 100) }}%</div>
      </template>
    </div>

    <div class="w-[30rem] h-[30rem]">
      <Pie id="planning-results" :data="chartData" :options="chartOptions" />
    </div>
  </div>
</template>
