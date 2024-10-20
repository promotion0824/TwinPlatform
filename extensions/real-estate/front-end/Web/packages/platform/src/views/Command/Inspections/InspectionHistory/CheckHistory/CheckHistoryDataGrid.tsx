import { ImgModal, MoreButtonDropdown, NotFound, useDateTime } from '@willow/ui'
import {
  Avatar,
  Badge,
  DataGrid,
  GridColDef,
  IconButton,
  Tooltip,
} from '@willowinc/ui'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import {
  Attachment,
  Check,
  CheckRecord,
  CheckRecordStatus,
} from '../../../../../services/Inspections/InspectionsServices'

export default function CheckHistoryDataGrid({
  check,
  checkRecords,
}: {
  check: Check
  checkRecords: CheckRecord[]
}) {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const dateTime = useDateTime()
  const [selectedImage, setSelectedImage] = useState<Attachment>()

  const rows = useMemo<Array<CheckRecord & { submittedDateDisplay: string }>>(
    () =>
      checkRecords.map((checkRecord) => ({
        ...checkRecord,
        submittedDateDisplay: dateTime(checkRecord.submittedDate).format(
          'dateTime',
          undefined,
          language
        ),
      })),
    [checkRecords, dateTime, language]
  )

  const columns = useMemo<GridColDef[]>(
    () => [
      {
        field: 'submittedDate',
        headerName: t('plainText.dateAndTime'),
        width: 150,
        valueGetter: ({ row }) => ({
          displayValue: row.submittedDateDisplay,
          sortValue: row.submittedDate,
        }),
        valueFormatter: (data) => data?.value?.displayValue || '',
        sortComparator: (v1, v2) =>
          new Date(v1.sortValue).getTime() - new Date(v2.sortValue).getTime(),
      },
      {
        field: 'enteredBy',
        headerName: t('plainText.enteredBy'),
        width: 150,
        renderCell: ({ row }: { row: CheckRecord }) => {
          const userName = row?.enteredBy ?? ''
          return (
            <Tooltip withinPortal label={userName}>
              <Avatar variant="subtle" color="gray">
                {userName
                  .split(' ')
                  .map((word) => word[0]?.toUpperCase() || '')
                  .join('')}
              </Avatar>
            </Tooltip>
          )
        },
      },
      {
        field: 'entry',
        headerName: t('labels.entry'),
        width: 90,
        valueGetter: ({ row }) =>
          getCheckRecordValue({ check, checkRecord: row, dateTime, language }),
      },
      {
        field: 'attachments',
        headerName: t('plainText.attachments'),
        minWidth: 180,
        renderCell: ({ row }) => (
          <>
            {row.attachments?.slice(0, 5).map((attachment: Attachment) => (
              <AttachmentButton
                key={attachment.id}
                attachment={attachment}
                onClick={() => setSelectedImage(attachment)}
              />
            ))}
            {row.attachments?.length > 5 && (
              <MoreButtonDropdown
                targetButtonProps={{
                  background: 'transparent',
                }}
              >
                {row.attachments.slice(5).map((attachment: Attachment) => (
                  <AttachmentButton
                    key={attachment.id}
                    attachment={attachment}
                    onClick={() => setSelectedImage(attachment)}
                  />
                ))}
              </MoreButtonDropdown>
            )}
          </>
        ),
      },
      {
        field: 'notes',
        headerName: t('labels.notes'),
        flex: 1,
      },
    ],
    [check, dateTime, language, t]
  )

  return (
    <>
      <DataGrid
        columns={columns}
        rows={rows}
        disableRowSelectionOnClick
        slots={{
          noRowsOverlay: () => (
            <NotFound>{t('plainText.noInspectionCheckFound')}</NotFound>
          ),
        }}
        // Remove extra padding from attachment column as
        // this column only contains Icons.
        css={`
          border: none;
          & [data-field='attachments'][role='cell'] {
            padding-left: 0 !important;
          }
        `}
      />
      {selectedImage != null && selectedImage.url && (
        <ImgModal
          src={selectedImage.url}
          name={selectedImage.fileName ?? ''}
          onClose={() => setSelectedImage(undefined)}
        />
      )}
    </>
  )
}

const AttachmentButton = ({
  attachment,
  onClick,
}: {
  attachment: Attachment
  onClick: () => void
}) => (
  <IconButton
    icon="attach_file"
    kind="secondary"
    background="transparent"
    key={attachment.id}
    onClick={onClick}
  />
)

// This util function follows legacy business logic and will likely be
// changed in the future.
const getCheckRecordValue = ({
  check,
  checkRecord,
  dateTime,
  language,
}: {
  check: Check
  checkRecord: CheckRecord
  dateTime: ReturnType<typeof useDateTime>
  language: string
}) => {
  if (checkRecord.status === CheckRecordStatus.Completed) {
    const checkType =
      check?.type?.toLowerCase() ?? checkRecord?.checkType?.toLowerCase()

    switch (checkType) {
      case 'date':
        return dateTime(checkRecord.dateValue).format(
          'date',
          undefined,
          language
        )
      case 'numeric':
      case 'total':
        return `${checkRecord.numberValue} ${
          checkRecord.typeValue || check.typeValue
        }`
      case 'list':
      default:
        return checkRecord.stringValue
    }
  }

  return (
    <Badge
      variant="outline"
      size="md"
      color={
        [CheckRecordStatus.Overdue, CheckRecordStatus.Missed].includes(
          checkRecord.status
        )
          ? 'red'
          : 'orange'
      }
    >
      {checkRecord.status}
    </Badge>
  )
}
