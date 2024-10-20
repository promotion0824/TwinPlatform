import { TemperatureUnit } from '@willow/common'

// This type is incomplete, but is sufficient for these components
export type User = {
  clearAllOptions: () => void
  firstName: string
  lastName: string
  logout: () => void
  name: string
  preferences?: { language: string }
  options: { temperatureUnit: TemperatureUnit }
  saveLanguage: (language: string) => void
  saveOptions: (key: string, value: string) => void
}
