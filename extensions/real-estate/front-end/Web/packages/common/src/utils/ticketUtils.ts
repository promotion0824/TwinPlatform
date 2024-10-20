import { formatDateTime } from '@willow/common'
import { Language } from '@willow/ui/providers/LanguageProvider/LanguageJson/LanguageJsonService/LanguageJsonService'
import { TFunction } from 'i18next'
import _ from 'lodash'
import {
  DependentDiagnostic,
  InsightDiagnosticResponse,
  TicketSimpleDto,
} from '../../../platform/src/services/Tickets/TicketsService'
import { SyncStatus } from '../ticketStatus/types'

/**
 * This function is used to get formatted failed diagnostics from the diagnostics list
 */
export function getFailedDiagnostics(diagnostics: DependentDiagnostic[]) {
  return diagnostics
    .filter((diagnostic) => !diagnostic.check)
    .map((diagnostic, index) => ({
      id: diagnostic.id,
      name: diagnostic.ruleName || diagnostic.name,
      originalIndex: index,
    }))
}

/**
 * This function appends diagnostic content to the existing description
 */
export function getDescriptionText(
  existingDescription: string,
  insightDiagnostic: InsightDiagnosticResponse,
  t: TFunction,
  language: Language
) {
  const formattedDescription = existingDescription?.toLowerCase() ?? ''
  const isDiagnosticPresent =
    (formattedDescription.includes(t('plainText.diagnostics')) &&
      formattedDescription.includes(t('plainText.occurrence'))) ||
    insightDiagnostic?.diagnostics?.length === 0
  return isDiagnosticPresent
    ? existingDescription
    : existingDescription + getDiagnosticContent(insightDiagnostic, t, language)
}

/**
 * This function is used to get the list of diagnostics and format it in string values
 */
function getDiagnosticLists(
  diagnosticsDetails: DependentDiagnostic[],
  level = 0,
  t: TFunction
) {
  const indentation = ' '.repeat(level * 6)
  return diagnosticsDetails
    .map(({ ruleName, name, check, ...rest }) => {
      const diagnosticContent = getDiagnosticLists(
        rest.diagnostics || [],
        level + 1,
        t
      )
      const status = check ? t('plainText.pass') : t('plainText.fail')
      return `${indentation}• ${ruleName || name}: ${_.upperCase(status)}${
        diagnosticContent ? `\n${diagnosticContent}` : ''
      }`
    })
    .join('\n')
}

/**
 * This function is used to get diagnostic content along with diagnostic lists
 */
function getDiagnosticContent(
  { diagnostics, started, ended }: InsightDiagnosticResponse,
  t: TFunction,
  language: Language
) {
  const diagnosticList = getDiagnosticLists(diagnostics, 0, t)
  // eslint-disable-next-line max-len
  return `\n${_.startCase(t('plainText.diagnostics'))}: \n${_.startCase(
    t('plainText.occurrence')
  )}: ${formatDateTime({ value: started, language })} - ${
    formatDateTime({ value: ended, language }) ||
    _.startCase(t('plainText.stillOngoing'))
  } \n${diagnosticList}`
}

export function getSyncStatus(ticket: TicketSimpleDto) {
  const currentTime = new Date().getTime()
  const ticketCreatedTime = new Date(ticket.createdDate ?? 0).getTime()

  const timeDiff = currentTime - ticketCreatedTime

  const status: SyncStatus | undefined =
    !ticket.externalId && timeDiff > 60000
      ? SyncStatus.Failed
      : !ticket.externalId && timeDiff <= 60000
      ? SyncStatus.InProgress
      : ticket.externalId
      ? SyncStatus.Completed
      : undefined

  return status
}
