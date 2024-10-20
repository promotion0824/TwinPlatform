import type { Meta, StoryObj } from '@storybook/react'
import { noop } from 'lodash'
import { useState } from 'react'
import { css } from 'styled-components'
import { Pagination } from '.'
import { Card } from '../../data-display/Card'
import { DataGrid } from '../../data-display/DataGrid'
import { DataGridFooterContainer } from '../../data-display/DataGrid/components'
import { Stack } from '../../layout/Stack'
import { Box } from '../../misc/Box'

const meta: Meta<typeof Pagination> = {
  title: 'Pagination',
  component: Pagination,
}

export default meta

type Story = StoryObj<typeof Pagination>

const items = Array.from(
  { length: 2000 },
  (_, i) => `Item ${(i + 1).toLocaleString()}`
)

export const Playground: Story = {
  render: () => <Pagination itemCount={2000} onChange={noop} />,
}

export const Defaults: Story = {
  render: () => (
    <Pagination itemCount={2000} onChange={noop} pageNumber={2} pageSize={50} />
  ),
}

export const CardExample: Story = {
  render: () => {
    const [pagedItems, setPagedItems] = useState<string[]>([])

    return (
      <Stack>
        <Stack
          css={css(({ theme }) => ({
            border: `1px solid ${theme.color.neutral.border.default}`,
            borderRadius: theme.radius.r2,
            overflowY: 'scroll',
          }))}
          p="s8"
          h={400}
        >
          {pagedItems.map((item) => (
            <Card background="panel" key={item} p="s8">
              {item}
            </Card>
          ))}
        </Stack>
        <Pagination
          itemCount={2000}
          onChange={({ pageSize, startingIndex }) =>
            setPagedItems(items.slice(startingIndex, startingIndex + pageSize))
          }
        />
      </Stack>
    )
  },
}

export const DataGridExample: Story = {
  render: () => {
    const [pagedItems, setPagedItems] = useState<string[]>([])

    return (
      <Box h={400}>
        <DataGrid
          columns={[{ field: 'id', headerName: 'ID' }]}
          rows={pagedItems.map((item) => ({ id: item }))}
          slots={{
            footer: () => (
              <DataGridFooterContainer>
                <Pagination
                  itemCount={2000}
                  onChange={({ pageSize, startingIndex }) =>
                    setPagedItems(
                      items.slice(startingIndex, startingIndex + pageSize)
                    )
                  }
                />
              </DataGridFooterContainer>
            ),
          }}
        />
      </Box>
    )
  },
}
