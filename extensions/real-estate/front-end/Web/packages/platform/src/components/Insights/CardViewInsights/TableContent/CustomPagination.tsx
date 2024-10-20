import { styled } from 'twin.macro'
import { Icon, IconButton, IconName, Select } from '@willowinc/ui'
import { useInsightsContext } from '../InsightsContext'

/**
 * This component is used for rendering the custom pagination in UI for the insights table.
 * It displays the current page number, the total number of pages, and allows the user to navigate between pages.
 */
export default function CustomPagination() {
  const {
    t,
    page = 1,
    pageSize = 10,
    totalCount = 0,
    onQueryParamsChange,
  } = useInsightsContext()

  // Calculating total pages based on totalCount and pageSize
  const totalPages = Math.ceil(totalCount / pageSize)

  const buttons: Array<{
    icon: IconName
    disabled: boolean
    onClick: () => void
  }> = [
    {
      icon: 'keyboard_double_arrow_left',
      onClick: () => onQueryParamsChange?.({ page: '1' }),
      disabled: page === 1,
    },
    {
      icon: 'chevron_left',
      onClick: () => onQueryParamsChange?.({ page: (page - 1).toString() }),
      disabled: page === 1,
    },
    {
      icon: 'chevron_right',
      onClick: () => onQueryParamsChange?.({ page: (page + 1).toString() }),
      disabled: page === totalPages,
    },
    {
      icon: 'keyboard_double_arrow_right',
      onClick: () => onQueryParamsChange?.({ page: totalPages.toString() }),
      disabled: page === totalPages,
    },
  ]

  return (
    <>
      <div tw="flex mr-[16px] items-center">
        <StyledSelect
          defaultValue={pageSize.toString()}
          onChange={(selectedPageSize: string) =>
            onQueryParamsChange?.({
              page: undefined,
              pageSize: selectedPageSize,
            })
          }
          data={['10', '20', '30'].map((option) => ({
            label: t('interpolation.numberPerPage', { number: option }),
            value: option,
          }))}
        />
        {buttons.map(({ icon, onClick, disabled }) => (
          <StyledIconButton
            key={`${icon}-${disabled}`}
            kind="secondary"
            background="transparent"
            onClick={onClick}
            disabled={disabled}
          >
            <Icon icon={icon} color="highlight" />
          </StyledIconButton>
        ))}
        <StyledDiv>
          {t('interpolation.pageNumberofRecords', {
            number: (page - 1) * pageSize + 1,
            total: Math.min(page * pageSize, totalCount),
            totalRecords: totalCount,
          })}
        </StyledDiv>
      </div>
    </>
  )
}

const StyledSelect = styled(Select)(({ theme }) => ({
  width: '108px',
  margin: `0 ${theme.spacing.s8}`,
}))

const StyledIconButton = styled(IconButton)<{
  disabled: boolean
}>(({ disabled, theme }) => ({
  padding: `0 ${theme.spacing.s4}`,
  color: disabled ? theme.color.state.disabled.fg : '',
}))

const StyledDiv = styled.div(({ theme }) => ({
  color: theme.color.neutral.fg.default,
  ...theme.font.body.md.regular,
}))
