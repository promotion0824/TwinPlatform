import { Link } from 'react-router-dom';
import { styled } from '@mui/material/styles'

const StyledLink = styled(Link)(({ theme }) => ({
  textDecoration: 'none',
  "&:hover": {
    textDecoration: 'underline'
  },
  color: theme.palette.primary.light
}));

export default (props: any) => <StyledLink {...props} />;
