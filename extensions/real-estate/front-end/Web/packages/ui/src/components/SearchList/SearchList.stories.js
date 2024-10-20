import SearchList from './SearchList'
import 'twin.macro'
import sampleItems from './sampleList.json'

const ListItem = ({ item: { text, color, checked }, index }) => (
  <div
    key={index}
    tw="flex border-solid border-0 border-b-1 p-2 align-items[center]"
  >
    <div tw="width[24px] text-center">{checked ? '🗸' : null}</div>
    <div tw="flex-1">{text}</div>
    <div tw="font-size[smaller] p-1" style={{ background: color }}>
      {color}
    </div>
  </div>
)

const meta = {
  component: SearchList,
  render: () => (
    <SearchList
      items={sampleItems}
      renderItem={(item, index) => <ListItem item={item} index={index} />}
    />
  ),
  decorators: [
    (Story) => (
      <div tw="height[300px] width[300px] border-solid border-1">
        <Story />
      </div>
    ),
  ],
}

export default meta

export const SearchByText = {
  args: {
    searchKeys: ['text'],
  },
}

export const SearchByTextAndColor = {
  args: {
    searchKeys: ['text', 'color'],
  },
}

export const SearchByTextAndColorAndChecked = {
  args: {
    searchKeys: ['text', 'color', 'checked'],
  },
}

export const SearchByTextWithStickyInput = {
  decorators: [
    (Story) => (
      <div tw="max-height[500px] overflow-auto border-solid border-1">
        <Story />
      </div>
    ),
  ],
  args: {
    searchKeys: ['text', 'color'],
  },
}
