import { Frequency } from './types'

const frequencyLabels: { [key in Frequency]: string } = {
  weekly: 'plainText.weeks',
  monthly: 'plainText.months',
  yearly: 'plainText.years',
}

export default frequencyLabels
