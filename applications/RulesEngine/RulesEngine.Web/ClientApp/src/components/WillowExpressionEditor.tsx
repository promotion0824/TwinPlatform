import { InputBaseComponentProps, TextField } from '@mui/material';
import { Editor, EditorProps } from '@monaco-editor/react';
import useMonacoInitializer from '../editorLanguage_Expressions';
import useMonacoTextInitializer from '../editorLanguage_Text';
import { RuleParameterDto } from '../Rules';
import { FieldErrors } from 'react-hook-form';
import { MutableRefObject, forwardRef, useRef, useState, useEffect } from 'react';

const lineHeight = 19;

type ValueRef = MutableRefObject<string | (readonly string[] & string) | undefined>;

type MonacoInputComponentProps = {
  onEditorBlur: (valueRef: ValueRef) => void,
  editorType: string;
} & InputBaseComponentProps & EditorProps;

const MonacoInputComponent = forwardRef((params: MonacoInputComponentProps, ref) => {

  const [height, setHeight] = useState(lineHeight);
  const valueRef = useRef(params.defaultValue);
  const { editorType } = params;

  useEffect(() => {
    // Manually trigger onDidBlurEditorText when component mounts when starting with null or empty string
    if (ref && valueRef.current === params.defaultValue) {
      params.onEditorBlur(valueRef);
    }
  }, []);

  const handleExpressionChange = (expression: string | undefined) => {
    valueRef.current = expression;
  };

  return (
    <Editor
      onMount={(editor) => {
        editor.onDidBlurEditorText(() => params.onEditorBlur(valueRef));
        editor.onDidContentSizeChange((e) => {
          setHeight(e.contentHeight);
        });
        if (ref) {
          if (typeof (ref) === 'function') {
            ref(editor);
          } else {
            ref.current = editor;
          }
        }
      }}
      height={height}
      language={`willowLang${editorType}`}
      theme={`willowLang${editorType}`}
      value={valueRef.current}
      options={{
        lineNumbers: 'off',
        minimap: {
          enabled: false
        },
        scrollbar: {
          horizontal: "hidden",
          vertical: "hidden",
          alwaysConsumeMouseWheel: false,
        },
        fontSize: 13,
        fontFamily: 'Poppins',
        glyphMargin: false,
        folding: false,
        lineDecorationsWidth: 0,
        lineNumbersMinChars: 0,
        overviewRulerLanes: 0,
        automaticLayout: true,
        wordWrap: 'on',
        wrappingIndent: 'same',
        lineHeight: lineHeight,
        scrollBeyondLastColumn: 0,
        scrollBeyondLastLine: false,
      }}
      onChange={handleExpressionChange}
    />
  );
})

interface EditParametersProps {
  p: RuleParameterDto,
  parameters: RuleParameterDto[],
  label: string,
  onParameterChanged: () => void,
  getFormErrors: () => FieldErrors,
}

export const WillowExpressionEditor = ({ p, parameters, label, onParameterChanged, getFormErrors }: EditParametersProps) => {

  const monaco = useMonacoInitializer('expressionFieldEditor', parameters);
  const errors = getFormErrors;

  const [expressionRef, setExpressionRef] = useState(p.pointExpression || '');

  const handleBlur = (valueRef: ValueRef) => {
    setExpressionRef(valueRef.current!);
  };

  useEffect(() => {
    if (p && p.pointExpression !== expressionRef) {
      p.pointExpression = expressionRef;
      setExpressionRef(expressionRef);
      onParameterChanged();
    }
  }, [expressionRef]);

  if (monaco) {
    return (
      <TextField
        label={label}
        id={p.fieldId}
        autoComplete="off"
        error={!!errors()[p.fieldId!]}
        multiline
        InputProps={{
          inputComponent: MonacoInputComponent,
        }}
        inputProps={{
          defaultValue: expressionRef,
          onEditorBlur: handleBlur,
          editorType: 'expressionFieldEditor'
        }}
        InputLabelProps={{
          shrink: false,
        }}
      />
    )
  }

  return (<TextField label={label} autoComplete='off' defaultValue='Loading...' inputProps={{ sx: { textAlign: 'center' } }} />);
}


interface EditTextProps {
  id: string,
  p: string,
  label: string,
  onTextChanged: (newValue: string) => void,
  getFormErrors: () => FieldErrors,
}

export const WillowTextEditor = ({ id, p, label, onTextChanged, getFormErrors }: EditTextProps) => {

  const monaco = useMonacoTextInitializer('textFieldEditor');
  const errors = getFormErrors;

  const [textRef, setTextRef] = useState(p || '');

  const handleBlur = (valueRef: ValueRef) => {
    setTextRef(valueRef.current!);
  };

  useEffect(() => {
    if (p !== textRef) {
      p = textRef;
      setTextRef(p);
      onTextChanged(p);
    }
  }, [textRef]);

  if (monaco) {
    return (
      <TextField
        label={label}
        id={id}
        autoComplete="off"
        error={!!errors()[id!]}
        multiline
        InputProps={{
          inputComponent: MonacoInputComponent,
        }}
        inputProps={{
          defaultValue: textRef,
          onEditorBlur: handleBlur,
          editorType: 'textFieldEditor'
        }}
        InputLabelProps={{
          shrink: false,
        }}
      />
    )
  }

  return (<TextField label={label} autoComplete='off' defaultValue='Loading...' inputProps={{ sx: { textAlign: 'center' } }} />);
}
