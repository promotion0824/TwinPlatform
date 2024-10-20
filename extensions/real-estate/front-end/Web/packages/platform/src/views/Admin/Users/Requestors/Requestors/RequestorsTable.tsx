import {
  DataGrid,
  GridColDef,
  GridRenderCellParams,
  GridTreeNodeWithRender,
} from '@willowinc/ui'
import { useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'

type Portfolio = {
  id: string
  name: string
}

type Requestor = {
  id: string
  name: string
  email: string
  type: string
  company: string
  contactNumber: string
  status: string
  sites: Array<{
    id: string
    name: string
    portfolio: Portfolio
  }>
  siteId: string
  portfolios: unknown[]
  contact: string
  created: string
}

export default function RequestorsTable({
  requestors,
  selectedRequestor,
  onRequestorClick,
}: {
  requestors: Requestor[]
  selectedRequestor: Requestor
  onRequestorClick: (requestor: Requestor) => void
}) {
  const { t } = useTranslation()

  const columns: GridColDef[] = useMemo(
    () => [
      {
        field: 'name',
        headerName: t('labels.name'),
        flex: 1,
      },
      {
        field: 'email',
        headerName: t('plainText.email'),
        flex: 1,
        renderCell: ({
          row: { email },
        }: GridRenderCellParams<
          Requestor,
          unknown,
          unknown,
          GridTreeNodeWithRender
        >) => (
          <a
            href={`mailto:${email}`}
            onClick={(e) => {
              e.stopPropagation()
            }}
          >
            {email}
          </a>
        ),
      },
      { field: 'contact', headerName: t('labels.contact'), flex: 1 },
      { field: 'company', headerName: t('labels.company'), flex: 1 },
    ],
    [t]
  )

  return (
    <StyledDataGrid
      rows={requestors}
      columns={columns}
      disableRowSelectionOnClick={false}
      rowSelectionModel={selectedRequestor?.id ? [selectedRequestor?.id] : []}
      onRowSelectionModelChange={([id]) => {
        const foundRequestor = requestors.find(
          (requestor) => requestor.id === id
        )
        if (foundRequestor) {
          onRequestorClick(foundRequestor)
        }
      }}
      disableMultipleRowSelection
      noRowsOverlayMessage={t('plainText.noRequestorsFound')}
    />
  )
}

const StyledDataGrid = styled(DataGrid)`
  /* mui style which is injected later has higher priority than
    the style added by styled-component. So has to increase specification
    here. Fix ticket added:
    https://dev.azure.com/willowdev/Unified/_workitems/edit/87764 */
  && {
    border: none;
  }
`
