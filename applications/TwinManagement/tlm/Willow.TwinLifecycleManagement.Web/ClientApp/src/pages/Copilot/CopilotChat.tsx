import AccessTime from "@mui/icons-material/AccessTime";
import Campaign from "@mui/icons-material/Campaign";
import Cancel from "@mui/icons-material/Cancel";
import MicOutlined from "@mui/icons-material/MicOutlined";
import RecordVoiceOver from "@mui/icons-material/RecordVoiceOver";
import RefreshIcon from '@mui/icons-material/Refresh';
import SendIcon from "@mui/icons-material/Send";
import Settings from "@mui/icons-material/Settings";
import StopCircle from "@mui/icons-material/StopCircle";
import VoiceOverOff from "@mui/icons-material/VoiceOverOff";
import VolumeOff from "@mui/icons-material/VolumeOff";
import VolumeUp from "@mui/icons-material/VolumeUp";
import Box from "@mui/material/Box/Box";
import Button from "@mui/material/Button/Button";
import ButtonGroup from "@mui/material/ButtonGroup/ButtonGroup";
import CircularProgress from "@mui/material/CircularProgress/CircularProgress";
import Dialog from "@mui/material/Dialog/Dialog";
import DialogContent from "@mui/material/DialogContent/DialogContent";
import FormControl from "@mui/material/FormControl/FormControl";
import Grid from "@mui/material/Grid/Grid";
import IconButton from "@mui/material/IconButton/IconButton";
import InputAdornment from "@mui/material/InputAdornment/InputAdornment";
import InputLabel from "@mui/material/InputLabel/InputLabel";
import List from "@mui/material/List/List";
import ListItem from "@mui/material/ListItem/ListItem";
import ListItemIcon from "@mui/material/ListItemIcon/ListItemIcon";
import ListItemText from "@mui/material/ListItemText/ListItemText";
import MenuItem from "@mui/material/MenuItem/MenuItem";
import Select from "@mui/material/Select/Select";
import Switch from "@mui/material/Switch/Switch";
import TextField from "@mui/material/TextField/TextField";
import { AudioConfig, PropertyId, Recognizer, ResultReason, SpeakerAudioDestination, SpeechConfig, SpeechRecognitionEventArgs, SpeechRecognizer, SpeechSynthesizer } from "microsoft-cognitiveservices-speech-sdk";
import { Fragment, useEffect, useMemo, useRef, useState } from "react";
import { HotKeys } from "react-hotkeys";
import { useQuery, useQueryClient } from "react-query";
import { AppPermissions } from "../../AppPermissions";
import { AuthHandler } from "../../components/AuthHandler";
import { StyledHeader } from "../../components/Common/StyledComponents";
import useApi from "../../hooks/useApi";
import { CopilotChatRequest, CopilotChatRequestOption, CopilotContext, ICopilotChatRequest, SpeechAuthorizationTokenResponse } from "../../services/Clients";
import './CopilotChat.css';
import CopilotChatMessage from "./CopilotChatMessage";
import { fuzzyMatch } from "./CopilotHelper";
import { CopilotMessage } from "./CopilotMessage";
import useStateRef from "./hooks/useStateRef";
import { useMsal, useAccount } from '@azure/msal-react';
import startListeningAudio from './start-listening-audio.mp3';

enum CopilotWakeUpMode {
  On = 'On',
  Off = 'Off',
  Prefix = 'Prefix'
}

const CopilotChat = () => {
  const api = useApi();
  const [input, setInput] = useState("");
  const [sessionId, setSessionId] = useState<number>(new Date().getTime());
  const [inputError, setInputError] = useState<boolean>(false);
  const [chatMessages, setChatMessages] = useState<CopilotMessage[]>([]);
  const { accounts } = useMsal();
  const account = useAccount(accounts[0]);
  const [queryKey, setQueryKey] = useState<CopilotMessage | undefined>();
  const client = useQueryClient();
  const [speechCred, setSpeechCred] = useState<SpeechAuthorizationTokenResponse | undefined>(undefined);
  const [listeningCommand, setListeningCommand] = useState(false);
  const [isPlaying, setIsPlaying] = useState(false);
  const [enableVoice, setEnableVoice] = useState(false);
  const [wakeUpMode, setWakeUpMode, wakeUpModeRef] = useStateRef<CopilotWakeUpMode>(CopilotWakeUpMode.Off);
  const [wakeUpKeyword, setWakeUpKeyword, wakeUpKeywordRef] = useStateRef('Hey Willow');
  const [settingOpen, setSettingOpen] = useState(false);
  const [silenceTimeout, setSilenceTimeout] = useState(1000);
  const [audioDestination, setAudioDestination] = useState<SpeakerAudioDestination>();
  const boxRef = useRef<HTMLDivElement>(null);
  const textBoxRef = useRef<HTMLDivElement>(null);
  const { isFetching } = useQuery(['copilotChat', queryKey],
    async (k) => {

      const userMessage = k.queryKey[1] as CopilotMessage;
      if (!userMessage)
        return null;

      const context: CopilotContext = new CopilotContext({
        sessionId: sessionId.toString(),
        userName: account?.username || 'Unknown User',
      });

      const chatRequestOption = new CopilotChatRequestOption();
      chatRequestOption.modelHint = getParamFromUrl('modelhint');
      chatRequestOption.promptHints = getParamsFromUrl('prompthint');
      chatRequestOption.runFlags = getParamsFromUrl('runflag');
      const chatRequest: ICopilotChatRequest = {
        userInput: userMessage.text,
        context: context,
        options: chatRequestOption,
      };
      const start = performance.now();
      let botResponse: CopilotMessage = new CopilotMessage("No response", "", true);
      try {
        let response = await api.chat(new CopilotChatRequest(chatRequest));
        const end = performance.now();
        console.log(`Total Execution time: ${end - start}`);
        // if request is sucessful - add it to the chat window
        botResponse = new CopilotMessage(response.responseText!, "", true);

        // Check if voice is enabled and read the response loud
        if (enableVoice) {
          handleTextToSpeech(botResponse.text);
        }

      } catch (e) {
        botResponse = new CopilotMessage("Copilot encountered an unknown error while generating your response. Please try again.", "", true);
      }
      setChatMessages([...chatMessages, botResponse]);
      return botResponse;
    });
  const isFetchingRef = useRef(false);
  useEffect(() => {
    isFetchingRef.current = isFetching;
  }, [isFetching]);
  const audioDestinationRef = useRef<SpeakerAudioDestination>();
  useEffect(() => {
    audioDestinationRef.current = audioDestination;
  }, [audioDestination]);
  useEffect(() => {
    // Scroll to the bottom when messages change
    if (!boxRef.current) return;
    boxRef.current.scrollTop = boxRef.current.scrollHeight;
  }, [chatMessages]);

  useEffect(() => {
    try {
      // Disable speech service telemetry
      SpeechRecognizer.enableTelemetry(false);

      const cacheSpeechToken = () => {
        api.speechToken().then((res) => {
          setSpeechCred(res);
        });
      };
      const intervId = setInterval(cacheSpeechToken, 10 * 60 * 1000);
      cacheSpeechToken();
      return () => {
        clearInterval(intervId);
        stopKeywordListening();
        pauseAndCloseAudioPlayer();
      };
    } catch (e) {
      console.error(e);
    }

  }, []);

  const recognizer = useMemo(() => {
    if (!speechCred || !speechCred.token)
      return null;
    const speechConfig = SpeechConfig.fromAuthorizationToken(speechCred.token!, speechCred.region!);
    speechConfig.speechRecognitionLanguage = "en-US";
    speechConfig.setProperty(PropertyId.Speech_SegmentationSilenceTimeoutMs, (silenceTimeout > 5000 ? 5000 : silenceTimeout).toString());
    const audioConfig = AudioConfig.fromDefaultMicrophoneInput();
    const recognizer = new SpeechRecognizer(speechConfig, audioConfig);
    return recognizer;
  }, [speechCred, silenceTimeout]);
  const recognizerRef = useRef<SpeechRecognizer>();
  useEffect(() => {
    if (!!recognizer) {
      recognizerRef.current = recognizer;
    }
  }, [recognizer]);

  const pauseAndCloseAudioPlayer = () => {
    if (!!audioDestinationRef.current) {
      audioDestinationRef.current.pause();
      audioDestinationRef.current.close();
      setIsPlaying(false);
    }
  };

  const handleTextToSpeech = (textToSpeak: string) => {
    if (!speechCred) {
      console.error("Unable to get speech service token. Text to speech is not available at this time.");
      return;
    }
    pauseAndCloseAudioPlayer();

    const speechConfig = SpeechConfig.fromAuthorizationToken(speechCred.token!, speechCred.region!);
    speechConfig.speechSynthesisVoiceName = "en-US-AndrewMultilingualNeural";
    const audioDestination = new SpeakerAudioDestination();
    const synthesizer = new SpeechSynthesizer(speechConfig, AudioConfig.fromSpeakerOutput(audioDestination));
    setAudioDestination(audioDestination);
    // Event listener for synthesis completed
    audioDestination.onAudioEnd = (s) => {
      setIsPlaying(false);
    };
    audioDestination.onAudioStart = (s) => {
      setIsPlaying(true);
    };

    // Start synthesis
    const regExpForSpeak = /<<<.*?>>>/g;
    let filteredTextToSpeak = textToSpeak.replaceAll(regExpForSpeak, '');
    synthesizer.speakTextAsync(filteredTextToSpeak,
      (res) => {
        synthesizer.close();
      }, () => {
        synthesizer.close();
      });

    if (!textBoxRef || !textBoxRef.current)
      return;
    textBoxRef.current.focus();
  };

  const startListening = () => {
    if (wakeUpModeRef.current === CopilotWakeUpMode.Off) {
      startRecognition(wakeUpMode);
    }
    else {
      setListeningCommand((prev) => true);
      playListeningAudio();
    }
  }

  const stopListening = () => {
    setListeningCommand((prev) => false);
    if (!!recognizerRef.current && wakeUpModeRef.current === CopilotWakeUpMode.Off) {
      recognizerRef.current.stopContinuousRecognitionAsync();
    }
  }

  const startRecognition = (mode: CopilotWakeUpMode) => {
    if (!recognizer) {
      return;
    }

    recognizer.startContinuousRecognitionAsync();
    recognizer.recognizing = (sender, event) => {
      let isListening = false;
      setListeningCommand((prev) => {
        isListening = prev;
        return prev;
      })
      if (isListening && !isFetchingRef.current) {
        setInput(event.result.text);
      }
      else if (!isListening &&
        wakeUpModeRef.current === CopilotWakeUpMode.Prefix &&
        fuzzyMatch(wakeUpKeywordRef.current, event.result.text, 3)) {
        startListening();
      }
    };
    recognizer.recognized = onRecognizedSpeech;
    recognizer.canceled = (s, e) => {
      stopKeywordListening();
      setWakeUpMode(CopilotWakeUpMode.Off);
      console.log(`CANCELED: Reason=${e.reason}`);
    }

    recognizer.sessionStarted = (rec, sessionEvt) => {
      console.log("\n Session started event.");
      if (mode === CopilotWakeUpMode.Off) {
        setListeningCommand((prev) => true);
        playListeningAudio();
      }
    };
    recognizer.sessionStopped = (rec, sessionEvt) => {
      console.log("\n Session stopped event.");
    };

  }

  const onRecognizedSpeech = (sender: Recognizer, e: SpeechRecognitionEventArgs) => {
    let command = e.result.text;
    let isListening = false;
    setListeningCommand((prev) => {
      isListening = prev;
      return prev;
    })

    console.log(`Listening:${isListening} Recognized:${command}`);
    if (e.result.reason === ResultReason.RecognizedSpeech) {
      if (isListening && !isFetching) {
        stopListening();
        if (wakeUpModeRef.current === CopilotWakeUpMode.Prefix) {
          command = command.trim().split(' ').slice(wakeUpKeywordRef.current.split(' ').length).join(' ');
        }
        if (!!command) {
          AskCopilot(command);
        }
      } else {
        const isKeywordMatch = fuzzyMatch(wakeUpKeywordRef.current, command, 3);
        if (isKeywordMatch) {
          // start listening
          startListening();
        }
      }
      // continue listening for the keyword
    }
    else if (wakeUpModeRef.current !== CopilotWakeUpMode.Off &&
      isListening &&
      (e.result.reason === ResultReason.NoMatch ||
        e.result.reason === ResultReason.Canceled)) {
      stopListening();
    }
  }

  const wakeModeOnChange = (changedValue: string) => {
    const enumMode = Object.values(CopilotWakeUpMode).find(value => value === changedValue)!;
    setWakeUpMode(enumMode);
    if (enumMode === CopilotWakeUpMode.Off) {
      stopKeywordListening();
    } else {
      startKeywordListening(enumMode);
    }
  };

  const startKeywordListening = (mode: CopilotWakeUpMode) => {
    pauseAndCloseAudioPlayer();
    if (!recognizer) {
      return;
    }
    startRecognition(mode);
  };

  const stopKeywordListening = () => {
    stopListening();
    if (!!recognizerRef.current) {
      recognizerRef.current.stopContinuousRecognitionAsync();
    }
  }

  const AskCopilot = (question: string) => {
    if (isFetchingRef.current) {
      return;
    }
    // Clear input for processing
    setInput('');
    const currUserMessage: CopilotMessage = new CopilotMessage(question, account?.name, false);
    setChatMessages((prev) => [...prev, currUserMessage]);
    // Set the query key
    setQueryKey(currUserMessage);
  };

  const handleSend = () => {
    if (!input)
      return;
    if (input.trim() !== "") {
      setInputError(false);
      AskCopilot(input);
    } else {
      // set error if input is empty
      setInputError(true);
    }
  };

  const handleEnterKeyPress = (event: any) => {
    if (isFetching)
      return;

    // Send message on enter key and ignore if other keys is being pressed.
    if (event.key === 'Enter' && !(event.shiftKey || event.ctrlKey || event.altKey || event.metaKey)) {
      handleSend();
    }
  };

  const clearSession = () => {
    setSessionId(new Date().getTime());
    setChatMessages([]);
    setQueryKey(undefined);
  };

  const getParamsFromUrl = (paramName: string) => {
    const params = new URLSearchParams(window.location.search);
    return params.getAll(paramName) || [];
  };
  const getParamFromUrl = (paramName: string) => {
    const params = new URLSearchParams(window.location.search);
    return params.get(paramName) || '';
  };

  const playListeningAudio = () => {
    const startLisAud = new Audio(startListeningAudio);
    startLisAud.play();
  }

  return (
    <AuthHandler requiredPermissions={[AppPermissions.CanChatWithCopilot]} noAccessAlert>
      <HotKeys handlers={{ 'listen': (listeningCommand ? stopListening : startListening), 'enter': handleEnterKeyPress }} >
        <Box
          sx={{
            height: "80vh",
            display: "flex",
            flexDirection: "column",
          }}
        >
          <Grid container spacing={2}>
            <Grid item xs={8}>
              <StyledHeader variant="h1">Copilot</StyledHeader>
            </Grid>
            <Grid item xs={4}>
              <Box display="flex" justifyContent="flex-end">
                <ButtonGroup variant="contained" aria-label="Session button group">
                  <Button sx={{ minWidth: '10em' }} size="small" variant="outlined" color="secondary">Session Id:</Button>
                  <Button size="small" variant="outlined" color="secondary" >{sessionId}</Button>
                  <Button size="small" disabled={isFetching} onClick={() => clearSession()} endIcon={<RefreshIcon />}>Reset</Button>
                </ButtonGroup>
                <Fragment>
                  <IconButton onClick={() => setSettingOpen(true)}>
                    <Settings titleAccess="Copilot Settings" />
                  </IconButton>
                  <Dialog
                    fullWidth
                    open={settingOpen}
                    onClose={() => setSettingOpen(false)}>
                    <DialogContent>

                      <List sx={{ width: "100%", bgcolor: 'background.paper' }}>
                        <ListItem>
                          <ListItemIcon>
                            {enableVoice ?
                              <VolumeUp titleAccess="Read Response Enabled" />
                              :
                              <VolumeOff titleAccess="Read Response Disabled" />
                            }
                          </ListItemIcon>
                          <ListItemText id="switch-list-label-read-response" primary={enableVoice ? "Read Response Enabled" : "Read Response Disabled"} />
                          <Switch
                            edge="end"
                            checked={enableVoice}
                            onChange={() => setEnableVoice(!enableVoice)}
                            inputProps={{
                              'aria-labelledby': 'switch-list-label-read-response',
                            }}
                          />
                        </ListItem>
                        <ListItem>
                          <ListItemIcon>{
                            (wakeUpMode === CopilotWakeUpMode.On || wakeUpMode === CopilotWakeUpMode.Prefix) ?
                              <RecordVoiceOver titleAccess="Wake Up Mode On|Prefix" /> :
                              <VoiceOverOff titleAccess="Wake Up Mode Off" />
                          }
                          </ListItemIcon>
                          <ListItemText id="switch-list-label-wake-up-mode" primary="Wake Up Mode" />
                          <FormControl>
                            <InputLabel id="wakeup-model-label">Mode</InputLabel>
                            <Select sx={{ minWidth: '6em' }}
                              labelId="wakeup-model-label"
                              id="wake-up-mode-select"
                              value={wakeUpMode}
                              label="WakeUpMode"
                              onChange={(event) => wakeModeOnChange(event.target.value as string)}
                            >
                              <MenuItem value={CopilotWakeUpMode.On}>On</MenuItem>
                              <MenuItem value={CopilotWakeUpMode.Off}>Off</MenuItem>
                              <MenuItem value={CopilotWakeUpMode.Prefix}>Prefix</MenuItem>
                            </Select>
                          </FormControl>
                        </ListItem>
                        {(wakeUpMode === 'On' || wakeUpMode === 'Prefix') &&
                          <ListItem>
                            <ListItemIcon>
                              <Campaign titleAccess="Wake Up Keyword" />
                            </ListItemIcon>
                            <ListItemText id="switch-list-label-wake-up-keyword" primary="Wake Up keyword" />
                            <TextField title="wake-up-keyword" value={wakeUpKeyword} onChange={(evt) => setWakeUpKeyword(evt.target.value)} />
                          </ListItem>
                        }
                        <ListItem>
                          <ListItemIcon>
                            <AccessTime titleAccess="Wake Up Keyword" />
                          </ListItemIcon>
                          <ListItemText id="silence-timeout-label" primary="Silence Timeout (ms)" />
                          <TextField title="silence-timeout" type="number" value={silenceTimeout} onChange={(evt) => {
                            recognizer?.close();
                            setWakeUpMode(CopilotWakeUpMode.Off);
                            setSilenceTimeout(Number(evt.target.value));
                          }}
                            inputProps={{
                              min: 100,
                              max: 5000,
                            }}
                          />
                        </ListItem>
                      </List>
                    </DialogContent>
                  </Dialog>
                </Fragment >
              </Box>
            </Grid>
          </Grid>
          <Box ref={boxRef} border={2} borderColor="primary.main" padding={1} sx={{ flexGrow: 1, overflow: "auto" }}>
            {chatMessages.map((message) => (
              <CopilotChatMessage key={message.id} message={message} textToSpeechHandle={handleTextToSpeech} />
            ))}
            {isFetching &&
              <Box sx={{ display: 'flex' }}>
                <CircularProgress />
                <Box sx={{ padding: '10px' }}>
                  Asking Copilot...
                </Box>
              </Box>}
          </Box>
          <Box border={1} borderColor="primary.main">
            <TextField ref={textBoxRef}
              autoFocus
              size="medium"
              disabled={isFetching}
              fullWidth
              placeholder="Type a message and press enter to send"
              variant="outlined"
              value={input}
              error={inputError}
              helperText={inputError ? "Please type a message" : ""}
              onChange={(e) => setInput(e.target.value)}
              onKeyDown={handleEnterKeyPress}
              multiline
              inputProps={{
                maxLength: 8192,
              }}
              InputProps={{
                startAdornment:
                  <InputAdornment position="start">
                    {(!isPlaying && !isFetching) && (<IconButton className={listeningCommand ? 'listening' : ''} title={listeningCommand ? "Listening..." : "Click to Listen or Ctrl+Shift+: to start listening"} disabled={isFetching || isPlaying} onClick={listeningCommand ? stopListening : startListening}>
                      <MicOutlined />
                    </IconButton>
                    )}

                    {isPlaying && (<IconButton onClick={pauseAndCloseAudioPlayer}>
                      <StopCircle />
                    </IconButton>)}
                  </InputAdornment>,
                endAdornment:
                  isFetching
                    ?
                    <InputAdornment position="end">
                      <IconButton title="Cancel" onClick={() => client.cancelQueries(['copilotChat', queryKey])}>
                        <Cancel />
                      </IconButton>
                    </InputAdornment>
                    :
                    <InputAdornment position="end">
                      <IconButton title="Submit" onClick={handleSend}>
                        <SendIcon />
                      </IconButton>
                    </InputAdornment>
              }}
            />

          </Box>
        </Box>
      </HotKeys>
    </AuthHandler>
  );
};


export default CopilotChat;
