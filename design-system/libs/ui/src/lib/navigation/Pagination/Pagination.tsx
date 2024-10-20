import { forwardRef, useEffect, useState } from 'react'
import styled from 'styled-components'
import { IconButton } from '../../buttons/Button'
import { Group } from '../../layout/Group'
import { WillowStyleProps } from '../../utils'
import PaginationPageNumberSelect from './PaginationPageNumberSelect'
import PaginationPageSizeSelect, {
  PageSizeOption,
} from './PaginationPageSizeSelect'

export interface PaginationProps extends WillowStyleProps {
  /** Number of items to be paged through. */
  itemCount: number
  /** Called when the page of items is updated. */
  onChange: ({
    currentPageSize,
    pageNumber,
    pageSize,
    startingIndex,
  }: {
    currentPageSize: number
    pageNumber: number
    pageSize: number
    startingIndex: number
  }) => void
  /**
   * The initial page number to be displayed.
   * @default 1
   */
  pageNumber?: number
  /**
   * The initial number of items per page.
   * @default 10
   */
  pageSize?: PageSizeOption
}

const Container = styled(Group)(({ theme }) => ({
  containerType: 'inline-size',
  overflowX: 'auto',
  width: '100%',

  [`@container (max-width: ${theme.breakpoints.mobile})`]: {
    '.pagination-item-count': {
      display: 'none',
    },
  },

  [`@container (max-width: 450px)`]: {
    '.pagination-page-number-select': {
      display: 'none',
    },
  },

  [`@container (max-width: 280px)`]: {
    label: {
      display: 'none',
    },
  },
}))

const ItemCount = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.muted,
  whiteSpace: 'nowrap',
}))

/** `Pagination` allows for navigation between multiple pages. */
export const Pagination = forwardRef<HTMLDivElement, PaginationProps>(
  (
    { itemCount, onChange, pageNumber = 1, pageSize = 10, ...restProps },
    ref
  ) => {
    const maxPageNumber = Math.ceil(itemCount / pageSize)

    const [currentPage, setCurrentPage] = useState(
      pageNumber <= maxPageNumber ? pageNumber : 1
    )

    const [currentPageSize, setCurrentPageSize] = useState(pageSize)

    const firstItem = (currentPage - 1) * currentPageSize + 1
    const lastItem = Math.min(currentPage * currentPageSize, itemCount)

    useEffect(
      () =>
        onChange({
          currentPageSize: lastItem - firstItem + 1,
          pageNumber: currentPage,
          pageSize: currentPageSize,
          startingIndex: firstItem - 1,
        }),
      [currentPage, currentPageSize, firstItem, lastItem, onChange]
    )

    return (
      <Container {...restProps} ref={ref} wrap="nowrap">
        <Group gap="s16" wrap="nowrap">
          <PaginationPageSizeSelect
            onChange={(newPageSize) => {
              setCurrentPage(1)
              setCurrentPageSize(newPageSize)
            }}
            value={currentPageSize}
          />
          <ItemCount className="pagination-item-count">
            {`${firstItem.toLocaleString()}-${lastItem.toLocaleString()} of ${itemCount.toLocaleString()} items`}
          </ItemCount>
        </Group>

        <Group gap="s16" ml="auto" wrap="nowrap">
          <PaginationPageNumberSelect
            itemCount={itemCount}
            onChange={setCurrentPage}
            pageSize={currentPageSize}
            value={currentPage}
          />

          <Group wrap="nowrap">
            <IconButton
              aria-label="Previous page"
              disabled={currentPage === 1}
              icon="chevron_left"
              kind="secondary"
              onClick={() => setCurrentPage((prev) => prev - 1)}
            />
            <IconButton
              aria-label="Next page"
              disabled={lastItem === itemCount}
              icon="chevron_right"
              kind="secondary"
              onClick={() => setCurrentPage((prev) => prev + 1)}
            />
          </Group>
        </Group>
      </Container>
    )
  }
)
