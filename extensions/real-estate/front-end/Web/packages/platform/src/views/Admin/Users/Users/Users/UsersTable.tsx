import { Row as User, Time } from '@willow/ui'
import {
  DataGrid,
  GridColDef,
  GridRenderCellParams,
  GridTreeNodeWithRender,
} from '@willowinc/ui'
import { useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'

type User = {
  id: string
  name: string
  firstName: string
  lastName: string
  email: string
  type: 'customerUser'
  createdDate: string
  company: string
  contactNumber: string
  status: string
  sites: Array<{
    siteId: string
    site: string
    portfolioId: string
    portfolio: string
  }>
  portfolios: []
  contact: string
  created: string
}

export default function UsersTable({
  users,
  selectedUser,
  onUserClick,
}: {
  users: User[]
  selectedUser: User
  onUserClick: (user: User) => void
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
          User,
          unknown,
          unknown,
          GridTreeNodeWithRender
        >) => (
          <a
            href={`mailTo:${email}`}
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
      {
        field: 'created',
        headerName: t('labels.userSince'),
        renderCell: ({
          row: { created },
        }: GridRenderCellParams<
          User,
          unknown,
          unknown,
          GridTreeNodeWithRender
        >) => <Time value={created} format="date" />,
      },
    ],
    [t]
  )

  return (
    <StyledDataGrid
      rows={users}
      columns={columns}
      disableRowSelectionOnClick={false}
      // rowSelectionModel will accept an array of row id or [],
      // need this [] to update row deselection
      rowSelectionModel={selectedUser?.id ? [selectedUser?.id] : []}
      onRowSelectionModelChange={([id]) =>
        onUserClick(users.find((user) => user.id === id)!)
      }
      disableMultipleRowSelection
      noRowsOverlayMessage={t('plainText.noUsersFound')}
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
