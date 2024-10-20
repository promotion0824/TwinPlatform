import { useState } from 'react'
import { GlobalPanelsContext } from './GlobalPanelsContext'

export default function GlobalPanelsProvider(props) {
  const [panelsGroups, setPanelsGroups] = useState([])

  const context = {
    panelsGroups,

    minimizePanel(panel, value) {
      setPanelsGroups((prevPanelsGroups) =>
        prevPanelsGroups.map((prevPanelsGroup) =>
          prevPanelsGroup.panels.includes(panel)
            ? {
                ...prevPanelsGroup,
                minimized:
                  (value === true ||
                    !prevPanelsGroup.minimized.includes(panel)) &&
                  value !== false
                    ? [...prevPanelsGroup.minimized, panel]
                    : prevPanelsGroup.minimized.filter(
                        (prevPanel) => prevPanel !== panel
                      ),
              }
            : prevPanelsGroup
        )
      )
    },

    maximizePanel(panel, value) {
      setPanelsGroups((prevPanelsGroups) =>
        prevPanelsGroups.map((prevPanelsGroup) =>
          prevPanelsGroup.panels.includes(panel)
            ? {
                ...prevPanelsGroup,
                maximized:
                  (value === true || prevPanelsGroup.maximized !== panel) &&
                  value !== false
                    ? panel
                    : undefined,
              }
            : prevPanelsGroup
        )
      )
    },

    registerPanel(panelsGroup) {
      setPanelsGroups((prevPanelsGroups) => {
        const panelsExists = prevPanelsGroups.some(
          (prevPanelsGroup) => prevPanelsGroup.panelsId === panelsGroup.panelsId
        )
        return panelsExists
          ? prevPanelsGroups.map((prevPanelsGroup) =>
              prevPanelsGroup.panelsId === panelsGroup.panelsId
                ? {
                    ...prevPanelsGroup,
                    panels: [...prevPanelsGroup.panels, panelsGroup.panel],
                  }
                : prevPanelsGroup
            )
          : [
              ...prevPanelsGroups,
              {
                panelsId: panelsGroup.panelsId,
                panels: [panelsGroup.panel],
                maximized: undefined,
                minimized: [],
              },
            ]
      })
    },

    unregisterPanel(panelsGroup) {
      setPanelsGroups((prevPanelsGroups) =>
        prevPanelsGroups
          .map((prevPanelsGroup) =>
            prevPanelsGroup.panelsId === panelsGroup.panelsId
              ? {
                  ...prevPanelsGroup,
                  panels: prevPanelsGroup.panels.filter(
                    (prevPanel) => prevPanel === panelsGroup.panel
                  ),
                }
              : prevPanelsGroup
          )
          .filter((prevPanelsGroup) => prevPanelsGroup.panels.length > 0)
      )
    },
  }

  return <GlobalPanelsContext.Provider {...props} value={context} />
}
