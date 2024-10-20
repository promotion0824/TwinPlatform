import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { SearchList } from '@willow/ui'
import Wrapper from '../../utils/testUtils/Wrapper'

const sampleList = require('./sampleList.json')

describe('SearchList', () => {
  const renderItem = (item, index) => (
    <div key={index} className="mySearchItem">
      {item.text}
    </div>
  )

  test('Throws error when either searchKeys or filterFn is not defined', () => {
    // Unfortunately there is no other way to suppress the logging of an
    // exception thrown in a React component.
    jest.spyOn(console, 'error').mockImplementation(() => jest.fn())

    try {
      expect(() =>
        render(<SearchList items={sampleList} renderItem={() => {}} />, {
          wrapper: Wrapper,
        })
      ).toThrowError()
    } finally {
      jest.restoreAllMocks()
    }
  })

  test('Render the list correctly', () => {
    render(
      <SearchList
        items={sampleList}
        searchKeys={['text']}
        renderItem={renderItem}
      />,
      {
        wrapper: Wrapper,
      }
    )

    expect(document.getElementsByClassName('mySearchItem').length).toBe(
      sampleList.length
    )
  })

  test('Filters the list based on search text and custom filter function', async () => {
    render(
      <SearchList
        items={sampleList}
        searchKeys={['text']}
        renderItem={renderItem}
        filterFn={(item) => item.checked && item.color === 'black'}
      />,
      {
        wrapper: Wrapper,
      }
    )
    const searchInput = screen.getByRole('textbox')

    userEvent.type(searchInput, 'mAtTiS')
    userEvent.tab()

    expect(document.getElementsByClassName('mySearchItem').length).toBe(4)
    expect(screen.getAllByText(/mattis/i).length).toBe(3)
    expect(
      screen.getAllByText('Duis lacinia lorem vel tempus pellentesque.').length
    ).toBe(1)

    userEvent.clear(searchInput)
    userEvent.tab()

    expect(document.getElementsByClassName('mySearchItem').length).toBe(
      sampleList.length
    )
  })

  test('Filters the list based on item value case-insensitively contains search text', async () => {
    render(
      <SearchList
        items={sampleList}
        searchKeys={['text']}
        renderItem={renderItem}
      />,
      {
        wrapper: Wrapper,
      }
    )
    const searchInput = screen.getByRole('textbox')

    userEvent.type(searchInput, 'mAtTiS')
    userEvent.tab()

    expect(document.getElementsByClassName('mySearchItem').length).toBe(3)
    expect(screen.getAllByText(/mattis/i).length).toBe(3)

    userEvent.clear(searchInput)
    userEvent.tab()

    expect(document.getElementsByClassName('mySearchItem').length).toBe(
      sampleList.length
    )
  })

  test('Filters the list based on item value containing boolean value', async () => {
    render(
      <SearchList
        items={sampleList}
        searchKeys={['text', 'checked']}
        renderItem={renderItem}
      />,
      {
        wrapper: Wrapper,
      }
    )
    const searchInput = screen.getByRole('textbox')

    userEvent.type(searchInput, 'blah')
    userEvent.tab()

    expect(document.getElementsByClassName('mySearchItem').length).toBe(21)

    userEvent.clear(searchInput)
    userEvent.tab()

    expect(document.getElementsByClassName('mySearchItem').length).toBe(
      sampleList.length
    )
  })

  test('Not found any match', async () => {
    render(
      <SearchList
        items={sampleList}
        searchKeys={['text']}
        renderItem={renderItem}
      />,
      {
        wrapper: Wrapper,
      }
    )
    const searchInput = screen.getByRole('textbox')

    userEvent.type(searchInput, 'blah')
    userEvent.tab()

    expect(document.getElementsByClassName('mySearchItem').length).toBe(0)

    userEvent.clear(searchInput)
    userEvent.tab()

    expect(document.getElementsByClassName('mySearchItem').length).toBe(
      sampleList.length
    )
  })
})
