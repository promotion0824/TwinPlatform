import { useMonaco } from '@monaco-editor/react';
import { useEffect } from 'react';
let completionRegistered = false;

export default function useMonacoTextInitializer(editorType: string) {
  const monaco = useMonaco();

  const stringFunctions = [
    {
      label: 'FAULTYTEXT', syntax: '**FAULTYTEXT(some text here)**',
      description: '*Text that displays when Insight is Faulty.*'
    },
    {
      label: 'NONFAULTYTEXT', syntax: '**NONFAULTYTEXT(some text here)**',
      description: '*Text that displays when Insight is not Faulty.*'
    }
  ];

  useEffect(() => {
    if (monaco) {
      monaco.languages.register({ id: `willowLang${editorType}` });

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
            [/,/, 'delimeter'],
            [/\)/, 'delimiter', '@pop'],
          ],

          common: [
            [/([A-Z_][A-Z_0-9]*)(\s*)(\()/, [{ token: 'function', next: '@otherFunction' }, '', 'delimiter']],
            [/(dtmi:com:willowinc:\w+;[0-9])/, 'type.identifier'],
            [/[0-9]+/, 'number'],
            [/\{([^}]+)\}/, 'variable.name']
          ],
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
        brackets: [["(", ")"], ["[", "]"], ["{", "}"]]
      });
      monaco.editor.defineTheme(`willowLang${editorType}`, {
        base: "vs-dark",
        inherit: true,
        rules: [
          {
            token: 'function',
            foreground: '#E1D093'
          },
        ],
        colors: {
          'editor.background': '#00000000',
          'editor.lineHighlightBorder': '#00000000',
          'editor.foreground': '#e2e2e2',
        },
      });
    }
  }, [monaco]);

  useEffect(() => {
    if (monaco && !completionRegistered) {
      const d = monaco.languages.registerCompletionItemProvider(`willowLang${editorType}`, {
        provideCompletionItems: (model, position) => {
          const word = model.getWordUntilPosition(position);

          const range = {
            startLineNumber: position.lineNumber,
            endLineNumber: position.lineNumber,
            startColumn: word.startColumn,
            endColumn: word.endColumn,
          };

          return {
            suggestions: [
              ...stringFunctions.map(item => ({
                label: item.label ?? "",
                kind: monaco.languages.CompletionItemKind.Function,
                insertText: item.label + '(${0})' ?? "",
                insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                range,
              })) ?? []
            ]
          };
        },
      });

      const d2 = monaco.languages.registerHoverProvider(`willowLang${editorType}`, {
        provideHover: (model, position) => {
          const word = model.getWordAtPosition(position);

          if (!word) {
            return null;
          }

          const range = {
            startLineNumber: position.lineNumber,
            endLineNumber: position.lineNumber,
            startColumn: word.startColumn,
            endColumn: word.endColumn,
          };

          const matchingOption = stringFunctions.find(opt => opt.label === word.word);
          if (matchingOption) {
            return {
              range,
              contents: [
                {
                  value: matchingOption?.syntax
                },
                {
                  value: matchingOption?.description
                }
              ]
            };
          }

          return null;
        }
      })
      completionRegistered = true;

      return () => {
        d.dispose();
        d2.dispose();
        completionRegistered = false;
      }
    }
  }, [monaco]);

  return monaco;
}
