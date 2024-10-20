import flattenObject from '../flattenObject'

describe('flattenObject', () => {
  test('Empty object', () => {
    expect(flattenObject({})).toEqual({})
  })

  test('Flattened object, includes "default" in key', () => {
    expect(
      flattenObject(
        {
          fontWeight: { default: 200, normal: 400, bold: 500, bolder: 600 },
        },
        '.'
      )
    ).toEqual({
      'fontWeight.default': '200',
      'fontWeight.normal': '400',
      'fontWeight.bold': '500',
      'fontWeight.bolder': '600',
    })
  })

  test('Flattened object, excludes "default" in key', () => {
    expect(
      flattenObject(
        {
          themes: {
            dark: {
              color: {
                bg: {
                  b1: {
                    default: 'gray-75',
                    hovered: 'gray-100',
                    activated: 'gray-125',
                  },
                  b2: {
                    default: 'gray-100',
                    hovered: 'gray-125',
                    activated: 'gray-150',
                  },
                },
              },
            },
            light: {
              color: {
                bg: {
                  b1: {
                    default: 'gray-75',
                    hovered: 'gray-100',
                    activated: 'gray-125',
                  },
                  b2: {
                    default: 'gray-100',
                    hovered: 'gray-125',
                    activated: 'gray-150',
                  },
                },
              },
            },
          },
        },
        '-',
        '--',
        true
      )
    ).toEqual({
      '--themes-dark-color-bg-b1': 'gray-75',
      '--themes-dark-color-bg-b1-hovered': 'gray-100',
      '--themes-dark-color-bg-b1-activated': 'gray-125',
      '--themes-dark-color-bg-b2': 'gray-100',
      '--themes-dark-color-bg-b2-hovered': 'gray-125',
      '--themes-dark-color-bg-b2-activated': 'gray-150',
      '--themes-light-color-bg-b1': 'gray-75',
      '--themes-light-color-bg-b1-hovered': 'gray-100',
      '--themes-light-color-bg-b1-activated': 'gray-125',
      '--themes-light-color-bg-b2': 'gray-100',
      '--themes-light-color-bg-b2-hovered': 'gray-125',
      '--themes-light-color-bg-b2-activated': 'gray-150',
    })
  })
})
