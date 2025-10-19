<script lang="ts" setup>
import type { Player } from '@/lib/model/player.interface.ts'
import { Pie } from 'vue-chartjs'
import { ArcElement, Chart as ChartJS, type ChartOptions, Colors, Tooltip } from 'chart.js'
import ChartDataLabels from 'chartjs-plugin-datalabels'

ChartJS.register(ArcElement, Tooltip, Colors, ChartDataLabels)

const HUE_BASE = 200
const SATURATION = 65
const LIGHTNESS_MIN = 15
const LIGHTNESS_MAX = 50

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

const sortedEntries = Object.entries(data).sort((a, b) => a[1] - b[1])

const uniqueVoteCounts = [...new Set(sortedEntries.map(([, count]) => count))].sort((a, b) => a - b)

const generateColorsForVoteCounts = (uniqueCounts: number[]) => {
  const colorMap: Record<number, string> = {}

  uniqueCounts.forEach((count, i) => {
    const lightness = LIGHTNESS_MIN + (i * LIGHTNESS_MAX) / Math.max(uniqueCounts.length - 1, 1)
    colorMap[count] = `hsl(${HUE_BASE}, ${SATURATION}%, ${lightness}%)`
  })

  return colorMap
}

const colorMap = generateColorsForVoteCounts(uniqueVoteCounts)

const countOccurrences: Record<number, number> = {}

const colors = sortedEntries.map(([, count]) => {
  const baseColor = colorMap[count]

  if (!countOccurrences[count]) {
    countOccurrences[count] = 0
  }

  const occurrence = countOccurrences[count]
  countOccurrences[count]++

  const darkenAmount = occurrence * 4
  const match = baseColor.match(/hsl\((\d+),\s*(\d+)%,\s*(\d+)%\)/)

  if (match) {
    const [h, s, l] = match
    const newLightness = Math.max(parseInt(l) - darkenAmount, 5)
    return `hsl(${h}, ${s}%, ${newLightness}%)`
  }

  return baseColor
})

const labels = sortedEntries.map(([key]) => key)
const values = sortedEntries.map(([, value]) => value)

const total = Object.values(data).reduce((acc, x) => acc + x, 0)
const percentage = Object.entries(data)
  .map(([k, v]) => ({ k, v: Math.trunc((v / total) * 100) }))
  .sort((a, b) => b.v - a.v)

const chartData = {
  labels: labels,
  datasets: [
    {
      data: values,
      backgroundColor: colors,
      borderColor: colors.map((c) => c.replace(/\d+%\)$/, '20%)')),
      borderWidth: 2,
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
      enabled: false,
    },
    datalabels: {
      color: '#fff',
      font: {
        size: 16,
        weight: 'bold',
      },
      formatter: (value: number, context: any) => {
        const label = context.chart.data.labels[context.dataIndex]
        return `${label} (${Math.trunc((value / total) * 100)}%)`
      },
    },
  },
}
</script>

<template>
  <div class="flex flex-col xl:flex-row w-full justify-center items-center gap-6">
    <div class="grid grid-cols-2 gap-2">
      <div class="font-bold">Score</div>
      <div class="font-bold">Share</div>
      <template v-for="entry in percentage" :key="entry.k">
        <div class="font-bold">{{ entry.k }}</div>
        <div>{{ entry.v }}%</div>
      </template>
    </div>

    <div class="w-[30rem] h-[30rem]">
      <Pie id="planning-results" :data="chartData" :options="chartOptions" />
    </div>
  </div>
</template>
