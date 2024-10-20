import _ from 'lodash'
import tw from 'twin.macro'
import { Icon, useModal, useSnackbar } from '@willow/ui'
import { useTheme } from 'styled-components'
import { useTranslation } from 'react-i18next'
import useUpdateInsightsStatuses from '../../../hooks/Insight/useUpdateInsightsStatuses'
import {
  Container,
  StyledIcon,
  ModalHeader,
  Warning,
  DeleteButton,
  CancelButton,
} from './shared'

/**
 * Modal content to be used for deleting single or multiple insights
 */
const DeleteInsightsConfirmation = ({
  siteId,
  selectedInsightIds = [],
  onClose,
  onClearInsightIds,
}: {
  siteId?: string
  selectedInsightIds?: string[]
  onClose?: () => void
  onClearInsightIds?: () => void
}) => {
  const { t } = useTranslation()
  const modal = useModal()
  const snackbar = useSnackbar()
  const theme = useTheme()

  const mutation = useUpdateInsightsStatuses({
    siteId: siteId ?? '',
    insightIds: selectedInsightIds,
    newStatus: 'deleted',
  })

  const handleConfirmationClose = () => {
    modal.close()
  }

  const handleConfirmationClick = () => {
    const snackbarOptions = {
      isToast: true,
      closeButtonLabel: t('plainText.dismiss'),
      color: theme.color.intent.negative.fg.default,
    }
    const insightsCountToBeDeleted = selectedInsightIds.length

    if (mutation.status === 'loading') {
      return // prevent user from clicking multiple times
    }
    mutation.mutate(undefined, {
      onError: () => {
        snackbar.show(t('plainText.errorOccurred'))
      },
      onSuccess: () => {
        onClearInsightIds?.()
        snackbar.show(
          _.capitalize(
            t('interpolation.insightsActioned', {
              count: insightsCountToBeDeleted,
              action: t('plainText.deleted'),
            })
          ),
          snackbarOptions
        )
        onClose?.()
      },
    })
  }

  return (
    <Container>
      <div tw="flex justify-between">
        <ModalHeader>{`${t('plainText.delete')} ${
          selectedInsightIds.length === 1
            ? t('headers.insight')
            : t('headers.insights')
        }`}</ModalHeader>
        <StyledIcon onClick={handleConfirmationClose} icon="close" />
      </div>
      <Warning>{t('plainText.deleteInsightWarning')}</Warning>
      <div tw="flex justify-end gap-[1rem]">
        <CancelButton onClick={handleConfirmationClose}>
          {t('plainText.cancel')}
        </CancelButton>
        <DeleteButton
          prefix={mutation.status === 'loading' && <Icon icon="progress" />}
          disabled={selectedInsightIds.length === 0}
          onClick={handleConfirmationClick}
        >
          {t('plainText.delete')}
        </DeleteButton>
      </div>
    </Container>
  )
}

export default DeleteInsightsConfirmation
