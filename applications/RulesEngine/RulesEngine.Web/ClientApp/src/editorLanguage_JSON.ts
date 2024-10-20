import { useMonaco } from '@monaco-editor/react';
import { useEffect } from 'react';

export default function useMonacoJSONInitializer(editorType: string) {
  const monaco = useMonaco();

  useEffect(() => {
    if (monaco) {
      monaco.languages.register({
        id: `willowLang${editorType}`,
        extensions: ['.json'],
        mimetypes: ['application/json'] });

      monaco.languages.setMonarchTokensProvider(`willowLang${editorType}`, {
        keywords: [
        ],

        typeKeywords: [
        ],

        operators: [
          '=', '>', '<', '!', '~', '?', ':', '==', '<=', '>=', '!=',
          '&&', '||', '++', '--', '+', '-', '*', '/', '&', '|', '^', '%',
          '<<', '>>', '>>>', '+=', '-=', '*=', '/=', '&=', '|=', '^=',
          '%=', '<<=', '>>=', '>>>='
        ],

        symbols: /[=><!~?:&|+\-*\/\^%]+/,

        tokenizer: {
          root: [
            { include: '@common' },
            { include: '@jsonProperties' },

            // delimiters and operators
            [/[{}()\[\]]/, '@brackets'],
            [/[<>](?!@symbols)/, '@brackets'],
            [/@symbols/, {
              cases: {
                '@operators': 'operator',
                '@default': ''
              }
            }],
          ],

          otherFunction: [
            { include: '@common' },
            [/,/, 'delimiter'],
            [/\)/, 'delimiter', '@pop'],
          ],

          common: [
            [/([A-Z_][A-Z_0-9]*)(\s*)(\()/, [{ token: 'function', next: '@otherFunction' }, '', 'delimiter']],
            [/(dtmi:com:willowinc:\w+;[0-9])/, 'type.identifier'],
            [/[0-9]+/, 'number'],
            [/\{([^}]+)\}/, 'variable']
          ],

          jsonProperties: [
            { include: '@common' },
            [/"([^"\\]|\\.)*"\s*:/, 'json.property'], // Match JSON properties
          ]
        },
      });

      monaco.languages.setLanguageConfiguration(`willowLang${editorType}`, {
        surroundingPairs: [
          { open: '{', close: '}' },
          { open: '[', close: ']' },
          { open: '(', close: ')' },
          { open: '<', close: '>' },
          { open: "'", close: "'" },
          { open: '"', close: '"' },
        ],
        autoClosingPairs: [
          { open: '{', close: '}' },
          { open: '[', close: ']' },
          { open: '(', close: ')' },
          { open: "'", close: "'", notIn: ['string', 'comment'] },
          { open: '"', close: '"', notIn: ['string', 'comment'] },
        ],
        brackets: [["(", ")"], ["[", "]"],["{", "}"]]
      });
      monaco.editor.defineTheme(`willowLang${editorType}`, {
        base: "vs-dark",
        inherit: true,
        rules: [
          {
            token: 'function',
            foreground: '#E1D093'
          },
          {
            token: 'json.property',
            foreground: '#97B6CC'
          }
        ],
        colors: {
          'editor.background': '#00000000',
          'editor.lineHighlightBorder': '#00000000',
          'editor.foreground': '#e2e2e2',
        },
      });
    }
  }, [monaco]);

  return monaco;
}
