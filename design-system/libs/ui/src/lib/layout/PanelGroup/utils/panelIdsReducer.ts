import { filter, union } from 'lodash'

export const INITIALIZE_PANEL_IDS = 'INITIALIZE_PANEL_IDS'
export const REMOVE_PANEL_ID = 'REMOVE_PANEL_ID'
export const ADD_PANEL_ID = 'ADD_PANEL_ID'

export type PanelIdsState = string[] | null
export type PanelIdsAction =
  | { type: typeof REMOVE_PANEL_ID; id: string }
  | { type: typeof ADD_PANEL_ID; id: string }
  | { type: typeof INITIALIZE_PANEL_IDS; ids: string[] }

/**
 * Will add or remove current panelId from the activePanelIds stored
 * in React state when toggle the collapse status of the panel. Cannot use
 * storage directly as it is not reactive to changes.
 */
export function panelIdsReducer(state: PanelIdsState, action: PanelIdsAction) {
  switch (action.type) {
    case INITIALIZE_PANEL_IDS:
      return action.ids
    case REMOVE_PANEL_ID:
      if (state === null) {
        return null
      }
      return filter(state, (id) => id !== action.id)
    case ADD_PANEL_ID:
      return union(state, [action.id])
    default:
      return state
  }
}
