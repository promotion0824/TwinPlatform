import { render } from '../../../jest/testUtils'
import { SegmentedControl } from '.'

describe('SegmentedControl', () => {
  afterEach(() => jest.restoreAllMocks())

  it('should render successfully', () => {
    const { baseElement } = render(
      <SegmentedControl data={['preview', 'code', 'export']} />
    )
    expect(baseElement).toBeTruthy()
  })

  it('should throw an error if iconOnly is true but iconName is not provided', () => {
    jest.spyOn(console, 'error').mockImplementation()

    expect(() => {
      render(
        <SegmentedControl
          data={[
            { value: 'preview_val', label: 'Preview', iconOnly: true },
            { value: 'code_val', label: 'Code', iconName: 'code' },
            { value: 'export_val', label: 'Export' },
          ]}
        />
      )
    }).toThrow("iconOnly is set to true, but iconName isn't provided.")
  })
})
