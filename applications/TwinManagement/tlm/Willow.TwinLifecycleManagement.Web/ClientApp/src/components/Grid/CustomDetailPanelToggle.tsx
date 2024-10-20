import { isValidElement } from 'react';
import IconButton from '@mui/material/IconButton';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import {
  useGridSelector,
  useGridApiContext,
  gridDetailPanelExpandedRowsContentCacheSelector,
  GridRenderCellParams,
} from '@mui/x-data-grid-pro';

export default function CustomDetailPanelToggle(props: Pick<GridRenderCellParams, 'id' | 'value'>) {
  const { id, value: isExpanded } = props;
  const apiRef = useGridApiContext();

  // To avoid calling ´getDetailPanelContent` all the time, the following selector
  // gives an object with the detail panel content for each row id.
  const contentCache = useGridSelector(apiRef, gridDetailPanelExpandedRowsContentCacheSelector);

  // If the value is not a valid React element, it means that the row has no detail panel.
  const hasDetail = isValidElement(contentCache[id]);

  return (
    <IconButton size="small" tabIndex={-1} disabled={!hasDetail} aria-label={isExpanded ? 'Close' : 'Open'}>
      <ExpandMoreIcon
        sx={{
          transform: `rotateZ(${isExpanded ? 180 : 0}deg)`,
          transition: (theme) =>
            theme.transitions.create('transform', {
              duration: theme.transitions.duration.shortest,
            }),
        }}
        fontSize="inherit"
      />
    </IconButton>
  );
}
