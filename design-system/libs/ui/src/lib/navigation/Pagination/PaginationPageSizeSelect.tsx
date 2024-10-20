import { Select } from '../../inputs/Select'
import { Group } from '../../layout/Group'
import styled from 'styled-components'

const options = [10, 20, 50, 100, 500, 1000] as const
export type PageSizeOption = (typeof options)[number]

const Label = styled.label(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.default,
  whiteSpace: 'nowrap',
}))

export default function PaginationPageSizeSelect({
  onChange,
  value,
}: {
  onChange: (value: PageSizeOption) => void
  value: PageSizeOption
}) {
  return (
    <Group wrap="nowrap">
      <Label htmlFor="pagination-page-size">Items per page</Label>
      <Select
        allowDeselect={false}
        data={options.map((option) => ({
          label: option.toLocaleString(),
          value: option.toString(),
        }))}
        id="pagination-page-size"
        miw={70}
        onChange={(value) => {
          if (value) onChange(parseInt(value, 10) as PageSizeOption)
        }}
        value={value.toString()}
        w={70}
      />
    </Group>
  )
}
