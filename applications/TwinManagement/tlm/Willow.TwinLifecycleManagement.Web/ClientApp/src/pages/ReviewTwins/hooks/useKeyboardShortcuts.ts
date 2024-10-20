import { useEffect, useRef } from 'react';
import { useMappings } from '../MappingsProvider';
import {
  GridEventListener,
  GridColDef,
  gridExpandedSortedRowIdsSelector,
  gridExpandedRowCountSelector,
  gridVisibleColumnDefinitionsSelector,
} from '@mui/x-data-grid-pro';
import { Status } from '../../../services/Clients';

export default function useKeyboardShortcuts() {
  const {
    tableApiRef,
    handleStatusChange,
    tabState,
    selectedRowsState,
    putMappedEntryMutate,
    isLoadingState,
    cellCoordinateState,
    changeMappedEntriesStatusMutate,
  } = useMappings();

  const { isLoading: isPutLoading } = putMappedEntryMutate;
  const { isLoading: isChangeStatusLoading } = changeMappedEntriesStatusMutate;
  const { handleCellNavigation } = useCellNavigation();

  const isInitialized = useRef(false);
  useEffect(() => {
    isInitialized.current = false;
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [tabState[0]]);

  const isLoading = isLoadingState[0] !== null || isPutLoading || isChangeStatusLoading;

  // Handle entire page's keyboard shortcuts
  function keydownHandler(event: KeyboardEvent) {
    const isEditMode = Object.keys(tableApiRef.current.state.editRows).length > 0;

    // disable keyboard shortcuts when making PUT requests
    if (isLoading) {
      event.preventDefault();
      return;
    }

    // disable select all text on page
    if (event.ctrlKey && event.key === 'a' && !isEditMode) {
      event.preventDefault();
    }

    let selectedRows = tableApiRef.current.state.rowSelection;

    function handleArrorKeys(direction: 'top' | 'bottom' | 'right' | 'left') {
      if (!isEditMode) event.preventDefault(); // prevent scrolling
      if (tableApiRef.current.state.focus.cell === null) {
        if ((selectedRows.length > 0 || cellCoordinateState[0].rowIndex === 0) && isInitialized.current) return;
        let firstRowId = tableApiRef.current.getRowIdFromRowIndex(0);
        tableApiRef.current.setCellFocus(firstRowId, '__check__');
        cellCoordinateState[1]({ rowIndex: 0, colIndex: 0 });
        return;
      }

      if (!isEditMode) handleCellNavigation(direction);
      if (!isInitialized.current) isInitialized.current = true;
    }

    switch (event.key) {
      case 'Tab':
        event.preventDefault();
        return;
      case 'ArrowUp':
        handleArrorKeys('top');
        return;

      case 'ArrowDown':
        handleArrorKeys('bottom');
        return;

      case 'ArrowRight':
        handleArrorKeys('right');
        return;

      case 'ArrowLeft':
        handleArrorKeys('left');
        return;

      case 'a':
        // Toggle select/deselect all rows on Ctrl + a key press
        if (event.ctrlKey && !isEditMode) {
          if (tableApiRef.current.state.rows.dataRowIds.length === tableApiRef.current.state.rowSelection.length) {
            tableApiRef.current.selectRows(tableApiRef.current.getAllRowIds(), false);
            return;
          }

          tableApiRef.current.selectRows(tableApiRef.current.getAllRowIds());
          return;
        }

        if (tabState[0] === 'conflicts') return;
        if (!event.ctrlKey && !isEditMode) handleStatusChange(Status.Approved);

        return;

      case 'i':
        if (tabState[0] === 'conflicts') return;
        if (!event.ctrlKey && !isEditMode) handleStatusChange(Status.Ignore);
        return;

      // toggle tabs with shift + number
      case '!':
        if (event.shiftKey && !isEditMode) tabState[1]('things');
        return;

      case '@':
        if (event.shiftKey && !isEditMode) tabState[1]('points');
        return;

      case '#':
        if (event.shiftKey && !isEditMode) tabState[1]('spaces');
        return;

      case '$':
        if (event.shiftKey && !isEditMode) tabState[1]('miscellaneous');
        return;

      case '%':
        if (event.shiftKey && !isEditMode) tabState[1]('conflicts');
        return;
    }
  }

  useEffect(() => {
    window.addEventListener('keydown', keydownHandler);

    return () => {
      window.removeEventListener('keydown', keydownHandler);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedRowsState[0], isLoading, cellCoordinateState[0]]);

  // keep cellCoordinateState insync
  function colCellCoordinateSync(columnField: string) {
    const colIndex = gridVisibleColumnDefinitionsSelector(tableApiRef).findIndex(
      (column) => column.field === columnField
    );

    cellCoordinateState[1]((prev) => ({ ...prev, colIndex }));
  }

  // Handle MUI DatagGrid's keyboard shortcuts
  useEffect(
    () => {
      const handleRowClick: GridEventListener<'cellKeyDown'> = (params: any, event: any) => {
        let visibilityColumns = tableApiRef?.current.state.columns.columnVisibilityModel;
        let visibleColumns = tableApiRef?.current.state.columns.orderedFields;
        let editableColumns = tableApiRef?.current
          ?.getAllColumns?.()
          .filter((column: GridColDef) => column.editable && visibilityColumns[column.field])
          .map((column: GridColDef) => column.field);

        let focusCell = tableApiRef.current.state.focus.cell;
        let isEditMode = Object.keys(tableApiRef.current.state.editRows).length > 0;

        switch (event.key) {
          // Start/End row edit mode on Enter key press
          case 'Enter':
            if (isLoading) return;

            // start row edit
            if (tableApiRef.current.getRowMode(params.id) === 'view') {
              // if user presses enter on a non-editable checkbox field, move focus to next editable cell
              if (tableApiRef.current.state.focus.cell?.field === '__check__') {
                tableApiRef.current.setCellFocus(params.id, editableColumns[0]);
                colCellCoordinateSync(editableColumns[0]);
              }
              // if user presses enter on a non-editable cell, move focus to next editable cell
              else if (!editableColumns.includes(focusCell?.field!)) {
                let nextEditableField = findNextEditableColumn(focusCell?.field!, editableColumns, visibleColumns);
                tableApiRef.current.setCellFocus(params.id, nextEditableField);

                // keep cellCoordinateState insync with cell focus
                colCellCoordinateSync(nextEditableField);
              }

              tableApiRef.current.startRowEditMode({ id: params.id });
              return;
            }
            // end row edit
            else {
              tableApiRef.current.stopRowEditMode({ id: params.id });
              tableApiRef.current.setCellFocus(params.id, tableApiRef.current.state.focus.cell!.field);
            }
            return;

          // Move to next editable cell on Tab key press
          case 'Tab':
            if (tableApiRef.current.getRowMode(params.id) === 'edit') {
              let nextCellFieldInd = editableColumns.indexOf(focusCell!.field) + 1;

              // If current cell is the last editable cell in the row, move to the first editable cell
              if (
                (nextCellFieldInd === -1 || nextCellFieldInd >= editableColumns.length) &&
                focusCell?.field !== '__check__'
              ) {
                nextCellFieldInd = 0;
              }

              let editableCellField = editableColumns[nextCellFieldInd];
              tableApiRef.current.setCellFocus(params.id, editableCellField);

              // keep cellCoordinateState insync with cell focus
              colCellCoordinateSync(editableCellField);
            }
            return;

          // Discard changes and end row edit on Escape key press
          case 'Escape':
            let editRowIds = Object.keys(tableApiRef.current.state.editRows);
            editRowIds.forEach((id: string) => {
              tableApiRef.current.stopRowEditMode({ id, ignoreModifications: true });
            });
            return;

          // Select/de-select row on Space key press
          case ' ':
            if (!isEditMode && !isLoading) {
              event.preventDefault();
              tableApiRef.current.selectRow(params.id, !tableApiRef.current.isRowSelected(params.id));
            }
            return;

          default:
            return;
        }
      };
      if (Object.keys(tableApiRef.current).length === 0) return;
      return tableApiRef?.current?.subscribeEvent('cellKeyDown', handleRowClick);
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [isLoading, cellCoordinateState, tableApiRef]
  );
}

function findNextEditableColumn(currentColumn: string, editableColumns: string[], visibleColumns: string[]) {
  let currentColumnInd = visibleColumns.indexOf(currentColumn);
  let nextColumn = currentColumn;

  while (!editableColumns.includes(nextColumn)) {
    currentColumnInd += 1;
    if (currentColumnInd >= visibleColumns.length) currentColumnInd = 0;
    nextColumn = visibleColumns[currentColumnInd];
  }

  return nextColumn;
}

// handle case when cell focus is out of view and user navigates to next row with arrow keys
function useCellNavigation() {
  const { tableApiRef, tabState, cellCoordinateState } = useMappings();

  const [coordinates, setCoordinates] = cellCoordinateState;

  useEffect(() => {
    if (tableApiRef.current && tableApiRef.current?.state?.focus?.cell) {
      const { rowIndex, colIndex } = coordinates;
      tableApiRef?.current?.scrollToIndexes?.(coordinates);
      const id = gridExpandedSortedRowIdsSelector(tableApiRef)[rowIndex];
      const column = gridVisibleColumnDefinitionsSelector(tableApiRef)[colIndex];
      tableApiRef?.current?.setCellFocus(id, column.field);
    }
  }, [tableApiRef, coordinates]);

  useEffect(
    () => {
      setCoordinates({ rowIndex: 0, colIndex: 0 });
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [tabState[0]]
  );
  const handleCellNavigation = (position: string) => {
    const maxRowIndex = gridExpandedRowCountSelector(tableApiRef) - 1;
    const maxColIndex = gridVisibleColumnDefinitionsSelector(tableApiRef).length - 1;
    setCoordinates((coords: any) => {
      switch (position) {
        case 'top':
          return { ...coords, rowIndex: Math.max(0, coords.rowIndex - 1) };
        case 'bottom':
          return { ...coords, rowIndex: Math.min(maxRowIndex, coords.rowIndex + 1) };
        case 'left':
          return { ...coords, colIndex: Math.max(0, coords.colIndex - 1) };
        case 'right':
          return { ...coords, colIndex: Math.min(maxColIndex, coords.colIndex + 1) };

        default:
          return { ...coords, rowIndex: 0, colIndex: 0 };
      }
    });
  };

  return { handleCellNavigation };
}
