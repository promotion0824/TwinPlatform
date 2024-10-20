import Link from '@mui/material/Link';
import { styled } from '@mui/material/styles'

const StyledAnchor = styled(Link)(({ theme }) => ({
  textDecoration: 'none',
  "&:hover": {
    textDecoration: 'underline',
  },
  color: theme.palette.primary.light
}));

export default (props: any) => <StyledAnchor {...props} />;
