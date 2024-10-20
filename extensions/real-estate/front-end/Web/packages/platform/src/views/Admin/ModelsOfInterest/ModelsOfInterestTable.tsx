import { useMemo } from 'react'
import {
  DataGrid,
  DataGridOverlay,
  GridColDef,
  GridValueGetterParams,
} from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { Panel, TwinChip, Button } from '@willow/ui'
import { styled } from 'twin.macro'
import {
  useModelsOfInterest,
  ModelOfInterest,
} from '@willow/common/twins/view/modelsOfInterest'
import { useManageModelsOfInterest } from './Provider/ManageModelsOfInterestProvider'

/**
 * ModelsOfInterestTable is a presentation component which displays a list of models of interest
 * where customer admins can decide whether to modify them.
 */
export default function ModelsOfInterestTable() {
  const modelsOfInterest = useModelsOfInterest({ includeExtras: false })

  return (
    <MarginTopPanel fill="content" $borderWidth="1px 0 0 0">
      {/**
       * TODO: error state, loading state
       */}
      {modelsOfInterest.data != null && (
        <TableContent modelsOfInterest={modelsOfInterest.data.items} />
      )}
    </MarginTopPanel>
  )
}

const MarginTopPanel = styled(Panel)({ marginTop: '4px' })

function TableContent({
  modelsOfInterest,
}: {
  modelsOfInterest: ModelOfInterest[]
}) {
  const { t } = useTranslation()
  const {
    setSelectedModelOfInterest,
    setExistingModelOfInterest,
    setFormMode,
    moveUp,
    moveDown,
    handleRowOrderChange,
    isReordering,
  } = useManageModelsOfInterest()

  const columns: GridColDef[] = useMemo(() => {
    const isButtonDisabled = (index: number, direction: 'up' | 'down') => {
      if (isReordering) return true
      if (direction === 'up') return index === 0
      return index === modelsOfInterest.length - 1
    }

    return [
      {
        field: 'name',
        headerName: t('labels.name'),
        flex: 2,
        renderCell: ({ row }: GridValueGetterParams) => (
          <NameText data-testid="name">{row.name}</NameText>
        ),
        sortable: false,
      },
      {
        field: 'representation',
        headerName: t('plainText.representation'),
        flex: 2,
        renderCell: ({ row }: GridValueGetterParams) => (
          <TwinChip text={row.name} modelOfInterest={row} />
        ),
        sortable: false,
      },
      {
        field: 'modelId',
        headerName: t('plainText.model'),
        flex: 10,
        renderCell: ({ row }: GridValueGetterParams) => (
          <ModelOfInterestText>{row.modelId}</ModelOfInterestText>
        ),
        sortable: false,
      },
      {
        field: 'edit',
        headerName: t('plainText.edit'),
        flex: 1,
        renderCell: ({ row }: GridValueGetterParams) => (
          <EditButton
            icon="create"
            data-testid="edit"
            onClick={() => {
              setSelectedModelOfInterest(row)
              setExistingModelOfInterest(row) // Preserve existing model's data. This is used for "Revert Changes" button.
              setFormMode('edit')
            }}
          />
        ),
        sortable: false,
      },
      {
        field: 'sortPosition',
        headerName: t('labels.sortPosition'),
        flex: 1,
        renderCell: ({ row }: GridValueGetterParams) => {
          const index = modelsOfInterest.findIndex((item) => item.id === row.id)

          const upDisabled = isButtonDisabled(index, 'up')
          const downDisabled = isButtonDisabled(index, 'down')

          return (
            <SortButtonContainer>
              <Button
                icon="up"
                iconSize="small"
                disabled={upDisabled}
                data-tooltip={
                  !upDisabled ? t('plainText.increaseSortOrder') : undefined
                }
                data-tooltip-position="bottom"
                data-testid="moveUp"
                onClick={() => moveUp(row.id)}
              />
              <Button
                icon="down"
                iconSize="small"
                disabled={downDisabled}
                data-tooltip={
                  !downDisabled ? t('plainText.decreaseSortOrder') : undefined
                }
                data-tooltip-position="bottom"
                data-testid="moveDown"
                onClick={() => moveDown(row.id)}
              />
            </SortButtonContainer>
          )
        },
        sortable: false,
      },
    ]
  }, [
    t,
    setSelectedModelOfInterest,
    setExistingModelOfInterest,
    setFormMode,
    modelsOfInterest,
    isReordering,
    moveUp,
    moveDown,
  ])

  return (
    <StyledDataGrid
      rows={modelsOfInterest}
      columns={columns}
      disableRowSelectionOnClick={false}
      disableMultipleRowSelection
      disableVirtualization
      rowReordering
      onRowOrderChange={handleRowOrderChange}
      loading={isReordering}
      slots={{
        noRowsOverlay: () => (
          <DataGridOverlay>
            {t('plainText.noModelsOfInterestFound')}
          </DataGridOverlay>
        ),
      }}
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

const SortButtonContainer = styled.div({ padding: '0 var(--padding)' })

const NameText = styled.span({
  font: 'normal 500 12px/18px Poppins',
  color: ' #D9D9D9',
})

const EditButton = styled(Button)({
  color: '#7E7E7E',
  '> svg': { width: '16px', height: '16px' },
  '&hover': { color: '#d9d9d9' },
})

const ModelOfInterestText = styled.span({
  lineHeight: '18px',
  color: '#959595',
})
