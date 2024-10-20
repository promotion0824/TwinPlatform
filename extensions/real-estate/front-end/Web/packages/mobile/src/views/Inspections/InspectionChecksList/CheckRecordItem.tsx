import { useEffect, useState } from 'react'
import { SubmitErrorHandler, SubmitHandler } from 'react-hook-form'
import { styled } from 'twin.macro'
import { CSSProp } from 'styled-components'
import { Button, Icon, Text } from '@willow/mobile-ui'
import CheckRecordForm, { FormValue } from './CheckRecordForm'
import {
  AttachmentEntry,
  Check,
  CheckRecord,
  CheckRow,
  ExtendedStatus,
} from './types'
import CheckHistory from './CheckHistory'

const defaultIcon: CSSProp = {
  color: 'var(--new-text)',
  background: 'var(--new-panel-bright)',
}

const checkStatusStyles: {
  [key in ExtendedStatus]: {
    label: string
    textStyle: CSSProp
    iconStyle: CSSProp
    iconName?: string
  }
} = {
  syncPending: {
    label: 'Sync Pending',
    textStyle: {
      color: '#959595',
    },
    iconStyle: {
      color: '#8779E2',
      background: 'rgba(128, 116, 217, 0.168627)',
    },
    iconName: 'sync',
  },
  syncError: {
    label: 'Sync Error',
    textStyle: {
      color: '#FC2D3B',
    },
    iconStyle: {
      color: '#FF3B48',
      background: 'rgba(252, 45, 59, 0.168627)',
    },
    iconName: 'cloudOff',
  },
  due: {
    label: 'Due',
    textStyle: {},
    iconStyle: defaultIcon,
  },
  missed: {
    label: 'Missed',
    textStyle: {},
    iconStyle: defaultIcon,
  },
  overdue: {
    label: 'Overdue',
    textStyle: {
      color: 'var(--red)',
    },
    iconStyle: defaultIcon,
  },
  completed: {
    label: 'Completed',
    textStyle: {
      color: 'var(--green)',
    },
    iconStyle: {
      color: 'var(--green)',
      background: 'var(--green-background)',
    },
  },
  notRequired: {
    label: 'Not Required',
    textStyle: {
      color: 'var(--green)',
    },
    iconStyle: defaultIcon,
  },
}

const CheckStatusText = styled(Text)<{ $status?: ExtendedStatus }>(
  ({ $status }) => ($status ? checkStatusStyles[$status].textStyle : {})
)

const TextContainer = styled.div({
  display: 'flex',
  flexDirection: 'row',
  alignItems: 'middle',
})

const CheckIcon = styled(Icon)<{ $status?: ExtendedStatus }>`
  border-radius: 50%;
  width: 26px;
  height: 26px;
  padding: 3px;
  ${({ $status }) => $status && checkStatusStyles[$status].iconStyle}
`

const StyledButton = styled(Button)({
  height: '58px',
  padding: '0 var(--padding-large)',
  width: '100%',
})

const ChevronIcon = styled(Icon)<{ $isExpanded?: boolean }>(
  ({ $isExpanded = false }) => ({
    transform: $isExpanded ? 'rotate(-180deg)' : undefined,
    height: '28px',
    width: '28px',
    transition: 'var(--transition-out)',
  })
)

const CheckRecordContainer = styled.div({
  borderBottom: '1px solid var(--theme-color-neutral-border-default)',
})

const CheckWrapper = styled.div({
  display: 'flex',
  flexDirection: 'row',
  width: '100%',
  alignItems: 'center',
})

const CheckNameAndStatus = styled.div({
  display: 'flex',
  flexDirection: 'column',
  padding: '0 var(--padding-large)',
  flex: '1',
})

const DependencyText = styled(Text)({
  paddingLeft: 'var(--padding)',
})

const DependencyIcon = styled(Icon)({
  paddingLeft: 'var(--padding)',
})

const ExpandedDetails = styled.div({
  borderTop: '1px solid var(--theme-color-neutral-border-default)',
  paddingTop: 'var(--padding-large)',
})

const ActionButtonWrapper = styled.div({
  padding: '0 var(--padding-large) var(--padding-large)',
  display: 'flex',
  justifyContent: 'space-between',
})

const HistoryButton = styled(Button)(({ selected }) => ({
  // override the active & hover background and text color.
  backgroundColor: selected ? '#8074d92b !important' : undefined,
  color: selected ? '#8779e2 !important' : undefined,
}))

/**
 * Check record item containing the summarised details of the record such as:
 * - Whether the check is completed
 * - The check name
 * - The check status
 *
 * This item contains a form that can be expanded for user to review or input, and
 * can be hidden by toggling this check record item.
 */
export default function CheckRecordItem({
  siteId,
  check,
  isExpanded,
  checkRecord,
  attachmentEntries,
  modified = false,
  syncStatus = null,
  dependentCheck,
  isAttachmentEnabled = false,
  readOnly = false,
  onToggle,
  onSubmit,
  onSubmitError,
}: {
  siteId: string
  check: Check
  isExpanded: boolean
  /**
   * Check record associated to the check. This may not be available when we add
   * a new check after the checkRecord is scheduled to be created.
   */
  checkRecord?: CheckRecord
  attachmentEntries: AttachmentEntry[]
  modified?: boolean
  syncStatus?: CheckRow['syncStatus']
  /**
   * If provided, contains the entry this check depends on to be editable
   * this check depends on to be editable.
   */
  dependentCheck?: Check
  isAttachmentEnabled?: boolean
  readOnly?: boolean
  onToggle: (checkId: string) => void
  onSubmit: SubmitHandler<FormValue>
  onSubmitError?: SubmitErrorHandler<FormValue>
}) {
  const [isHistoryView, setIsHistoryView] = useState(false)

  let status: ExtendedStatus | undefined = checkRecord?.status
  if (syncStatus === 'error') {
    status = 'syncError'
  } else if (modified) {
    status = 'syncPending'
  }

  useEffect(() => {
    if (!isExpanded) {
      // Reset to show current input form.
      setIsHistoryView(false)
    }
  }, [isExpanded])

  return (
    <CheckRecordContainer>
      <StyledButton
        onClick={() => {
          onToggle(check.id)
        }}
      >
        <CheckWrapper>
          <CheckIcon
            icon={
              (status != null ? checkStatusStyles[status].iconName : null) ??
              'check'
            }
            $status={status}
          />
          <CheckNameAndStatus>
            <Text>{check.name}</Text>
            <TextContainer>
              <CheckStatusText $status={status}>
                {status != null ? checkStatusStyles[status].label : null}
              </CheckStatusText>
              {dependentCheck && (
                <>
                  <DependencyText>●</DependencyText>
                  <DependencyIcon icon="link" size="medium" />
                  <Text tw="flex[1]" whiteSpace="nowrap">
                    Linked to {dependentCheck.name}
                  </Text>
                </>
              )}
            </TextContainer>
          </CheckNameAndStatus>
          <ChevronIcon icon="chevron" $isExpanded={isExpanded} />
        </CheckWrapper>
      </StyledButton>
      {isExpanded && (
        <ExpandedDetails>
          {!isHistoryView && (
            <CheckRecordForm
              check={check}
              checkRecord={checkRecord}
              attachmentEntries={attachmentEntries}
              isAttachmentEnabled={isAttachmentEnabled}
              onSubmit={(values, event) => {
                onSubmit(values, event)
              }}
              onSubmitError={onSubmitError}
              readOnly={readOnly}
            />
          )}
          <ActionButtonWrapper>
            {isHistoryView && (
              <Button
                color="grey"
                size="small"
                onClick={() => setIsHistoryView(false)}
              >
                Back
              </Button>
            )}
            <div />
            {!isHistoryView && (
              <HistoryButton
                color="grey"
                size="small"
                onClick={() => setIsHistoryView(true)}
                selected={isHistoryView}
              >
                View All History
              </HistoryButton>
            )}
          </ActionButtonWrapper>
          <Label>
            {isHistoryView ? 'Check History' : 'Latest Check History'}
          </Label>
          <CheckHistory
            status={status}
            siteId={siteId}
            check={check}
            attachmentEntries={attachmentEntries}
            count={isHistoryView ? 10 : 3}
          />
        </ExpandedDetails>
      )}
    </CheckRecordContainer>
  )
}

const Label = styled.div({
  display: 'flex',
  padding: `0px 8px`,
  paddingLeft: '0px',
  alignItems: 'flex-start',
  gap: '4px',
  alignSelf: 'stretch',
  marginLeft: '16px',
})
