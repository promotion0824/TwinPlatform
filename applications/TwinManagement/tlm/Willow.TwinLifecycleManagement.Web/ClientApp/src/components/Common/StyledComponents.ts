import { styled, Typography, DialogContentText, alpha } from '@mui/material';

const StyledHeader = styled(Typography)(() => ({
  margin: '0 0 10px 0',
}));

const StyledDialogCaution = styled(DialogContentText)(({ theme }) => ({
  borderRadius: theme.shape.borderRadius,
  color: alpha('#ffffff', 0.95),
  backgroundColor: alpha('#f4433630', 0.3),
  textDecoration: 'none',
  padding: '6px 10px',
}));

export { StyledHeader, StyledDialogCaution };
