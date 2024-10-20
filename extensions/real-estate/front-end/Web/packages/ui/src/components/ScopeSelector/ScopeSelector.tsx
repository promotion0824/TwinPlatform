import { localStorage, titleCase } from '@willow/common'
import { Json } from '@willow/common/twins/view/twinModel'
import { ALL_LOCATIONS } from '@willow/ui/constants'
import { Popover } from '@willowinc/ui'

import { debounce } from 'lodash'
import { useCallback, useMemo, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'

import ScopeSelectorDropdown from './ScopeSelectorDropdown'
import ScopeSelectorTrigger from './ScopeSelectorTrigger'
import getScopeSelectorModel from './getScopeSelectorModel'

export type LocationNode = {
  isAllItemsNode?: boolean
  children?: LocationNode[]
  parents?: string[]
  twin: {
    id: string
    name: string
    siteId?: string
    metadata: {
      modelId: string
      [key: string]: Json
    }
    userId?: string
  }
}

// Currently "All Locations" is the only supported Portfolio.
export function generateDefaultPortfolios({
  allLocationsName,
  locations,
}: {
  allLocationsName: string
  locations: LocationNode[]
}) {
  return [
    {
      twin: {
        id: ALL_LOCATIONS,
        name: allLocationsName,
        siteId: '',
        metadata: {
          modelId: ALL_LOCATIONS,
        },
        userId: '',
      },
      children: locations,
    },
  ]
}

interface ScopeSelectorProps {
  /** A location to be selected by default. If none is provided, All Locations will be selected. */
  defaultLocation?: LocationNode
  /** The list of locations that should be displayed in the scope selector. */
  locations: LocationNode[]
  /** A function to be called with the new location once one is selected. */
  onLocationChange?: (location: LocationNode) => void
}

const DROPDOWN_LOCAL_STORAGE_KEY = 'scopeSelectorDropdownContentWidth'

export default function ScopeSelector({
  defaultLocation,
  locations,
  onLocationChange,
}: ScopeSelectorProps) {
  const {
    i18n: { language },
    t,
  } = useTranslation()

  const dropdownRef = useRef<HTMLDivElement | null>(null)
  const [dropdownContentWidth /* exclude borders */, setDropdownContentWidth] =
    useState(
      localStorage.get(DROPDOWN_LOCAL_STORAGE_KEY) ||
        466 /* default width same as previous ScopeSelectorDropdown */
    )
  const resizeObserver = useMemo(
    () =>
      new ResizeObserver(
        debounce((entries: ResizeObserverEntry[]) => {
          const [dropdown] = entries

          // The width is rounded here to help with, what seems to be, an issue with
          // subpixels that we've only seen on Surface tablets when a mouse is used.
          const contentWidth = Math.round(dropdown.contentRect.width)

          if (dropdown) {
            // save the ref so that we can unobserve the element later
            dropdownRef.current = dropdown.target as HTMLDivElement
          }

          if (contentWidth) {
            // update dropdownContentWidth so that the content will expand when resizing
            setDropdownContentWidth(contentWidth)
            localStorage.set(DROPDOWN_LOCAL_STORAGE_KEY, contentWidth)
          }
        }, 50)
      ),
    []
  )
  // cannot use useRef as the Dropdown portal seems to have different life cycle
  //  as this component, it will keep switching between null and valid.
  const dropdownCallbackRef = useCallback(
    (node: HTMLDivElement | null) => {
      if (node) {
        resizeObserver.observe(node)
        return
      }

      // node is null now,
      // we need to unobserve the element that stored in dropdownRef to clean up
      if (
        dropdownRef.current /* should always exist here, but just in case. */
      ) {
        resizeObserver.unobserve(dropdownRef.current)
        dropdownRef.current = null
      }
      resizeObserver.disconnect()
    },
    [resizeObserver]
  )

  const portfolios = generateDefaultPortfolios({
    allLocationsName: titleCase({
      language,
      text: t(getScopeSelectorModel(ALL_LOCATIONS).name),
    }),
    locations,
  })

  const [isOpen, setIsOpen] = useState(false)
  const selectedLocation = defaultLocation || portfolios[0]

  return (
    <Popover
      onChange={setIsOpen}
      opened={isOpen}
      position="bottom-start"
      withinPortal
    >
      <Popover.Target>
        <ScopeSelectorTrigger
          isOpen={isOpen}
          onClick={() => setIsOpen(!isOpen)}
          selectedLocation={selectedLocation}
        />
      </Popover.Target>
      <Popover.Dropdown
        css={{
          resize: 'horizontal',
          overflowX: 'auto',
        }}
        ref={dropdownCallbackRef}
      >
        <ScopeSelectorDropdown
          locations={locations}
          onSelect={(newLocation: LocationNode) => {
            setIsOpen(false)
            onLocationChange?.(newLocation)
          }}
          portfolios={portfolios}
          selectedLocation={selectedLocation}
          css={{
            // setting width for dropdown won't work, it will be overridden by
            // a width applied by style attribute from the base library.
            // So we need to define the width of the content in the dropdown to
            // size the dropdown.
            width: dropdownContentWidth,
          }}
        />
      </Popover.Dropdown>
    </Popover>
  )
}
