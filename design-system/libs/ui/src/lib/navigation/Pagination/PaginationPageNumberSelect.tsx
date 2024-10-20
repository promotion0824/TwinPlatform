import { Select } from '../../inputs/Select'
import { Group } from '../../layout/Group'
import styled from 'styled-components'

const Label = styled.label(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.default,
  whiteSpace: 'nowrap',
}))

const Suffix = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.default,
  whiteSpace: 'nowrap',
}))

export default function PaginationPageNumberSelect({
  itemCount,
  onChange,
  pageSize,
  value,
}: {
  itemCount: number
  onChange: (value: number) => void
  pageSize: number
  value: number
}) {
  const pageCount = Math.ceil(itemCount / pageSize)
  const options = Array.from({ length: pageCount }).map((_, i) => ({
    // Increment these by 1 so that the page numbers don't start at 0
    label: (i + 1).toLocaleString(),
    value: (i + 1).toString(),
  }))

  return (
    <Group className="pagination-page-number-select" wrap="nowrap">
      <Label htmlFor="pagination-page-number">Page</Label>
      <Select
        allowDeselect={false}
        data={options}
        id="pagination-page-number"
        miw={64}
        onChange={(value) => {
          if (value) onChange(parseInt(value, 10))
        }}
        value={value.toString()}
        w={64}
      />
      <Suffix>of {pageCount}</Suffix>
    </Group>
  )
}
