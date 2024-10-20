import {
  GridToolbarContainer,
  GridToolbarColumnsButton,
  GridToolbarFilterButton,
  GridToolbarExport,
} from '@mui/x-data-grid-pro';

import { styled } from '@mui/material';

export function CustomToolBar() {
  return (
    <Flex>
      <StyledToolBarContainer>
        <GridToolbarColumnsButton />
        <GridToolbarFilterButton />
        <GridToolbarExport />
      </StyledToolBarContainer>
    </Flex>
  );
}

const Flex = styled('div')({
  display: 'flex',
  flexDirection: 'row',
  width: '100%',
  justifyContent: 'space-between',
  '&:last-child': { padding: '0 10px' },
});
const StyledToolBarContainer = styled(GridToolbarContainer)({ gap: 10, flexWrap: 'nowrap', padding: '4px 3px' });
