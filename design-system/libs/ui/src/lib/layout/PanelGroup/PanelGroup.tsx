import _ from 'lodash'
import React, {
  ReactElement,
  useCallback,
  useEffect,
  useMemo,
  useReducer,
  useRef,
  useState,
} from 'react'
import {
  PanelGroup as ResizablePanelGroup,
  PanelGroupProps as ResizablePanelGroupProps,
} from 'react-resizable-panels'
import styled from 'styled-components'

import Panel from './Panel'
import { PanelGroupContext, PanelGroupContextProps } from './PanelGroupContext'
import { INITIALIZE_PANEL_IDS, panelIdsReducer, stringToObject } from './utils'
import { useStylesAndProps, WillowStyleProps } from '../../utils'

export interface PanelGroupProps
  extends WillowStyleProps,
    Omit<ResizablePanelGroupProps, 'direction'> {
  /**
   * The orientation of the panels within the group
   */
  direction?: ResizablePanelGroupProps['direction']
  /**
   * Avoid using `<Fragment>` as children within `<PanelGroup>` as this will cause UI
   * issues to resizable panels. Instead, please use arrays of elements as
   * children as needed.
   */
  children: ReactElement | ReactElement[]
  /**
   * The size of the gap between panels. The resize handle is hidden if set to small.
   * @default medium
   */
  gapSize?: PanelGroupContextProps['gapSize']
  /**
   * Whether all the child panels within this group are resizable.
   */
  resizable?: boolean
  /**
   * The layout of the panels within the group will be persisted in storage
   * if an `autoSaveId` is provided.
   *
   * Each `PanelGroup` include sub `PanelGroup` should have it's own
   * `autoSaveId` if layout needs to be persisted.
   *
   * `autoSaveId` should be unique under the same domain otherwise it will have
   * conflicts under the same storage (i.e. same localStorage by default).
   */
  autoSaveId?: ResizablePanelGroupProps['autoSaveId']
  /**
   * Custom storage API; defaults to localStorage.
   *
   * Storage API must define the following synchronous methods:
   * `getItem: (name:string) => string`
   * `setItem: (name: string, value: string) => void`
   */
  storage?: ResizablePanelGroupProps['storage']
}

const StyledPanelGroup = styled(ResizablePanelGroup)({
  boxSizing: 'border-box',
})

export const ResizablePanelGroupError =
  'Some of the panels within <PanelGroup resizable> contains defaultSize and are collapsible. Please ensure that there are at least one non-collapsible panel without defaultSize within the group.'

/**
 * `<PanelGroup>` is the container for a group of panels that displays either
 * horizontally (by default) or vertically.
 *
 * Each `Panel` is either fixed or resizable, and the panel may be collapsible. This component uses
 * [react-resizable-panels](https://github.com/bvaughn/react-resizable-panels) for resizable capability.
 *
 * #### Known issues:
 * - When panel group is resizable and a `defaultSize` is set to at least 1 of the panel,
 * please ensure that at least one panel must not be collapsible and at least one of the non-collapsible
 * panel to no `defaultSize` set. This is to avoid [this issue](https://github.com/bvaughn/react-resizable-panels/issues/160).
 */
const PanelGroup = ({
  autoSaveId,
  children,
  direction = 'horizontal',
  gapSize = 'medium',
  resizable = false,
  storage = localStorage,
  ...rest
}: PanelGroupProps) => {
  const idsRef = useRef<string[]>([])
  const persistPanelLayout = Boolean(autoSaveId)
  const activePanelsStorageKey = persistPanelLayout
    ? `PanelGroup:active:${autoSaveId}` // same format as react-resizable-panels
    : ''

  // Since we are rendering a collapsed panel as a custom div instead of the
  // <Panel> component, the base library generate a new panelGroupIds for
  // the same PanelGroup. Consider a PanelGroup with 3 panels, it will store
  // layout info as {'1:10,10,2:10': [30,20,30]}. Collapsing the
  // first panel (index = 0) adds a new pair like {'1:10,2:10': [50,50]}.
  // Thus, storing activePanelIds is essential to identify un-collapsed Panels.
  // Once the Panel renders as the correct component, it will produce the same
  // panelGroupIds and will match to the corresponding ratio array in storage.
  //
  // Used to store which PanelGroup combination is currently active in storage.
  // could be empty array, which means all the panels are collapsed,
  // if null means no initial active group id stored, will be initialized later
  const [activePanelIds, dispatchActivePanelIds] = useReducer(
    panelIdsReducer,
    stringToObject<string[]>(storage.getItem(activePanelsStorageKey))
  )

  const [initialized, setInitialized] = useState<boolean>(
    activePanelIds !== null
  )
  const updateActivePanelIdsInStorage = useMemo(
    () => (ids: string[]) => {
      storage.setItem(activePanelsStorageKey, JSON.stringify(ids))
    },
    [activePanelsStorageKey, storage]
  )

  useEffect(() => {
    if (activePanelIds) {
      // sync state change to storage
      updateActivePanelIdsInStorage(activePanelIds)
    }
  }, [activePanelIds, updateActivePanelIdsInStorage])

  /**
   * Initialization of panel layout information in localStorage happens after
   * first useEffect in PanelGroup, invoked by react-resizable-panels.
   * Therefore, this initialize function with an initialized flag is needed to
   * get the panel layout information during the first `setItem` call by the base
   * library and then set initial activePanelIds to storage.
   */
  const initializeActiveGroupIds = useCallback((value: string) => {
    const panelLayoutIds = Object.keys(stringToObject(value) || {})

    dispatchActivePanelIds({
      type: INITIALIZE_PANEL_IDS,
      ids: panelLayoutIds[0].split(','),
    })
    setInitialized(true)
  }, [])

  const panelGroupValue = useMemo(
    () => ({
      gapSize,
      isVertical: direction === 'vertical',
      resizable,
      idsRef,
      activePanelIds,
      dispatchActivePanelIds,
      persistPanelLayout,
    }),
    [
      gapSize,
      direction,
      resizable,
      activePanelIds,
      dispatchActivePanelIds,
      persistPanelLayout,
    ]
  )

  if (resizable) {
    const reactChildren = React.Children.map(
      children,
      (child: ReactElement) => child
    ).filter((child) => child.type === Panel)

    if (
      reactChildren.some((child) => child.props.defaultSize != null) &&
      reactChildren.some(
        (child) => !child.props.collapsible && child.props.defaultSize != null
      )
    ) {
      throw new Error(ResizablePanelGroupError)
    }
  }

  const interceptedStorage = useMemo(() => {
    return {
      // Cannot use `getItem: storage.getItem` directly, which will return
      // undefined in `loadSerializedPanelGroupState` in react-resizable-panels,
      // not sure why.
      getItem: (key: string) => {
        return storage.getItem(key)
      },
      setItem: (key: string, value: string) => {
        if (persistPanelLayout && !initialized) {
          initializeActiveGroupIds(value)
        }
        storage.setItem(key, value)
      },
    }
  }, [initializeActiveGroupIds, initialized, persistPanelLayout, storage])

  return (
    <PanelGroupContext.Provider value={panelGroupValue}>
      <StyledPanelGroup
        direction={direction}
        autoSaveId={autoSaveId}
        storage={interceptedStorage}
        // some props will be discarded by react-resizable-panel,
        // for example data-testid
        {...useStylesAndProps(rest)}
      >
        {React.Children.map(children, (child: ReactElement, index) => {
          if (!idsRef.current[index]) {
            idsRef.current.push(_.uniqueId())
          }

          if (child.type === PanelGroup) {
            return (
              <Panel
                grouped
                /* this attribute won't be added to final element */ id={
                  idsRef.current[index]
                }
              >
                {child}
              </Panel>
            )
          }

          return React.cloneElement(child, {
            id: idsRef.current[index],
          })
        })}
      </StyledPanelGroup>
    </PanelGroupContext.Provider>
  )
}

export default PanelGroup
