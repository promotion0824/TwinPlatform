import { updateTargetLabel, replaceLabelForOption } from './updateTargetLabel'

const getLabel = (label: string) => `${label} (browser default)`
const targetValue = 'targetValue'
describe('replaceLabelForOption', () => {
  it('should update only label for string type option', () => {
    const option = targetValue
    expect(replaceLabelForOption(option, targetValue, getLabel)).toEqual({
      value: option,
      label: getLabel(option),
    })
  })

  it('should update only label for object type option', () => {
    const option = { value: targetValue, label: targetValue }
    expect(replaceLabelForOption(option, targetValue, getLabel)).toEqual({
      ...option,
      label: getLabel(option.label),
    })
  })

  it('should return option as string if value does not match targetValue', () => {
    const option = 'option'
    expect(replaceLabelForOption(option, targetValue, getLabel)).toEqual(option)
  })

  it('should return option as object if value does not match targetValue', () => {
    const option = { value: 'option', label: 'option' }
    expect(replaceLabelForOption(option, targetValue, getLabel)).toEqual(option)
  })
})

describe('updateTargetLabel', () => {
  it('should update label for string arrays', () => {
    const options = ['option1', 'option2', targetValue]
    expect(updateTargetLabel(options, targetValue, getLabel)).toEqual([
      ...options.slice(0, 2),
      {
        value: targetValue,
        label: getLabel(targetValue),
      },
    ])
  })

  it('should update label for SelectItem arrays', () => {
    const options = [
      { value: 'option1', label: 'option1' },
      { value: 'option2', label: 'option2' },
      { value: targetValue, label: targetValue },
    ]
    expect(updateTargetLabel(options, targetValue, getLabel)).toEqual([
      ...options.slice(0, 2),
      {
        ...options[2],
        label: getLabel(options[2].label),
      },
    ])
  })

  it('should update labels for ComboboxGroup arrays', () => {
    const options = [
      {
        group: 'group1',
        items: ['option1', 'option2'],
      },
      {
        group: 'group2',
        items: ['option3', 'option4', targetValue],
      },
    ]
    expect(updateTargetLabel(options, targetValue, getLabel)).toEqual([
      options[0],
      {
        group: options[1].group,
        items: [
          ...options[1].items.slice(0, 2),
          {
            value: targetValue,
            label: getLabel(targetValue),
          },
        ],
      },
    ])
  })

  it('should update all matching labels for mixed arrays', () => {
    const options = [
      targetValue,
      { value: targetValue, label: 'option2' },
      {
        group: 'group1',
        items: [targetValue],
      },
    ]

    expect(updateTargetLabel(options, targetValue, getLabel)).toEqual([
      {
        value: targetValue,
        label: getLabel(targetValue),
      },
      {
        value: targetValue,
        label: getLabel('option2'),
      },
      {
        group: 'group1',
        items: [
          {
            value: targetValue,
            label: getLabel(targetValue),
          },
        ],
      },
    ])
  })
})
