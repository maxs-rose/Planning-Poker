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
const percentage = Object.entries(data)
  .map(([k, v]) => ({ k, v: Math.trunc((v / total) * 100) }))
  .sort((a, b) => b.v - a.v)

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
  <div class="flex flex-col xl:flex-row w-full justify-center items-center gap-6">
    <div class="grid grid-cols-2 gap-2">
      <div class="font-bold">Score</div>
      <div class="font-bold">Share</div>

      <template v-for="entry in percentage">
        <div class="font-bold">{{ entry.k }}</div>
        <div>{{ entry.v }}%</div>
      </template>
    </div>

    <div class="w-[30rem] h-[30rem]">
      <Pie id="planning-results" :data="chartData" :options="chartOptions" />
    </div>
  </div>
</template>
