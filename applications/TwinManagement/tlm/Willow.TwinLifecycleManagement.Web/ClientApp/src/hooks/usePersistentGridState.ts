import { GridApiPro } from '@mui/x-data-grid-pro/models/gridApiPro';
import { MutableRefObject, useEffect, useRef } from 'react';

/**
 * This hook will allow grid configuration to be persistent.
 * It will listen to grid events and save the state in local storage and,
 * on first grid component render, restore the state from local storage.
 * example:
 * `
 * function MyGrid() {
 *   const apiRef = useGridApiRef()
 *   usePersistColumnSettings(apiRef, 'customers-grid')
 *   return <DataGrid apiRef={apiRef} />
 * }
 * `
 */
export function usePersistentGridState(
  apiRef: MutableRefObject<GridApiPro>,
  key: string,
  isDependenciesLoaded: boolean = true // bandaid solution for async columns issue, ontology and locations need to be loaded before the grid is initialized
) {
  const isGridInitialized = useRef(false);
  const storageKey = `${key}-grid-state`;
  const savedState = parseOrNull(localStorage.getItem(storageKey));

  useEffect(() => {
    const ref = apiRef?.current;
    if (!ref?.subscribeEvent || !isDependenciesLoaded) return;

    if (!isGridInitialized.current) {
      isGridInitialized.current = true;

      const gridStateJSON = localStorage.getItem(storageKey);
      if (gridStateJSON) {
        const parsedGridState = parseOrNull(gridStateJSON);
        if (parsedGridState) {
          try {
            ref.restoreState(parsedGridState);
          } catch (e) {
            console.warn(`Failed to restore grid state`, e);
          }
        }
      }
    }

    const subs: VoidFunction[] = [];

    // Save grid state to local storage based on grid events we're listening to.
    const save = () => {
      const state = ref.exportState(); // get the grid state
      state.preferencePanel = { open: false }; // always close the preference panel
      state.pagination = {}; // don't save pagination state
      if (state && isGridInitialized.current) {
        localStorage.setItem(storageKey, JSON.stringify(state));
      }
    };

    const subscribe = (event: any) => {
      subs.push(ref.subscribeEvent(event, save)); // https://mui.com/x/react-data-grid/events/#with-apiref-current-subscribeevent
    };

    // List of MUI grid's events we can listen to  https://mui.com/x/react-data-grid/events/#catalog-of-events
    subscribe('columnResizeStop');
    subscribe('columnOrderChange');
    subscribe('columnVisibilityModelChange');
    subscribe('sortModelChange');
    subscribe('filterModelChange');

    // unsubscribe event handler on component dismount
    return () => {
      subs.forEach((unsubscribe) => {
        unsubscribe();
      });
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [storageKey, isGridInitialized, apiRef.current, isDependenciesLoaded]);

  return { savedState };
}

function parseOrNull(raw: unknown) {
  if (!raw) return null;

  if (typeof raw === 'string') {
    try {
      return JSON.parse(raw);
    } catch (e) {
      console.warn(`Failed to parse: ${raw.substring(0, 50)}`);
      return null;
    }
  }

  return null;
}
