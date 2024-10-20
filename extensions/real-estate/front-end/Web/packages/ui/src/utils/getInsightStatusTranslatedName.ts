// insight status data come from packages\platform\src\components\insightStatuses.json
import { TFunction } from 'react-i18next'

export default function getInsightStatusTranslatedName(
  t: TFunction,
  id: string
) {
  switch (id) {
    case 'open':
      return t('headers.open')
    case 'archived':
      return t('headers.archived')
    case 'inProgress':
      return t('plainText.inProgress')
    case 'resolved':
      return t('headers.resolved')
    case 'new':
      return t('beamer.new')
    case 'acknowledged':
      return t('headers.acknowledged')
    case 'closed':
      return t('headers.closed')
    case 'deleted':
      return t('plainText.deleted')
    case 'ignored':
      return t('headers.ignored')
    case 'readyToResolve':
      return t('plainText.readyToResolve')
    default:
      return null
  }
}
