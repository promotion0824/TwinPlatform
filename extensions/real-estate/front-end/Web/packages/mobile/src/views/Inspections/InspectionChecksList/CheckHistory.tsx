import tw, { styled } from 'twin.macro'
import {
  Button,
  Loader,
  useApi,
  Icon,
  Text,
  useDateTime,
  stringUtils,
} from '@willow/mobile-ui'
import { useState } from 'react'
import { useQuery } from 'react-query'
import { AttachmentEntry, Check, CheckRecord, ExtendedStatus } from './types'
import Avatar from '../../../components/Avatar/Avatar'
import CheckRecordForm, { getFieldName } from './CheckRecordForm'

const CheckRecordToggle = styled(Button)({
  display: 'flex',
  width: '100%',
  padding: 'var(--padding-large) var(--padding-large)',
})

const Summary = styled.div({
  flex: '1',
  margin: '0 var(--padding-large)',
  color: '#959595',
})

const EntryText = styled.span({
  color: '#D9D9D9',
})

const ChevronIcon = styled(Icon)<{ $isExpanded: boolean }>(
  ({ $isExpanded }) => ({
    transform: $isExpanded ? 'rotate(-180deg)' : undefined,
    height: '28px',
    width: '28px',
    transition: 'var(--transition-out)',
  })
)

const ListItem = styled.li({
  margin: 'var(--padding-large)',
  padding: '0',
  background: '#252525',
})

const StyledAvatar = styled(Avatar)({
  height: 32,
  width: 32,
  lineHeight: '32px',
})

const Message = styled.div({
  padding: 'var(--padding-large)',
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'center',
  justifyContent: 'space-between',
  height: '90px',
  background: '#252525',
  margin: 'var(--padding-large)',
  fontSize: '12px',
  lineHeight: '20px',
  color: '#959595',
})

const useGetCheckHistory = (
  siteId: string,
  inspectionId: string,
  checkId: string,
  count: number,
  status?: ExtendedStatus
) => {
  const api = useApi()
  // Re-Fetch history, when status changes from overdue to => 'completed', 'due' etc.
  return useQuery(
    ['inspectionCheckHistory', inspectionId, checkId, count, status],
    () =>
      api.get(
        `/api/sites/${siteId}/inspections/${inspectionId}/checks/${checkId}/submittedhistory`,
        { count }
      )
  )
}

/**
 * A historical check record item containing the entered entry and timestamp.
 * This can be expanded to view the check record form in readOnly mode.
 */
const PastCheckRecord = ({
  check,
  checkRecord,
  attachmentEntries,
  onToggle,
  isExpanded,
}: {
  check: Check
  checkRecord: CheckRecord
  attachmentEntries: AttachmentEntry[]
  onToggle: () => void
  isExpanded: boolean
}) => {
  const { enteredBy } = checkRecord
  const dateTime = useDateTime()
  const formatEntry = (type, entry, decimalPlaces) => {
    if (entry == null) {
      return '-'
    } else if (type === 'date') {
      return dateTime(entry).format('date')
    } else if (type === 'list') {
      return stringUtils.capitalizeFirstLetter(entry)
    } else {
      return Number.parseFloat(entry).toFixed(decimalPlaces)
    }
  }

  return (
    <ListItem>
      <CheckRecordToggle onClick={onToggle}>
        {enteredBy != null && (
          <StyledAvatar
            firstName={enteredBy.firstName}
            lastName={enteredBy.lastName}
          />
        )}
        <Summary>
          <div data-testid="entry">
            Entry:{' '}
            <EntryText>
              {formatEntry(
                check.type,
                checkRecord[getFieldName(check.type)],
                'decimalPlaces' in check ? check.decimalPlaces : undefined
              )}
            </EntryText>
          </div>
          <div data-testid="timestamp">
            Timestamp:{' '}
            {dateTime(checkRecord.submittedDate).format('dateTimeLong')}
          </div>
        </Summary>
        <ChevronIcon icon="chevron" $isExpanded={isExpanded} />
      </CheckRecordToggle>

      {isExpanded && (
        <CheckRecordForm
          check={check}
          checkRecord={checkRecord}
          attachmentEntries={attachmentEntries}
          isAttachmentEnabled={false}
          readOnly
          isHistorical
        />
      )}
    </ListItem>
  )
}

/**
 * List of Check record history as retrieved via {@link useGetCheckHistory}
 */
export default function CheckHistory({
  siteId,
  check,
  attachmentEntries,
  count = 10,
  status,
}: {
  siteId: string
  check: Check
  attachmentEntries: AttachmentEntry[]
  count?: number
  status?: ExtendedStatus
}) {
  const [activeId, setActiveId] = useState<string | undefined>(undefined)
  const { data, isLoading, isError, isSuccess } = useGetCheckHistory(
    siteId,
    check.inspectionId,
    check.id,
    count,
    status
  )

  if (isLoading) {
    return <Loader />
  } else if (isSuccess && !data?.length) {
    return (
      <Message>
        <Icon tw="color[#383838]" size="large" icon="notFound" />
        <Text>No inspection check history found</Text>
      </Message>
    )
  } else if (isError) {
    return (
      <Message>
        <Icon tw="color[#383838]" size="large" icon="building" />
        <Text>Check history is not available in offline mode.</Text>
      </Message>
    )
  }

  return (
    <ul tw="list-style[none] padding[0]">
      {data?.map((checkRecord) => (
        <PastCheckRecord
          key={checkRecord.id}
          check={check}
          checkRecord={checkRecord}
          attachmentEntries={attachmentEntries}
          onToggle={() =>
            setActiveId(
              activeId !== checkRecord.id ? checkRecord.id : undefined
            )
          }
          isExpanded={activeId === checkRecord.id}
        />
      ))}
    </ul>
  )
}
