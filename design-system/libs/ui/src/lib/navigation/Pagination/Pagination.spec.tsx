import { noop } from 'lodash'
import { Pagination } from '.'
import { act, render, screen } from '../../../jest/testUtils'

const nextPageButton = () => screen.getByLabelText('Next page')
const previousPageButton = () => screen.getByLabelText('Previous page')

const changePage = (page: number) => {
  act(() => screen.getByLabelText('Page').click())
  act(() => screen.getAllByText(page.toString()).at(-1)?.click())
}

const changePageSize = (pageSize: number) => {
  act(() => screen.getByLabelText('Items per page').click())
  act(() => screen.getAllByText(pageSize.toString())[0].click())
}

describe('Pagination', () => {
  it('should return the filtered items to the onChange callback', () => {
    const onChange = jest.fn()
    render(<Pagination itemCount={2000} onChange={onChange} />)
    expect(onChange).lastCalledWith({
      currentPageSize: 10,
      pageNumber: 1,
      pageSize: 10,
      startingIndex: 0,
    })
  })

  it('should disable the "previous" button when on the first page', () => {
    render(<Pagination itemCount={2000} onChange={noop} />)
    expect(previousPageButton()).toBeDisabled()
  })

  it('should disable the "next" button when on the last page', () => {
    render(<Pagination itemCount={2000} onChange={noop} />)
    expect(nextPageButton()).toBeEnabled()

    changePage(200)
    expect(nextPageButton()).toBeDisabled()
  })

  it('should calculate the correct items when changing pages', () => {
    const onChange = jest.fn()
    render(<Pagination itemCount={2000} onChange={onChange} />)
    expect(screen.getByText('1-10 of 2,000 items')).toBeInTheDocument()

    changePage(3)
    expect(screen.getByText('21-30 of 2,000 items')).toBeInTheDocument()
    expect(onChange).lastCalledWith({
      currentPageSize: 10,
      pageNumber: 3,
      pageSize: 10,
      startingIndex: 20,
    })
  })

  it('should calculate the correct items when changing page size', () => {
    const onChange = jest.fn()
    render(<Pagination itemCount={2000} onChange={onChange} />)
    expect(screen.getByText('1-10 of 2,000 items')).toBeInTheDocument()

    changePageSize(50)
    expect(screen.getByText('1-50 of 2,000 items')).toBeInTheDocument()
    expect(onChange).lastCalledWith({
      currentPageSize: 50,
      pageNumber: 1,
      pageSize: 50,
      startingIndex: 0,
    })

    changePage(3)
    expect(screen.getByText('101-150 of 2,000 items')).toBeInTheDocument()
    expect(onChange).lastCalledWith({
      currentPageSize: 50,
      pageNumber: 3,
      pageSize: 50,
      startingIndex: 100,
    })
  })

  it('should reset to the first page when changing page size', () => {
    const onChange = jest.fn()
    render(<Pagination itemCount={2000} onChange={onChange} />)
    expect(screen.getByText('1-10 of 2,000 items')).toBeInTheDocument()

    changePage(10)
    expect(screen.getByText('91-100 of 2,000 items')).toBeInTheDocument()
    expect(onChange).lastCalledWith({
      currentPageSize: 10,
      pageNumber: 10,
      pageSize: 10,
      startingIndex: 90,
    })

    changePageSize(50)
    expect(screen.getByText('1-50 of 2,000 items')).toBeInTheDocument()
    expect(onChange).lastCalledWith({
      currentPageSize: 50,
      pageNumber: 1,
      pageSize: 50,
      startingIndex: 0,
    })
  })

  it('should select the specific page when a pageNumber is provided', () => {
    const onChange = jest.fn()

    render(<Pagination itemCount={2000} onChange={onChange} pageNumber={3} />)
    expect(screen.getByText('21-30 of 2,000 items')).toBeInTheDocument()
    expect(onChange).lastCalledWith({
      currentPageSize: 10,
      pageNumber: 3,
      pageSize: 10,
      startingIndex: 20,
    })
  })

  it('should select the specific page when a pageSize is provided', () => {
    const onChange = jest.fn()

    render(<Pagination itemCount={2000} onChange={onChange} pageSize={50} />)
    expect(screen.getByText('1-50 of 2,000 items')).toBeInTheDocument()
    expect(onChange).lastCalledWith({
      currentPageSize: 50,
      pageNumber: 1,
      pageSize: 50,
      startingIndex: 0,
    })
  })

  it('should select the specific page when both pageNumber and pageSize are provided', () => {
    const onChange = jest.fn()

    render(
      <Pagination
        itemCount={2000}
        onChange={onChange}
        pageNumber={3}
        pageSize={50}
      />
    )

    expect(screen.getByText('101-150 of 2,000 items')).toBeInTheDocument()
    expect(onChange).lastCalledWith({
      currentPageSize: 50,
      pageNumber: 3,
      pageSize: 50,
      startingIndex: 100,
    })
  })

  it('should select page 1 when an out of range pageNumber is provided', () => {
    const onChange = jest.fn()

    render(
      <Pagination itemCount={2000} onChange={onChange} pageNumber={1000} />
    )

    expect(screen.getByText('1-10 of 2,000 items')).toBeInTheDocument()
    expect(onChange).lastCalledWith({
      currentPageSize: 10,
      pageNumber: 1,
      pageSize: 10,
      startingIndex: 0,
    })
  })

  it('should select page 1 when out of range pageSize and pageNumber props are provided', () => {
    const onChange = jest.fn()

    render(
      <Pagination
        itemCount={2000}
        onChange={onChange}
        pageSize={50}
        pageNumber={100}
      />
    )

    expect(screen.getByText('1-50 of 2,000 items')).toBeInTheDocument()
    expect(onChange).lastCalledWith({
      currentPageSize: 50,
      pageNumber: 1,
      pageSize: 50,
      startingIndex: 0,
    })
  })

  it('should return the correct currentPageSize when on the final page', () => {
    const onChange = jest.fn()
    render(<Pagination itemCount={197} onChange={onChange} pageSize={50} />)

    changePage(4)
    expect(onChange).lastCalledWith({
      currentPageSize: 47,
      pageNumber: 4,
      pageSize: 50,
      startingIndex: 150,
    })
  })
})
