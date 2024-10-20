import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import getInsightStatusTranslatedName from '../getInsightStatusTranslatedName.ts'

i18n.use(initReactI18next).init({
  fallbackLng: 'en',
  lng: 'en',
  resources: {},
})

describe('getInsightStatusTranslatedName', () => {
  test('should return translated open status', () => {
    expect(getInsightStatusTranslatedName(i18n.t, 'open')).toEqual(
      'headers.open'
    )
  })

  test('should return translated inProgress status', async () => {
    expect(getInsightStatusTranslatedName(i18n.t, 'inProgress')).toEqual(
      'plainText.inProgress'
    )
  })

  test('should return translated acknowledged status', async () => {
    expect(getInsightStatusTranslatedName(i18n.t, 'acknowledged')).toEqual(
      'headers.acknowledged'
    )
  })

  test('should return translated closed status', async () => {
    expect(getInsightStatusTranslatedName(i18n.t, 'closed')).toEqual(
      'headers.closed'
    )
  })

  test('should return null if invalid status', async () => {
    expect(getInsightStatusTranslatedName(i18n.t, 'invalid')).toBeNull()
  })
})
