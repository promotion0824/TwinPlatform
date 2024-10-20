import type { Meta, StoryObj } from '@storybook/react'

import { useState } from 'react'
import { css } from 'styled-components'
import { Combobox, useCombobox } from '.'
import { Avatar } from '../../data-display/Avatar'
import { Group } from '../../layout/Group'
import { Box } from '../../misc/Box'
import { Checkbox } from '../Checkbox'
import { flattenTree, SingleSelectTree } from '../SingleSelectTree'

const meta: Meta<typeof Combobox> = {
  title: 'Combobox',
  component: Combobox,
}
export default meta

type Story = StoryObj<typeof Combobox>

export const Playground: Story = {
  render: () => {
    const groceries = ['Apples', 'Bananas', 'Broccoli', 'Carrots', 'Chocolate']

    const combobox = useCombobox({
      onDropdownClose: () => combobox.resetSelectedOption(),
    })
    const [selectedValue, setSelectedValue] = useState<string | undefined>()

    return (
      <Combobox
        store={combobox}
        onOptionSubmit={(val) => {
          setSelectedValue(val)
          combobox.closeDropdown()
        }}
      >
        <Combobox.Target>
          <Combobox.InputBase
            component="button"
            type="button"
            pointer
            suffix={<Combobox.Chevron />}
            suffixPointerEvents="none"
            onClick={() => combobox.toggleDropdown()}
          >
            {selectedValue || (
              <Combobox.InputPlaceholder>Pick value</Combobox.InputPlaceholder>
            )}
          </Combobox.InputBase>
        </Combobox.Target>

        <Combobox.Dropdown>
          <Combobox.Options>
            {groceries.map((item) => (
              <Combobox.Option value={item} key={item}>
                {item}
              </Combobox.Option>
            ))}
          </Combobox.Options>
        </Combobox.Dropdown>
      </Combobox>
    )
  },
}

export const CustomizedWithSingleSelectTree: Story = {
  render: () => {
    const combobox = useCombobox({
      onDropdownClose: () => combobox.resetSelectedOption(),
    })
    const [selectedValue, setSelectedValue] = useState<string | undefined>(
      'asset'
    )

    const allNodes = flattenTree([allItemsNode, ...treeData])

    return (
      <Combobox store={combobox}>
        <Combobox.Target>
          <Combobox.InputBase
            component="button"
            type="button"
            pointer
            suffix={<Combobox.Chevron />}
            suffixPointerEvents="none"
            onClick={() => combobox.toggleDropdown()}
          >
            {allNodes.filter(({ id }) => id === selectedValue)[0].name || (
              <Combobox.InputPlaceholder>
                Select a category
              </Combobox.InputPlaceholder>
            )}
          </Combobox.InputBase>
        </Combobox.Target>

        <Combobox.Dropdown
          mah={190}
          css={{
            overflowY: 'auto',
          }}
        >
          <Combobox.Options>
            <SingleSelectTree
              allItemsNode={allItemsNode}
              data={treeData}
              onChange={(value) => {
                setSelectedValue(value[0].id)
                combobox.closeDropdown()
              }}
              selection={selectedValue ? [selectedValue] : undefined}
            />
          </Combobox.Options>
        </Combobox.Dropdown>
      </Combobox>
    )
  },
}

export const CustomizedSelectOption: Story = {
  render: () => {
    const options = [
      { color: 'red', label: 'Bug', value: 'bug' },
      { color: 'purple', label: 'Feature', value: 'feature' },
      { color: 'blue', label: 'Improvement', value: 'improvement' },
    ] as const
    const combobox = useCombobox({
      onDropdownClose: () => combobox.resetSelectedOption(),
    })
    const [selectedValue, setSelectedValue] = useState<string | undefined>(
      'feature'
    )

    const StyledOption = ({
      label,
      color,
    }: {
      label: string
      color: (typeof options)[number]['color']
    }) => (
      <Group>
        <Box
          w="s12"
          h="s12"
          css={css(({ theme }) => ({
            borderRadius: '50%',
            backgroundColor: theme.color.core[color].border.default,
          }))}
        />
        {label}
      </Group>
    )

    const selectedOption = options.find(
      (option) => option.value === selectedValue
    )

    return (
      <Combobox
        store={combobox}
        onOptionSubmit={(val) => {
          setSelectedValue(val)
          combobox.closeDropdown()
        }}
      >
        <Combobox.Target>
          <Combobox.InputBase
            component="button"
            type="button"
            pointer
            suffix={<Combobox.Chevron />}
            suffixPointerEvents="none"
            onClick={() => combobox.toggleDropdown()}
          >
            {selectedOption ? (
              <StyledOption
                label={selectedOption.label}
                color={selectedOption.color}
              />
            ) : (
              <Combobox.InputPlaceholder>
                Select a label
              </Combobox.InputPlaceholder>
            )}
          </Combobox.InputBase>
        </Combobox.Target>

        <Combobox.Dropdown>
          <Combobox.Options>
            {options.map(({ value, label, color }) => (
              <Combobox.Option
                value={value}
                key={label}
                selected={value === selectedValue}
              >
                <StyledOption label={label} color={color} />
              </Combobox.Option>
            ))}
          </Combobox.Options>
        </Combobox.Dropdown>
      </Combobox>
    )
  },
}

export const CustomizedMultiSelectWithCheckboxes: Story = {
  render: () => {
    const people = [
      {
        firstName: 'Floyd',
        lastName: 'Miles',
        color: 'red',
        value: 'Floyd Miles',
      },
      {
        firstName: 'Savannah',
        lastName: 'Nguyen',
        color: 'orange',
        value: 'Savannah Nguyen',
      },
      {
        firstName: 'Esther',
        lastName: 'Howard',
        color: 'yellow',
        value: 'Esther Howard',
      },
      {
        firstName: 'Jacob',
        lastName: 'Jones',
        color: 'teal',
        value: 'Jacob Jones',
      },
      {
        firstName: 'Ralph',
        lastName: 'Edwards',
        color: 'green',
        value: 'Ralph Edwards',
      },
    ] as const

    const combobox = useCombobox({
      onDropdownClose: () => combobox.resetSelectedOption(),
      onDropdownOpen: () => combobox.updateSelectedOptionIndex('active'),
    })

    const [selected, setSelected] = useState<string[]>(['Floyd Miles'])

    const handleValueSelect = (val: string) =>
      setSelected((current) =>
        current.includes(val)
          ? current.filter((v) => v !== val)
          : [...current, val]
      )

    const inputValue = selected.length > 0 && (
      <Group mt="-1px">
        {/* TODO: will use AvatarGroup once it's available */}
        <Group
          gap="0"
          css={{
            //  Give all children expect the first child a margin-left of -12px
            '> * + *': {
              marginLeft: '-12px',
            },
          }}
        >
          {selected.map((value) => {
            const option = people.find((person) => person.value === value)
            return option ? (
              <Avatar size="sm" color={option.color} key={option.value}>
                {option.firstName[0] + option.lastName[0]}
              </Avatar>
            ) : null
          })}
        </Group>
        {selected.length > 1
          ? `${selected.length} Assignees`
          : selected.length === 1
          ? people.find((person) => person.value === selected[0])?.firstName +
            ' ' +
            people.find((person) => person.value === selected[0])?.lastName
          : null}
      </Group>
    )

    return (
      <Combobox store={combobox} onOptionSubmit={handleValueSelect}>
        <Combobox.Target>
          <Combobox.InputBase
            component="button"
            type="button"
            pointer
            suffix={<Combobox.Chevron />}
            suffixPointerEvents="none"
            onClick={() => combobox.toggleDropdown()}
          >
            {inputValue || (
              <Combobox.InputPlaceholder>
                Select assignees
              </Combobox.InputPlaceholder>
            )}
          </Combobox.InputBase>
        </Combobox.Target>

        <Combobox.Dropdown>
          <Combobox.Options>
            {people.map(({ value, color, firstName, lastName }) => (
              <Combobox.Option
                value={value}
                key={value}
                active={selected.includes(value)}
              >
                <Checkbox
                  checked={selected.includes(value)}
                  aria-hidden
                  tabIndex={-1}
                  style={{ pointerEvents: 'none' }}
                  label={
                    <Group>
                      <Avatar size="sm" color={color}>
                        {firstName[0] + lastName[0]}
                      </Avatar>
                      {firstName} {lastName}
                    </Group>
                  }
                  // need this to remove HTML warning
                  onChange={() => undefined}
                />
              </Combobox.Option>
            ))}
          </Combobox.Options>
        </Combobox.Dropdown>
      </Combobox>
    )
  },
}

const allItemsNode = {
  id: 'allCategories',
  name: 'All Categories',
}
const treeData = [
  {
    id: 'asset',
    name: 'Asset',
    children: [
      {
        id: 'architecturalAsset',
        name: 'Architectural Asset',
      },
      {
        id: 'distributionAsset',
        name: 'Distribution Asset',
      },
    ],
  },
  {
    id: 'buildingComponent',
    name: 'Building Component',
    children: [
      {
        id: 'architecturalBuildingComponent',
        name: 'Architectural Building Component',
        children: [
          {
            id: 'ceiling',
            name: 'Ceiling',
          },
          {
            id: 'facade',
            name: 'Facade',
          },
          {
            id: 'floor',
            name: 'Floor',
          },
          {
            id: 'wall',
            name: 'Wall',
          },
        ],
      },
      {
        id: 'structuralBuildingComponent',
        name: 'Structural Building Component',
      },
    ],
  },
  {
    id: 'collection',
    name: 'Collection',
  },
  {
    id: 'component',
    name: 'Component',
  },
  {
    id: 'space',
    name: 'Space',
  },
]
