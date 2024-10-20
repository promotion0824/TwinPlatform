import {
  ADD_PANEL_ID,
  INITIALIZE_PANEL_IDS,
  REMOVE_PANEL_ID,
  panelIdsReducer,
} from './panelIdsReducer'

describe('panelIdsReducer', () => {
  it('should initialize with type = INITIALIZE_PANEL_IDS', () => {
    const initialState = null
    const ids = ['1:10', '10']
    const action = {
      type: INITIALIZE_PANEL_IDS,
      ids: ids,
    } as const

    const newState = panelIdsReducer(initialState, action)
    expect(newState).toEqual(ids)
  })

  it('should handle REMOVE_PANEL_ID when state is not null', () => {
    const initialState = ['1:10', '10']
    const action = {
      type: REMOVE_PANEL_ID,
      id: '10',
    } as const

    const newState = panelIdsReducer(initialState, action)
    expect(newState).toEqual(['1:10'])
  })

  it('should handle REMOVE_PANEL_ID when state is null', () => {
    const initialState = null
    const action = {
      type: REMOVE_PANEL_ID,
      id: '1:10',
    } as const

    const newState = panelIdsReducer(initialState, action)
    expect(newState).toBeNull()
  })

  it('should handle ADD_PANEL_ID when state is not null', () => {
    const initialState = ['1:10']
    const action = {
      type: ADD_PANEL_ID,
      id: '2:10',
    } as const

    const newState = panelIdsReducer(initialState, action)
    expect(newState).toEqual(['1:10', '2:10'])
  })

  it('should handle ADD_PANEL_ID when state is null', () => {
    const initialState = null
    const action = {
      type: ADD_PANEL_ID,
      id: '2:10',
    } as const

    const newState = panelIdsReducer(initialState, action)
    expect(newState).toEqual(['2:10'])
  })
})
