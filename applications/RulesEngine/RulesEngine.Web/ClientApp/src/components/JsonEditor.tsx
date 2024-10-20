import Editor from '@monaco-editor/react';
import { Alert, Button, Card, CardContent, Grid, Snackbar, Typography } from '@mui/material';
import { Suspense, useEffect, useState } from 'react';
import useCopyToClipboard from '../hooks/useCopyToClipboard';
import useMonacoJSONInitializer from '../editorLanguage_JSON';

interface JsonEditorProps {
  input: string | undefined,
  saveJsonObject: (jsonObject: any) => Promise<void>,
  updateJsonObject: (jsonObject: any | undefined) => void;
}

const JsonEditor = (params: { props: JsonEditorProps }) => {
  const { input, saveJsonObject, updateJsonObject } = params.props;

  const editorType = 'jsonEditor';
  const monaco = useMonacoJSONInitializer(editorType);

  const [height, setHeight] = useState('65vh');
  const [editedJson, setEditedJson] = useState(input);
  const [isSaving, setIsSaving] = useState(false);
  const [copyCompleted, setCopyCompleted] = useState(false);
  const [parseSuccess, setParseSuccess] = useState(true);
  const [cancelCompleted, setCancelCompleted] = useState(false);

  const handleCloseAlert = () => {
    setParseSuccess(true);
    setCancelCompleted(false);
    setCopyCompleted(false);
  };

  useEffect(() => {
    setEditedJson(input)
  }, [input]);

  const onSave = async () => {
    try {
      const parsedJson = JSON.parse(editedJson ?? "{}");
      if (typeof parsedJson === 'object' && parsedJson !== null) {
        setIsSaving(true);

        await saveJsonObject(parsedJson);

        setIsSaving(false);
      }
    }
    catch {
      setParseSuccess(false);
    }
  };

  if (!monaco) {
    return (<></>);
  }

  return (
    <>
      <Card variant="outlined" >
        <CardContent>
          <Grid container spacing={2} justifyContent="flex-end" mb={2}>
            <Grid item>
              <Button variant="contained" color="primary" disabled={isSaving} onClick={onSave}>SAVE</Button>
            </Grid>
            <Grid item>
              <Button variant="contained" color="secondary" disabled={editedJson === input} onClick={() => { setEditedJson(input); updateJsonObject(null); setCancelCompleted(true); }}>CANCEL</Button>
            </Grid>
            <Grid item>
              <Button variant="outlined" color="secondary" onClick={() => { useCopyToClipboard(input ?? ""); setCopyCompleted(true); }}>COPY</Button>
            </Grid>
          </Grid>
          <Editor onMount={(editor) => {
            editor.onDidContentSizeChange((e) => setHeight(`${e.contentHeight}px`));
            editor.onDidChangeModelContent((_e) => { console.log('Editor onDidChangeModelContent'); updateJsonObject(editedJson); });
          }}
            height={height} language={`willowLang${editorType}`}
            theme={`willowLang${editorType}`} value={editedJson}
            options={{
              minimap: {
                enabled: false
              },
              scrollbar: {
                horizontal: "hidden",
                vertical: "hidden",
                alwaysConsumeMouseWheel: false,
              },
              lineNumbersMinChars: 0,
              overviewRulerLanes: 0,
              automaticLayout: true,
              wordWrap: 'on',
              wrappingIndent: 'same',
              lineHeight: 20,
              scrollBeyondLastColumn: 0,
              scrollBeyondLastLine: false,
            }}
            onChange={(value) => { setEditedJson(value!); }} 
          />
        </CardContent>
      </Card>
      <Suspense fallback={<div>Loading...</div>}>
        <Snackbar open={!parseSuccess} onClose={handleCloseAlert} autoHideDuration={5000} >
          <Alert onClose={handleCloseAlert} variant="filled" severity="error">
            {!parseSuccess && <Typography variant="body1">Invalid skill JSON</Typography>}
          </Alert>
        </Snackbar>
        <Snackbar open={cancelCompleted} onClose={handleCloseAlert} autoHideDuration={5000} >
          <Alert onClose={handleCloseAlert} variant="filled" severity="warning">
            {cancelCompleted && <Typography variant="body1">Changes reverted</Typography>}
          </Alert>
        </Snackbar>
        <Snackbar open={copyCompleted} onClose={handleCloseAlert} autoHideDuration={5000} >
          <Alert onClose={handleCloseAlert} variant="filled" severity="info">
            {copyCompleted && <Typography variant="body1">Skill JSON copied</Typography>}
          </Alert>
        </Snackbar>
      </Suspense>
    </>
  );
}

export default JsonEditor;
