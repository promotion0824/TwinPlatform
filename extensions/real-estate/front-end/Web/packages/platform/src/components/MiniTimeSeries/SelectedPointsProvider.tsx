import _ from 'lodash'
import { useCallback, useReducer } from 'react'
import SensorPointContext, {
  ApiParams,
  Point,
  State,
} from './SelectedPointsContext'
import colors from './colors.json'

enum ActionType {
  'remove',
  'loading',
  'loaded',
  'selected',
  'deselected',
  'updateParams',
}

type Action =
  | {
      type:
        | ActionType.remove
        | ActionType.loading
        | ActionType.selected
        | ActionType.deselected
      sitePointId: string
    }
  | {
      type: ActionType.loaded
      point: Point
    }
  | {
      type: ActionType.updateParams
      params: ApiParams
    }

const defaultState: State = {
  points: [],
  loadingPointIds: [],
  pointIds: [],
  pointColorMap: {},
}

let nextColorIndex = 0

function filterAwayId(thisArray: string[], thisId: string) {
  return thisArray.filter((thatId: string): boolean => thisId !== thatId)
}

const reducer = (state: State, action: Action): State => {
  switch (action.type) {
    // Keep track of point that is loading in progress.
    case ActionType.loading: {
      return {
        ...state,
        loadingPointIds: state.loadingPointIds.includes(action.sitePointId)
          ? state.loadingPointIds
          : [...state.loadingPointIds, action.sitePointId],
      }
    }
    // Remove point from loading list and selected list. - This is rarely called - only happens when
    // there is issue loading the point data.
    case ActionType.remove: {
      return {
        ...state,
        loadingPointIds: filterAwayId(
          state.loadingPointIds,
          action.sitePointId
        ),
        pointIds: filterAwayId(state.pointIds, action.sitePointId),
      }
    }
    // Handles when point data has been loaded.
    case ActionType.loaded: {
      const hasPreviousData = state.points.some(
        (point) => point.sitePointId === action.point.sitePointId
      )
      const updatedPoints = hasPreviousData
        ? state.points.map((point) =>
            point.sitePointId === action.point.sitePointId
              ? { ...point, ...action.point }
              : point
          )
        : [...state.points, { ...action.point }]
      return {
        ...state,
        points: updatedPoints,
        loadingPointIds: filterAwayId(
          state.loadingPointIds,
          action.point.sitePointId
        ),
      }
    }
    // Handles selection of a point
    case ActionType.selected: {
      const isSelected = state.pointIds.includes(action.sitePointId)
      return {
        ...state,
        pointIds: isSelected
          ? state.pointIds
          : _.xor(state.pointIds, [action.sitePointId]),
        pointColorMap: isSelected
          ? state.pointColorMap
          : {
              ...state.pointColorMap,
              [action.sitePointId]: colors[nextColorIndex++ % colors.length],
            },
      }
    }
    // Handles deselection of a point
    case ActionType.deselected: {
      return {
        ...state,
        points: state.points.filter(
          (point) => point.sitePointId !== action.sitePointId
        ),
        pointIds: filterAwayId(state.pointIds, action.sitePointId),
        loadingPointIds: filterAwayId(
          state.loadingPointIds,
          action.sitePointId
        ),
        pointColorMap: {
          ...state.pointColorMap,
          [action.sitePointId]: undefined,
        },
      }
    }
    case ActionType.updateParams: {
      return {
        ...state,
        params: action.params,
      }
    }
    default:
      return state
  }
}

const SelectedPointsProvider = ({ children }) => {
  const [state, dispatch] = useReducer(reducer, defaultState)
  const isLoading = !!state.loadingPointIds.length

  const onSelectPoint = useCallback(
    (sitePointId: string, isSelected = true) =>
      dispatch({
        type: isSelected ? ActionType.selected : ActionType.deselected,
        sitePointId,
      }),
    []
  )
  const onLoadPoint = useCallback(
    (sitePointId: string) =>
      dispatch({ type: ActionType.loading, sitePointId }),
    []
  )
  const onLoadedPoint = useCallback(
    (point) => dispatch({ type: ActionType.loaded, point }),
    []
  )
  const onRemovePoint = useCallback(
    (sitePointId: string) => dispatch({ type: ActionType.remove, sitePointId }),
    []
  )
  const onUpdateParams = useCallback(
    (params: ApiParams) => dispatch({ type: ActionType.updateParams, params }),
    []
  )
  return (
    <SensorPointContext.Provider
      value={{
        ...state,
        onSelectPoint,
        onLoadPoint,
        onLoadedPoint,
        onRemovePoint,
        onUpdateParams,
        isLoading,
      }}
    >
      {children}
    </SensorPointContext.Provider>
  )
}

export default SelectedPointsProvider
