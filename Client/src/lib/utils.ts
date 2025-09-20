import type { ClassValue } from 'clsx'
import { clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

const generatedNumbers: number[] = []

export const getFibbonacciNumber = (index: number): number => {
  switch (index) {
    case 0:
      return 0
    case 1:
      return 1
    default:
      if (generatedNumbers[index]) {
        return generatedNumbers[index]
      }

      const result = getFibbonacciNumber(index - 1) + getFibbonacciNumber(index - 2)
      generatedNumbers[index] = result

      return result
  }
}
