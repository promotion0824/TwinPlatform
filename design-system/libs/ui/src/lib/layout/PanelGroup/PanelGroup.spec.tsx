import { act, render } from '../../../jest/testUtils'

import { darkTheme } from '@willowinc/theme'
import { Panel, PanelContent, PanelGroup } from '.'
import { Tabs } from '../../navigation/Tabs'
import { getColor, getSpacing } from '../../utils'
import { ResizablePanelGroupError } from './PanelGroup'
import PanelHeader from './PanelHeader'

describe('Panel', () => {
  it('should throw error when Panel is not wrapped in PanelGroup', () => {
    jest.spyOn(console, 'error').mockImplementation()

    expect(() => render(<Panel>Panel 1</Panel>)).toThrowError(
      'Panel components must be wrapped in <PanelGroup/>'
    )
  })

  it('should throw error when PanelHeader is not wrapped in Panel', () => {
    jest.spyOn(console, 'error').mockImplementation()

    expect(() =>
      render(
        <PanelGroup>
          <PanelHeader>Panel Header</PanelHeader>
        </PanelGroup>
      )
    ).toThrowError('PanelHeader and PanelContent must be wrapped in <Panel/>')
  })

  it('should throw error in resizable PanelGroup when there are defaultSize set for panel', () => {
    jest.spyOn(console, 'error').mockImplementation()

    expect(() =>
      render(
        <PanelGroup resizable>
          <Panel defaultSize={20}>Panel 1</Panel>
          <Panel collapsible>Panel 1</Panel>
          <Panel collapsible>Panel 1</Panel>
        </PanelGroup>
      )
    ).toThrowError(ResizablePanelGroupError)
  })

  it('should render resizable panel group successfully', () => {
    const { baseElement } = render(
      <PanelGroup resizable>
        <Panel collapsible defaultSize={20}>
          Panel 1
        </Panel>
        <Panel collapsible defaultSize={20}>
          Panel 2
        </Panel>
        <Panel>Panel 3</Panel>
        <Panel collapsible>Panel 3</Panel>
      </PanelGroup>
    )
    expect(baseElement).toBeTruthy()
  })

  it('should render fixed panel group successfully', () => {
    const { baseElement } = render(
      <PanelGroup>
        <Panel>Panel 1</Panel>
        <Panel>Panel 2</Panel>
      </PanelGroup>
    )
    expect(baseElement).toBeTruthy()
  })

  it("should throw an errors if a Tab component isn't the first child of the tabs property", () => {
    jest.spyOn(console, 'error').mockImplementation()

    expect(() =>
      render(
        <PanelGroup>
          <Panel
            tabs={<div>I will throw because Tabs isn't the first child</div>}
          />
        </PanelGroup>
      )
    ).toThrow('The Tabs component must be the first child of the tabs property')
  })

  it("should throw an error if the tabs property doesn't have at least two correct children", () => {
    jest.spyOn(console, 'error').mockImplementation()

    expect(() =>
      render(
        <PanelGroup>
          <Panel
            tabs={
              <Tabs>
                <Tabs.List>
                  <Tabs.Tab value="1">Tab 1</Tabs.Tab>
                </Tabs.List>
              </Tabs>
            }
          />
        </PanelGroup>
      )
    ).toThrow(
      'The Tabs component must have at least 2 children, the 1st being a list of tabs, the 2nd being the content'
    )
  })
})

describe('Style props should work when', () => {
  const bgToken = 'core.blue.bg.bold.hovered' as const
  const spacingToken = 's12' as const

  const colorTestCases = [
    {
      propName: 'bg',
      styleName: 'background',
      propValue: bgToken,
      styleValue: getColor(bgToken, darkTheme),
    },
    {
      propName: 'c',
      styleName: 'color',
      propValue: bgToken,
      styleValue: getColor(bgToken, darkTheme),
    },
  ] as const
  const spacingTestCases = [
    {
      propName: 'm',
      styleName: 'margin',
      propValue: spacingToken,
      styleValue: getSpacing(spacingToken),
    },
    {
      propName: 'm',
      styleName: 'margin',
      propValue: '20px',
      styleValue: '20px',
    },
    {
      propName: 'p',
      styleName: 'padding',
      propValue: spacingToken,
      styleValue: getSpacing(spacingToken),
    },
    {
      propName: 'w',
      styleName: 'width',
      propValue: '200px',
      styleValue: '200px',
    },
  ] as const
  const allTestCases = [...colorTestCases, ...spacingTestCases] as const

  const getPanelGroup = (baseElement: HTMLElement) =>
    baseElement.querySelector('[data-panel-group]')

  const getPanel = (baseElement: HTMLElement) =>
    baseElement.querySelector('[data-panel]')
  it.each(allTestCases)(
    '$propName = $propValue applies to PanelGroup',
    ({ propName, styleName, propValue, styleValue }) => {
      const { baseElement } = render(
        <PanelGroup {...{ [propName]: propValue }}>
          <Panel>Panel 1</Panel>
        </PanelGroup>
      )

      expect(getPanelGroup(baseElement)).toHaveStyle(
        `${styleName}: ${styleValue}`
      )
    }
  )

  // spacing props won't work for Box in Jest, will test in Playground
  it.each(colorTestCases)(
    '$propName = $propValue applies to fixed Panel',
    ({ propName, styleName, propValue, styleValue }) => {
      const { baseElement } = render(
        <PanelGroup>
          <Panel {...{ [propName]: propValue }}>Panel 1</Panel>
        </PanelGroup>
      )

      expect(getPanel(baseElement)).toHaveStyle(`${styleName}: ${styleValue}`)
    }
  )

  it.each(allTestCases)(
    '$propName = $propValue applies to resizable Panel',
    ({ propName, styleName, propValue, styleValue }) => {
      const { baseElement } = render(
        <PanelGroup resizable>
          <Panel {...{ [propName]: propValue }}>Panel 1</Panel>
        </PanelGroup>
      )

      expect(getPanel(baseElement)).toHaveStyle(`${styleName}: ${styleValue}`)
    }
  )

  // spacing props won't work for Box in Jest, will test in Playground
  it.each(colorTestCases)(
    '$propName = $propValue applies to collapsed Panel',
    async ({ propName, styleName, propValue, styleValue }) => {
      const { baseElement, getByRole, queryByText } = render(
        <PanelGroup>
          <Panel {...{ [propName]: propValue }} collapsible>
            Panel 1
          </Panel>
        </PanelGroup>
      )
      // to collapse panel, need await here
      await act(() => getByRole('button').click())
      expect(queryByText('Panel 1')).not.toBeInTheDocument()

      expect(getPanel(baseElement)).toHaveStyle(`${styleName}: ${styleValue}`)
    }
  )

  // spacing props won't work for Box in Jest, will test in Playground
  test.each(colorTestCases)(
    '$propName = $propValue applies to Panel Content',
    ({ propName, styleName, propValue, styleValue }) => {
      const testId = 'panel-content'
      const { baseElement } = render(
        <PanelGroup>
          <Panel>
            <PanelContent {...{ [propName]: propValue }} data-testid={testId}>
              Panel 1
            </PanelContent>
          </Panel>
        </PanelGroup>
      )

      expect(baseElement.querySelector(`[data-testid=${testId}]`)).toHaveStyle(
        `${styleName}: ${styleValue}`
      )
    }
  )
})
