// priorities data come from packages\platform\src\components\priorities.json
import { TFunction } from 'react-i18next'

export default function getPriorityTranslatedName(t: TFunction, id: number) {
  switch (id) {
    case 1:
      return t('plainText.critical')
    case 2:
      return t('plainText.high')
    case 3:
      return t('plainText.medium')
    case 4:
      return t('plainText.low')
    default:
      return null
  }
}
