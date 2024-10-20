import {
  useGridApiContext,
  GridFooterContainer,
  useGridRootProps,
  useGridSelector,
  gridTopLevelRowCountSelector,
  selectedGridRowsCountSelector,
  gridFilteredTopLevelRowCountSelector,
  GridSelectedRowCount,
} from '@mui/x-data-grid-pro';

export default function CustomGridFooter({ selectAll, totalCount }: { selectAll: boolean; totalCount: number }) {
  const apiRef = useGridApiContext();
  const rootProps = useGridRootProps();
  const totalTopLevelRowCount = useGridSelector(apiRef, gridTopLevelRowCountSelector);
  const selectedRowCount = useGridSelector(apiRef, selectedGridRowsCountSelector);
  const visibleTopLevelRowCount = useGridSelector(apiRef, gridFilteredTopLevelRowCountSelector);

  const rowCount = selectAll ? totalCount : selectedRowCount;
  const selectedRowCountElement =
    !rootProps.hideFooterSelectedRowCount && rowCount > 0 ? (
      <GridSelectedRowCount selectedRowCount={rowCount} />
    ) : (
      <div />
    );

  const rowCountElement =
    !rootProps.hideFooterRowCount && !rootProps.pagination ? (
      <rootProps.slots.footerRowCount
        {...rootProps.slotProps?.footerRowCount}
        rowCount={totalTopLevelRowCount}
        visibleRowCount={visibleTopLevelRowCount}
      />
    ) : null;

  return (
    <GridFooterContainer>
      {selectedRowCountElement}
      {rowCountElement}
    </GridFooterContainer>
  );
}
