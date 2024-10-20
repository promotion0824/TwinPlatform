import { useCallback } from "react";
import { CopilotMessage } from "./CopilotMessage";
import Box from "@mui/material/Box";
import Avatar from "@mui/material/Avatar";
import Paper from "@mui/material/Paper";
import Typography from "@mui/material/Typography";
import Grid from "@mui/material/Grid";
import ButtonGroup from "@mui/material/ButtonGroup";
import IconButton from "@mui/material/IconButton";
import ContentCopy from '@mui/icons-material/ContentCopy';
import VolumeUp from '@mui/icons-material/VolumeUp';
import { getFormattedTime } from "./CopilotHelper";
import Tooltip from "@mui/material/Tooltip";
import Markdown from "react-markdown";

const CopilotChatMessage = ({ message, textToSpeechHandle }: { message: CopilotMessage, textToSpeechHandle: (textToSpeak: string) => void }) => {

  const copyToClipboard = useCallback((textToCopy: string) => {

    // Write text to the clipboard
    navigator.clipboard.writeText(textToCopy)
      .then(() => {
        console.log('Text copied to clipboard:', textToCopy);
      })
      .catch((error) => {
        console.error('Error copying text to clipboard:', error);
      });
  }, []);

  return (
    <Box
      sx={{
        display: "flex",
        justifyContent: message.isBot ? "flex-start" : "flex-end",
        mb: 2,
      }}
    >
      <Box
        sx={{
          display: "flex",
          flexDirection: message.isBot ? "row" : "row-reverse",
          alignItems: "center",
        }}
      >
        <Tooltip title={message.sender}>
          <Avatar sx={{ backgroundColor: message.isBot ? "primary.main" : "secondary.main", color: "white" }}>
            {message.sender.length > 0 ? message.sender[0] : 'U'}
          </Avatar>
        </Tooltip>
        <Tooltip title={<>{new Date(message.id).toString()}</>}>
          <Paper
            variant="outlined"
            sx={{
              p: 2,
              ml: message.isBot ? 1 : 0,
              mr: message.isBot ? 0 : 1,
              borderColor: message.isBot ? "primary.light" : "secondary.light",
              borderRadius: message.isBot ? "20px 20px 20px 5px" : "20px 20px 5px 20px",
            }}
          >
            <Markdown>
              {message.text}
            </Markdown>
            <Grid container direction="row" justifyContent="space-between">
              <Grid item>
                <Typography fontSize="0.9rem" color="grey" >
                  {getFormattedTime(message.id)}
                </Typography>
              </Grid>
              <Grid item>
                <ButtonGroup variant="text" aria-label="Chat Message Actions">
                  <IconButton title="Copy To Clipboard" onClick={() => copyToClipboard(message.text)}>
                    <ContentCopy sx={{ fontSize: "0.7rem" }} />
                  </IconButton>
                  <IconButton title="Speak" onClick={() => textToSpeechHandle(message.text)}>
                    <VolumeUp sx={{ fontSize: "0.7rem" }} />
                  </IconButton>
                </ButtonGroup>
              </Grid>
            </Grid>
          </Paper>
        </Tooltip>
      </Box>
    </Box>
  );
};


export default CopilotChatMessage;
