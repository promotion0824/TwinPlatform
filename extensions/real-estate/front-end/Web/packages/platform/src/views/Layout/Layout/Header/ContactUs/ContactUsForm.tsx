import _ from 'lodash'
import { styled } from 'twin.macro'
import { useTheme } from 'styled-components'
import { useRef } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { Text, caseInsensitiveEquals, useSnackbar, useUser } from '@willow/ui'
import { Button, Icon, Modal, Select, TextInput, Textarea } from '@willowinc/ui'
import Attachments from './Attachments'
import { CustomerFormDetails } from './types'
import useGetCategories from '../../../../../hooks/ContactUs/useGetCategories'
import useCreateTicket from '../../../../../hooks/ContactUs/useCreateTicket'
import { isValidEmail } from './validationRules'

const ATTACHMENT_LIMIT = 5

const ContactUsForm = ({
  isFormOpen,
  insightIds = [],
  siteId,
  onClose,
  onSubmitForm,
  onClearInsightIds,
}: {
  siteId?: string
  isFormOpen?: boolean
  insightIds?: string[]
  onClose?: (nextBoolean: boolean) => void
  onSubmitForm?: () => void
  onClearInsightIds?: () => void
}) => {
  const user = useUser()
  const { t } = useTranslation()
  const { data: categoriesData = [] } = useGetCategories()
  const isUserEmailValid =
    isValidEmail(user.email) &&
    !(user.email || '').toLowerCase().includes('support@willowinc.com')

  const formDetails: CustomerFormDetails = {
    requestorsName: isUserEmailValid
      ? `${user?.firstName || ''} ${user?.lastName || ''}`.trim()
      : '',
    requestorsEmail: isUserEmailValid ? user.email : '',
    comment: '',
    url: window.location.href, // Adding browser url as part of the request
    attachmentFiles: [],
    category: insightIds.length > 0 ? 'Insights' : '',
  }
  const createTicketMutation = useCreateTicket()
  const { register, control, handleSubmit, watch, reset } = useForm({
    defaultValues: formDetails,
  })
  const theme = useTheme()
  const snackbar = useSnackbar()
  const isInsightSelected = caseInsensitiveEquals(watch('category'), 'Insights')

  const onSubmit = (formData: CustomerFormDetails) => {
    if (
      formData.attachmentFiles &&
      formData.attachmentFiles?.length <= ATTACHMENT_LIMIT
    ) {
      // Passing insightsIds in request only if "Insight" category is selected
      createTicketMutation.mutate(
        {
          ...formData,
          siteId,
          insightIds: isInsightSelected ? insightIds : [],
        },
        {
          onError: () => {
            snackbar.show(t('plainText.errorOccurred'))
          },
          onSuccess: () => {
            // Display snackbar messages depending on whether an insight or insights have been reported
            // or a support request has been submitted
            if (isInsightSelected && insightIds?.length === 1) {
              snackbar.show(t('plainText.insightReported_one'), {
                isToast: true,
                closeButtonLabel: t('plainText.dismiss'),
              })
            } else if (isInsightSelected && insightIds?.length > 0) {
              snackbar.show(
                t('interpolation.insightReported_other', {
                  count: insightIds.length,
                }),
                {
                  isToast: true,
                  closeButtonLabel: t('plainText.dismiss'),
                }
              )
            } else {
              snackbar.show(t('plainText.supportRequestSubmitted'), {
                isToast: true,
                closeButtonLabel: t('plainText.dismiss'),
              })
            }
            reset()
            onClose?.(false)
            onSubmitForm?.()
          },
        }
      )
    }
  }

  const onCloseModal = () => {
    reset()
    onClose?.(false)
  }

  const formRef = useRef<HTMLFormElement | null>(null)

  return (
    <StyledModal
      opened={!!isFormOpen}
      onClose={onCloseModal}
      header={_.upperFirst(t('plainText.contactUs'))}
      size="sm"
    >
      <Form onSubmit={handleSubmit(onSubmit)} noValidate ref={formRef}>
        <StyledFieldSet>
          <Controller
            name="requestorsName"
            control={control}
            rules={{ required: t('messages.nameRequired') }}
            render={({ fieldState: { error }, field }) => (
              <TextInput
                label={t('labels.name')}
                required
                error={error?.message}
                {...field}
              />
            )}
          />
        </StyledFieldSet>
        <StyledFieldSet>
          <Controller
            name="requestorsEmail"
            control={control}
            rules={{ required: true }}
            render={({ fieldState: { error }, field: { value } }) => (
              <TextInput
                {...register('requestorsEmail', {
                  required: t('validationError.ERR_EMAIL_REQUIRED'),
                  validate: {
                    validEmail: (email) => {
                      // disallow Willow internal user from putting "support@willowinc.com" in the email field
                      // when submitting a contact us form on multi-tenant
                      // no need to translate, internal use only
                      // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/95416
                      if (
                        (email || '')
                          .toLowerCase()
                          .includes('support@willowinc.com')
                      ) {
                        return "Please use an email address other than 'support@willowinc.com'"
                      }

                      return (
                        isValidEmail(email) ||
                        t('plainText.validEmailAddressError')
                      )
                    },
                  },
                })}
                required
                label={t('labels.emailAddress')}
                error={error?.message}
                value={value}
              />
            )}
          />
        </StyledFieldSet>
        <StyledFieldSet>
          <Controller
            name="category"
            control={control}
            render={({ field }) => (
              <Select
                label={t('labels.category')}
                data={categoriesData}
                {...field}
              />
            )}
          />
        </StyledFieldSet>
        <StyledFieldSet>
          {isInsightSelected && (
            <>
              <Text tw="flex items-center">
                {_.upperFirst(t('plainText.addedInsights'))}
                <StyledIcon
                  icon="help"
                  data-tooltip={t('plainText.addedInsightsTooltipText')}
                  data-tooltip-position="top"
                  data-tooltip-z-index={theme.zIndex.popover}
                />
              </Text>
              <InsightCountContainer $disabled={insightIds?.length === 0}>
                <Text>
                  {t('interpolation.selectedInsights', {
                    count: insightIds?.length || 0,
                  })}
                </Text>
                <Button
                  kind="secondary"
                  size="large"
                  disabled={insightIds?.length === 0}
                  onClick={onClearInsightIds}
                >
                  {t('plainText.clear')}
                </Button>
              </InsightCountContainer>
            </>
          )}
        </StyledFieldSet>
        <StyledFieldSet>
          <Controller
            name="comment"
            control={control}
            rules={{ required: t('plainText.commentIsRequired') }}
            render={({ fieldState: { error }, field }) => (
              <Textarea
                label={t('plainText.howCanWeHelpYou')}
                required
                error={error?.message}
                {...field}
              />
            )}
          />
        </StyledFieldSet>
        <StyledFieldSet>
          <Controller
            name="attachmentFiles"
            control={control}
            rules={{ required: false }}
            render={({ field }) => (
              <Attachments
                limit={ATTACHMENT_LIMIT}
                value={field.value || []}
                onChange={field.onChange}
              />
            )}
          />
        </StyledFieldSet>
        <StyledFieldSet>
          <Button
            loading={createTicketMutation.isLoading}
            size="large"
            type="submit"
            tw="float-right"
          >
            {t('plainText.submit')}
          </Button>
        </StyledFieldSet>
      </Form>
    </StyledModal>
  )
}

export default ContactUsForm

const Form = styled.form(({ theme }) => ({
  padding: '0px 28px',

  'input:-webkit-autofill, input:-webkit-autofill:hover, input:-webkit-autofill:focus, input:-webkit-autofill:active':
    {
      filter: 'none',
      // by setting the transition to 50000000s, we are effectively prevent the autofill
      // background color change from happening
      transition: 'background-color 50000000s',
      '-webkit-text-fill-color': `${theme.color.neutral.fg.default} !important`,
    },
}))

const StyledModal = styled(Modal)(({ theme }) => ({
  '> div': {
    backgroundColor: 'transparent',

    '> section': {
      position: 'absolute',
      bottom: theme.spacing.s24,
      right: theme.spacing.s24,
      width: '380px',
      height: '583px',
      overflowY: 'auto',
    },
  },
}))

const StyledFieldSet = styled.div(({ theme }) => ({
  marginTop: theme.spacing.s16,

  '> div': {
    gap: theme.spacing.s4,
    width: '100%',
  },
}))

const StyledIcon = styled(Icon)(({ theme }) => ({
  marginLeft: theme.spacing.s8,
}))

const InsightCountContainer = styled.div<{
  $disabled?: boolean
}>(({ $disabled = false, theme }) => ({
  margin: `${theme.spacing.s12} 0px 0px ${theme.spacing.s12}`,
  justifyContent: 'space-between',
  display: 'inline-flex',
  color: $disabled
    ? theme.color.state.disabled.fg
    : theme.color.neutral.fg.default,
  alignItems: 'center',

  '&&&': {
    width: '75%',
  },
}))
