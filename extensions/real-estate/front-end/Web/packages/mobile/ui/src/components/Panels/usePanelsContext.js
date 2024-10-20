import { Children, useLayoutEffect, useState } from 'react'
import _ from 'lodash'
import { useGlobalPanels, useUser } from 'providers'
import { useUniqueId } from 'hooks'
import { useEffectOnceMounted } from '@willow/common'

export default function usePanelsContext(props) {
  const {
    horizontal = false,
    resizable = true,
    defaultMaximized,
    children,
  } = props

  const user = useUser()
  const globalPanels = useGlobalPanels()
  const panelsId = useUniqueId()

  function loadSizes() {
    const savedSizes = user.options.panelsSizes ?? []

    const sizes = Children.toArray(children).map((child) => ({
      name: child?.props?.name,
      size: child?.props?.initialSize,
      maxSize: child?.props?.maxSize,
    }))

    return sizes.map(
      (size) =>
        savedSizes.find((savedSize) => savedSize.name === size.name) ?? size
    )
  }

  const [sizes, setSizes] = useState(loadSizes())

  const [isDragging, setIsDragging] = useState(false)
  const [maximized, setMaximized] = useState(defaultMaximized)

  useEffectOnceMounted(() => {
    const nextMaximized = globalPanels.panelsGroups.find(
      (panelsGroup) => panelsGroup.panelsId === panelsId
    )?.maximized
    if (maximized !== nextMaximized) {
      setMaximized(nextMaximized)
    }
  }, [globalPanels])

  const minimized =
    globalPanels.panelsGroups.find(
      (panelsGroup) => panelsGroup.panelsId === panelsId
    )?.minimized ?? []

  function loadPanels() {
    const minimizedPanels = user.options.minimizedPanels ?? []

    const nextMinimizedPanel = minimizedPanels.find((panel) =>
      sizes.map((size) => size.name).includes(panel)
    )
    if (nextMinimizedPanel != null) {
      globalPanels.minimizePanel(nextMinimizedPanel, true)
    }

    const maximizedPanels = user.options.maximizedPanels ?? []

    const nextMaximizedPanel = maximizedPanels.find((panel) =>
      sizes.map((size) => size.name).includes(panel)
    )
    if (nextMaximizedPanel != null) {
      globalPanels.maximizePanel(nextMaximizedPanel, true)
    }
  }

  function saveMinimizedPanels(panel, value) {
    const nextMinimizedValue =
      value === undefined ? !minimized.includes(panel) : value

    const minimizedPanels = user.options.minimizedPanels ?? []

    const nextMinimizedPanels = nextMinimizedValue
      ? _.uniq([panel, ...minimizedPanels])
      : minimizedPanels.filter(
          (nextMinimizedPanel) => nextMinimizedPanel !== panel
        )

    user.saveUserOptions('minimizedPanels', nextMinimizedPanels)
  }

  function saveMaximizedPanels(panel, value) {
    const nextValue = value === undefined ? panel !== maximized : value

    const maximizedPanels = user.options.maximizedPanels ?? []

    const nextMaximizedPanels = nextValue
      ? _.uniq([panel, ...maximizedPanels])
      : maximizedPanels.filter(
          (nextMaximizedPanel) => nextMaximizedPanel !== panel
        )

    user.saveUserOptions('maximizedPanels', nextMaximizedPanels)
  }

  useLayoutEffect(() => {
    loadPanels()
  }, [])

  return {
    horizontal,
    resizable,
    isDragging,
    maximized,
    minimized,
    sizes,

    setIsDragging,
    setSizes,

    minimizePanel(panel, value) {
      globalPanels.minimizePanel(panel, value)

      saveMinimizedPanels(panel, value)
    },

    maximizePanel(panel, value) {
      globalPanels.maximizePanel(panel, value)

      saveMaximizedPanels(panel, value)
    },

    registerPanel(panel) {
      globalPanels.registerPanel({
        panelsId,
        panel,
      })
    },

    unregisterPanel(panel) {
      globalPanels.unregisterPanel({
        panelsId,
        panel,
      })
    },
  }
}
