import { act, render, screen } from '../../../jest/testUtils'

import { CheckboxGroup, CheckboxGroupProps } from '.'
import { Checkbox } from '../Checkbox'

describe('CheckboxGroup with Checkbox as children', () => {
  describe('with string values', () => {
    const CheckboxGroupForStringValues = (
      props: Partial<CheckboxGroupProps>
    ) => (
      <CheckboxGroup {...props} type="string">
        <Checkbox label="Label1" value="value1" />
        <Checkbox label="Label2" value="value2" />
        <Checkbox label="Label3" value="value3" />
      </CheckboxGroup>
    )

    it('default value should be working', () => {
      render(<CheckboxGroupForStringValues defaultValue={['value1']} />)

      expect(getCheckbox('Label1')).toBeChecked()
    })

    it('value should be working', () => {
      render(<CheckboxGroupForStringValues value={['value1']} />)

      expect(getCheckbox('Label1')).toBeChecked()
    })

    it('custom onChange should receive string[] values', () => {
      const onChange = jest.fn()
      render(<CheckboxGroupForStringValues onChange={onChange} />)

      act(() => {
        getCheckbox('Label1').click()
      })

      expect(onChange).toHaveBeenCalledWith(['value1'])
    })
  })

  describe('with number values', () => {
    const CheckboxGroupForNumberValues = (
      props: Partial<CheckboxGroupProps<'number'>>
    ) => (
      <CheckboxGroup {...props} type="number">
        <Checkbox label="Label1" value={1} />
        <Checkbox label="Label2" value={2} />
        <Checkbox label="Label3" value={3} />
      </CheckboxGroup>
    )

    it('default value should be working', () => {
      render(<CheckboxGroupForNumberValues defaultValue={[1]} />)

      expect(getCheckbox('Label1')).toBeChecked()
    })

    it('value should be working', () => {
      render(<CheckboxGroupForNumberValues value={[1]} />)

      expect(getCheckbox('Label1')).toBeChecked()
    })

    it('custom onChange should receive number[] values', () => {
      const onChange = jest.fn()
      render(<CheckboxGroupForNumberValues onChange={onChange} />)

      act(() => {
        getCheckbox('Label1').click()
      })

      expect(onChange).toHaveBeenCalledWith([1])
    })
  })

  it('with mixed value types should not throw', () => {
    expect(() =>
      render(
        <CheckboxGroup>
          <Checkbox label="Label1" value={1} />
          <Checkbox label="Label2" value="value2" />
        </CheckboxGroup>
      )
    ).not.toThrow()
  })

  describe('without values', () => {
    const CheckboxGroupWithoutValues = (props: Partial<CheckboxGroupProps>) => {
      return (
        <CheckboxGroup {...props} type={undefined}>
          <Checkbox label="Label1" />
          <Checkbox label="Label2" />
        </CheckboxGroup>
      )
    }
    it('should not throw', () => {
      expect(() => {
        render(<CheckboxGroupWithoutValues />)
      }).not.toThrow()
    })

    it('should call onChange with "undefined" when no value is provided', () => {
      const onChange = jest.fn()
      render(<CheckboxGroupWithoutValues onChange={onChange} />)

      getCheckbox('Label1').click()

      expect(onChange).toHaveBeenCalledWith(['undefined'])
    })
  })
})

describe.each([
  {
    data: [
      { value: 1, label: 'Label1' },
      { value: 2, label: 'Label2' },
      { value: 3, label: 'Label3' },
    ],
    defaultValue: [1],
    selectedValue: [1, 2],
    type: 'number',
  },
  {
    data: [
      { value: '1', label: 'Label1' },
      { value: '2', label: 'Label2' },
      { value: '3', label: 'Label3' },
    ],
    defaultValue: ['1'],
    selectedValue: ['1', '2'],
  },
  {
    data: [1, 2, 3],
    defaultValue: [1],
    selectedValue: [1, 2],
    type: 'number',
  },
  {
    data: ['1', '2', '3'],
    defaultValue: ['1'],
    selectedValue: ['1', '2'],
  },
])(
  `CheckboxGroup with data props`,
  ({ data, defaultValue, selectedValue, type }) => {
    const getLabelByIndex = (index: number) => {
      const item = data[index] // need to declare this constant to avoid an error in type-check test
      return typeof item === 'object' ? item.label : item.toString()
    }

    const typeString = type
      ? type === 'string'
        ? 'string'
        : 'number'
      : undefined
    it('should render Checkbox with defaultValue applied', () => {
      render(
        <CheckboxGroup
          data={data}
          defaultValue={defaultValue}
          type={typeString}
        />
      )

      expect(getCheckbox(getLabelByIndex(0))).toBeChecked()
    })

    it('should be able to select multiple checkboxes', () => {
      render(<CheckboxGroup data={data} type={typeString} />)
      const checkbox1 = getCheckbox(getLabelByIndex(0))
      const checkbox2 = getCheckbox(getLabelByIndex(1))

      checkbox1.click()
      checkbox2.click()

      expect(checkbox1).toBeChecked()
      expect(checkbox2).toBeChecked()
    })

    it('should update onChange with array of values', () => {
      const onChange = jest.fn()
      render(
        <CheckboxGroup onChange={onChange} data={data} type={typeString} />
      )

      const checkbox1 = getCheckbox(getLabelByIndex(0))
      const checkbox2 = getCheckbox(getLabelByIndex(1))

      checkbox1.click()
      checkbox2.click()

      expect(onChange).toHaveBeenCalledWith(selectedValue)
    })
  }
)

it('CheckboxGroup option should be disabled with disabled item in data props', () => {
  render(
    <CheckboxGroup
      type="number"
      data={[
        { value: 1, label: 'Label1', disabled: true },
        { value: 2, label: 'Label2' },
        { value: 3, label: 'Label3' },
      ]}
    />
  )

  expect(getCheckbox('Label1')).toBeDisabled()
})

const getCheckbox = (name: string) =>
  screen.getByRole('checkbox', {
    name,
  })
