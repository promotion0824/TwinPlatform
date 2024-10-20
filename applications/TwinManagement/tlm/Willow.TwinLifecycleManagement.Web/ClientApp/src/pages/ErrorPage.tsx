import { Box, Link, Typography } from '@mui/material';
import SmartToyOutlinedIcon from '@mui/icons-material/SmartToyOutlined';

const ErrorPage = () => {
  function getAISessionId() {
    const name = 'ai_session';
    const value = `; ${document.cookie}`;
    const parts: any = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift().split('|')[0];
    else return '';
  }

  return (
    <div style={{ width: '100%' }}>
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'row',
          flexWrap: 'wrap',
          justifyContent: 'center',
          alignContent: 'center',
          gap: 3,
          mt: 5,
        }}
      >
        <SmartToyOutlinedIcon fontSize="large" />
        <Box
          sx={{
            display: 'flex',
            flexDirection: 'column',
            flexWrap: 'wrap',
            justifyContent: 'flex-start',
            alignContent: 'flex-start',
            gap: 3,
            maxWidth: '40%',
          }}
        >
          <Typography variant="h2">An error we did not expect has happened.</Typography>
          <Typography variant="h3">
            It's not you, it's us! Please try closing the tab and opening it again. If the error persists, please reach
            out for help at
            <Link
              href="https://teams.microsoft.com/l/channel/19%3a610f5495553448cda0294e06d01601de%40thread.tacv2/
                            Twin%2520Lifecycle%2520Management?groupId=c6d7c64f-4877-4cd1-8d84-
                            2d3395786a4b&tenantId=d43166d1-c2a1-4f26-a213-f620dba13ab8"
            >
              {' '}
              Twin Lifecycle Management channel.
            </Link>
          </Typography>
          <Typography variant="h3">
            When reaching out, please provide this session ID: <b>{getAISessionId()}</b>
          </Typography>
          <Typography variant="h3"></Typography>
        </Box>
      </Box>
    </div>
  );
};

export default ErrorPage;
