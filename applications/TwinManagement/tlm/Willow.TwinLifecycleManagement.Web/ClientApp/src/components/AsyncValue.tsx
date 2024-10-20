import { CircularProgress, SxProps, Theme } from '@mui/material';

/**
 * Display async values. When isLoading is true, display a circular progress indicator.
 */
export function AsyncValue({ value, isLoading, sx }: { value: any; isLoading: boolean; sx?: SxProps<Theme> }) {
  const val = isLoading ? (
    <CircularProgress sx={{ ...sx, height: '15px !important', width: '15px !important' }} />
  ) : (
    <>{value}</>
  );
  return val;
}
