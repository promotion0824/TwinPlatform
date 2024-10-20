import { useMonaco } from '@monaco-editor/react';
import { useQuery } from 'react-query';
import { useEffect } from 'react';
import useApi from './hooks/useApi';
import { BatchDtoModelSimpleGraphDto, BatchRequestDto, GlobalVariableDtoBatchDto, MLModelDtoBatchDto, RuleParameterDto } from './Rules';
import env from './services/EnvService';

let completionRegistered = false;

export default function useMonacoInitializer(editorType: string, parameters: RuleParameterDto[]) {
  const monaco = useMonaco();
  const apiclient = useApi();
  const baseurl = env.baseurl();

  const defaultFunctions = [
    {
      label: 'ABS', syntax: '**ABS(value)**', returnType: '`double`', defaults: '',
      description: '*Calculates the absolute value of a number.*'
    },
    {
      label: 'ACOS', syntax: '**ACOS(value)**', returnType: '`double`', defaults: '',
      description: '*Calculates the arc cosine (in radians) of a number.*'
    },
    {
      label: 'ALL', syntax: '**ALL(arg1, arg2, ...)** *OR* **ALL(arg1, arg2, ..., timePeriod)**', returnType: '`boolean`', defaults: '',
      description: '*Check an enumerable of bool from multiple capabilities*'
    },
    {
      label: 'ANY', syntax: '**ANY([model] operator condition)** *OR* **ANY([model] operator condition, timePeriod)**', returnType: '', defaults: '',
      description: '*Apply to models that may reference multiple instances and allow skills to handle unexpected or expected arrays, e.g. ChillersOperational = ANY([ChwValvePosition] > 0)*'
    },
    {
      label: 'ASIN', syntax: '**ASIN(value)**', returnType: '`double`', defaults: '',
      description: '*Calculates the arc sine (in radians) of a number.*'
    },
    {
      label: 'ATAN', syntax: '**ATAN(value)**', returnType: '`double`', defaults: '',
      description: '*Calculates the arc tangent (in radians) of a number.*'
    },
    {
      label: 'ATAN2', syntax: '**ATAN2(y, x)**', returnType: '`double`', defaults: '',
      description: '*Calculates the angle (in radians) from the X axis to a point represented by the coordinates (x, y).*'
    },
    {
      label: 'AVERAGE', syntax: '**AVERAGE(arg1, arg2, ...)** *OR* **AVERAGE(arg1, arg2, ..., timePeriod)**', returnType: '', defaults: '',
      description: '*Aggregate values from multiple capabilities, e.g. AVERAGE([AHU].ratedCFM) calculates the average ratedCFM across a range of air handlers.*'
    },
    {
      label: 'CEILING', syntax: '**CEILING(value)**', returnType: '`integer`', defaults: '',
      description: '*Rounds a number up to the nearest integer.*'
    },
    {
      label: 'CELSIUS', syntax: '**CELSIUS(capability)**', returnType: '', defaults: '',
      description: '*If the capability\'s unit is celsius then it is used, otherwise it is converted to celsius.*'
    },
    {
      label: 'CONTAINS', syntax: '**CONTAINS(arg, filter_value)**', returnType: '', defaults: '',
      description: '*Operates on twin properties - Include if the argument contains the filter_value.*'
    },
    {
      label: '!CONTAINS', syntax: '**!CONTAINS(arg, filter_value)**', returnType: '', defaults: '',
      description: '*Operates on twin properties - Exclude if the argument contains the filter_value.*'
    },
    {
      label: 'COS', syntax: '**COS(value)**', returnType: '`double`', defaults: '',
      description: '*Calculates the cosine of an angle (in radians).*'
    },
    {
      label: 'COUNT', syntax: '**COUNT(arg1, arg2, ...)**', returnType: '', defaults: '',
      description: '*Aggregate values from multiple capabilities, e.g. COUNT([ChwValvePosition] > 0) returns the number of open valves.*'
    },
    {
      label: 'COUNT_BINDINGS', syntax: '**COUNT_BINDINGS([model])**', returnType: '`integer`', defaults: '',
      description: '*Returns the total number of matches for the given expression. e.g. COUNT_BINDINGS([ZoneAirHeatingTemperatureSetpoint;1])*'
    },
    {
      label: 'COUNTLEADING', syntax: '**COUNTLEADING(arg1, arg2, ..., timePeriod)**', returnType: '', defaults: '',
      description: '*Counts leading edge for a given timeperiod, e.g. COUNTLEADING([ChwValvePosition], 5d).*'
    },
    {
      label: 'DAYOFWEEK', syntax: '**DAYOFWEEK(NOW)**', returnType: '', defaults: '(NOW)',
      description: '*Gets the current hour in local time as a value between 1 (Monday) and 7 (Sunday)*'
    },
    {
      label: 'DELTA', syntax: '**DELTA(arg1, arg2, ..., timePeriod)**', returnType: '', defaults: '',
      description: '*Delta of an enumerable of a token expression for checking an enumerable of bool.*'
    },
    {
      label: 'DELTA_TIME', syntax: '**DELTA_TIME(arg, timeUnit)**', returnType: '`double`', defaults: '',
      description: '*Delta Time of an enumerable of a token expression for checking an enumerable of bool (days, hours, minutes, seconds).*'
    },
    {
      label: 'DAY', syntax: '**DAY(NOW)**', returnType: '', defaults: '(NOW)',
      description: '*Gets the current day in local time as a value between 1 and 31*'
    },
    {
      label: 'EACH', syntax: '**EACH([modelType], identifier, calculation)**', returnType: '', defaults: '',
      description: '*Performs a calculation on multiple instances of a model within a given context. Arguments: the model type to iterate over, an identifier for the current instance of the model type, and the calculation to perform on each instance.*'
    },
    {
      label: 'ENDSWITH', syntax: '**ENDSWITH(arg, filter_value)**', returnType: '', defaults: '',
      description: '*Operates on twin properties - Include if the argument ends with the filter_value.*'
    },
    {
      label: '!ENDSWITH', syntax: '**!ENDSWITH(arg, filter_value)**', returnType: '', defaults: '',
      description: '*Operates on twin properties - Exclude if the argument ends with the filter_value.*'
    },
    {
      label: 'EXISTS', syntax: '**EXISTS(arg1, arg2, ...)**', returnType: '', defaults: '',
      description: '*Returns true if all argument(s) bind successfully else False if any one argument fails*\n\n*'
    },
    {
      label: 'FAHRENHEIT', syntax: '**FAHRENHEIT(capability)**', returnType: '', defaults: '',
      description: '*If the capability\'s unit is fahrenheit then it is used, otherwise it is converted to fahrenheit.*'
    },
    {
      label: 'FINDALL', syntax: '**FINDALL(expr)** *OR* **FINDALL(twin, expr)**', returnType: '', defaults: '',
      description: '*Does an ADT query to find twins for a certain model type or under a certain location combined with optional twin property filters. Examples are FINDALL([modelid;1] & UNDER([twinid]) or FINDALL(twin, [modelid;1] & UNDER([twinid] & twin.prop == x)*'
    },
    {
      label: 'FIRST', syntax: '**FIRST(array)**', returnType: '``', defaults: '',
      description: '*Returns the first item from an array expression.*'
    },
    {
      label: 'FLOOR', syntax: '**FLOOR(value)**', returnType: '`integer`', defaults: '',
      description: '*Rounds a number down to the nearest integer.*'
    },
    {
      label: 'FORECAST', syntax: '**FORECAST(arg1, arg2, ...)** *OR* **FORECAST(arg1, arg2, ..., timePeriod)**', returnType: '', defaults: '',
      description: '*Forecast of an Enumerable of a token expression.*'
    },
    {
      label: 'HOUR', syntax: '**HOUR(NOW)**', returnType: '', defaults: '(NOW)',
      description: '*Gets the current hour in local time as a value between 0 and 23*'
    },
    {
      label: 'IF', syntax: '**IF(condition, value_true, value_false)**', returnType: '', defaults: '',
      description: '*If condition is satisfied, use value_true, else use value_false. Everything inside it must be a valid time series or constant value.*'
    },
    {
      label: 'IFNAN', syntax: '**IFNAN(value, alt_value)**', returnType: '', defaults: '',
      description: '*If the value is not NAN, return the value else return the alt_value.*'
    },
    {
      label: 'ISNAN', syntax: '**ISNAN(value)**', returnType: '', defaults: '',
      description: '*If the value is NAN, returns true else false.*'
    },
    {
      label: 'LOG', syntax: '**LOG(value)**', returnType: '`double`', defaults: '',
      description: '*Calculates the natural logarithm (base e) of a number.*'
    },
    {
      label: 'LOG10', syntax: '**LOG10(value)**', returnType: '`double`', defaults: '',
      description: '*Calculates the base-10 logarithm of a number.*'
    },
    {
      label: 'MAX', syntax: '**MAX(arg1, arg2, ...)** *OR* **MAX(arg1, arg2, ..., timePeriod)**', returnType: '', defaults: '',
      description: '*Aggregate values from multiple capabilities and returns the maximum value.*'
    },
    {
      label: 'MIN', syntax: '**MIN(arg1, arg2, ...)** *OR* **MIN(arg1, arg2, ..., timePeriod)**', returnType: '', defaults: '',
      description: '*Aggregate values from multiple capabilities and returns the minimum value.*'
    },
    {
      label: 'MINUTE', syntax: '**MINUTE(NOW)**', returnType: '', defaults: '(NOW)',
      description: '*Gets the current minute in local time as a value between 0 and 59*'
    },
    {
      label: 'MONTH', syntax: '**MONTH(NOW)**', returnType: '', defaults: '(NOW)',
      description: '*Gets the current hour in local time as a value between 1 and 12*'
    },
    {
      label: 'OPTION', syntax: '**OPTION(arg1, arg2, ...)**', returnType: '', defaults: '',
      description: '*Matches the first argument that can be bound to the twin*\n\n ***OR*** *Lookup a twin property with a fallback to a constant value, e.g. OPTION([AHU].ratedCFM, 900)*\n\n ***OR*** *Bind the most specific model type and to fall back to a more general model type if not found, e.g. OPTION[ZoneAirHeatingTemperatureSetpoint;1], [ZoneAirTemperatureSetpoint;1], [AirTemperatureSetpoint;1])*'
    },
    {
      label: 'POW', syntax: '**POW(base, exponent)**', returnType: '`double`', defaults: '',
      description: '*Calculates the result of raising a number to a specified power.*'
    },
    {
      label: 'PREDICT', syntax: '**PREDICT(modelName(string), modelVersion(number), inputs[]...(numbers))**', returnType: '`double`', defaults: '',
      description: '*Predicts a value for the given model name and model version.*'
    },
    {
      label: 'RND', syntax: '**RND(min, max)**', returnType: '`integer[]`', defaults: '',
      description: '*Generates a random number between the given min and max.*'
    },
    {
      label: 'ROUND', syntax: '**ROUND(value)**', returnType: '`integer`', defaults: '',
      description: '*Rounds a number to the nearest integer.*'
    },
    {
      label: 'SIGN', syntax: '**SIGN(value)**', returnType: '`integer`', defaults: '',
      description: '*Returns the sign of a number (1 for positive numbers, -1 for negative numbers, and 0 for zero).*'
    },
    {
      label: 'SIN', syntax: '**SIN(value)**', returnType: '`double`', defaults: '',
      description: '*Calculates the sine of an angle (in radians).*'
    },
    {
      label: 'SLOPE', syntax: '**SLOPE(arg1, arg2, ...)** *OR* **SLOPE(arg1, arg2, ..., timePeriod)**', returnType: '', defaults: '',
      description: '*Slope of an Enumerable of a token expression.*'
    },
    {
      label: 'SQRT', syntax: '**SQRT(value)**', returnType: '`double`', defaults: '',
      description: '*Calculates the square root of a number.*'
    },
    {
      label: 'STARTSWITH', syntax: '**STARTSWITH(arg, filter_value)**', returnType: '', defaults: '',
      description: '*Operates on twin properties - Include if the argument starts with the filter_value.*'
    },
    {
      label: '!STARTSWITH', syntax: '**!STARTSWITH(arg, filter_value)**', returnType: '', defaults: '',
      description: '*Operates on twin properties - Exclude if the argument starts with the filter_value.*'
    },
    {
      label: 'STND', syntax: '**STND(arg1, arg2, ...)** *OR* **STND(arg1, arg2, ..., timePeriod)**', returnType: '', defaults: '',
      description: '*Standard Deviation of an Enumerable of a token expression for checking an enumerable of double.*'
    },
    {
      label: 'SUM', syntax: '**SUM(arg1, arg2, ...)**', returnType: '', defaults: '',
      description: '*Aggregate values from multiple capabilities, e.g. SUM([Chiller].[CapacitySensor]) will return the total value of the bound chillers and capacity sensor values.*'
    },
    {
      label: 'TAN', syntax: '**TAN(value)**', returnType: '`double`', defaults: '',
      description: '*Calculates the tangent of an angle (in radians).*'
    },
    {
      label: 'TIMER', syntax: '**TIMER(condition)** *OR* **TIMER(condition, timeUnit)**', returnType: '`double`', defaults: '',
      description: '*Track the duration of a condition and reset on change. Used for fault detection and diagnostics, to quantify the time span of system anomalies or operational deviations. The TIMER function translate to `IF(condition, exprId + DELTA_TIME(condition), 0)`*'
    },
    {
      label: 'TOLERANCE', syntax: '**TOLERANCE(expr, tolerance[0-1])**', returnType: '', defaults: '',
      description: '*Applies tolerance to capability validation for a given expression (expr). The tolerance must be between 0 (lenient) and 1 (strict)*'
    },
    {
      label: 'TOLERANTOPTION', syntax: '**TOLERANTOPTION(arg1, arg1, ...)**', returnType: '', defaults: '',
      description: '*If data quality flags a capability used within an option clause, default to the next option argument if possible*'
    },
    {
      label: 'TUPLE', syntax: '**TUPLE(arg1, arg2...)**', returnType: '', defaults: '',
      description: '*A Tuple is a tuple of TokenExpressions grouped together as one unit e.g. Tuple (var:Name, var:Height, var:Width)*'
    },
    {
      label: 'UNDER', syntax: '**UNDER(expr)**', returnType: '', defaults: '',
      description: '*Only applies to the FINDALL method to query twins under a location of another twin, eg UNDER([twinid]) or UNDER(this))*'
    }
  ]

  const defaultVariables = [
    'AREA_INCREMENTAL',
    'AREA_OUTSIDE',
    'DELTA_TIME_S',
    'IS_FAULTY',
    'NOW',
    'TOTAL',
    'TIME',
    'TIME_PERCENTAGE',
    'AND',
    'OR',
    'LAST_TRIGGER_TIME'
  ]

  const fetchModels = async () => {
    try {
      return await apiclient.models();
    }
    catch {
      return new BatchDtoModelSimpleGraphDto();
    }
  }

  const fetchGlobals = async () => {
    const request = new BatchRequestDto();
    try {
      return await apiclient.globals(request);
    }
    catch {
      return new GlobalVariableDtoBatchDto();
    }
  }

  const fetchMLModels = async () => {
    const request = new BatchRequestDto();
    try {
      return await apiclient.mLModels(request);
    }
    catch {
      return new MLModelDtoBatchDto();
    }
  }

  const {
    data, isFetched
  } = useQuery(['lookupdata'], async () => {
    const models = await fetchModels();
    const globals = await fetchGlobals();
    const mlModels = await fetchMLModels();
    return {
      models: models,
      globals: globals,
      mlModels: mlModels
    }
  }, { keepPreviousData: true })

  let propMode = false;

  useEffect(() => {
    if (monaco) {
      monaco.languages.register({ id: `willowLang${editorType}` });

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
        brackets: [["(", ")"], ["[", "]"]]
      });

      monaco.editor.defineTheme(`willowLang${editorType}`, {
        base: 'vs-dark',
        inherit: true,
        rules: [
          { token: 'error-token', foreground: '#ff0000' },
          { token: 'function', foreground: '#E1D093' },
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
    if (monaco && isFetched && !completionRegistered) {
      let common = [
        [/([A-Z_][A-Z_0-9]*)(\s*)(\()/, [{ token: 'function', next: '@otherFunction' }, '', 'delimiter']],
        [/(dtmi:com:willowinc:\w+;[0-9])/, 'type.identifier'],
        [/[0-9]+/, 'number'],
        [/\w+(?:_\w+)/, 'variable.name']
      ] as any[];

      data?.globals?.items?.forEach(item => common.push([item.name!, 'function']));

      data?.mlModels?.items?.forEach(item => common.push([item.fullName!, 'function']));
      parameters.forEach(item => common.push([item.fieldId!, 'variable.name']));

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

          common: common
        },
      });

      const d = monaco.languages.registerCompletionItemProvider(`willowLang${editorType}`, {
        triggerCharacters: ['.'],
        provideCompletionItems: (model, position) => {
          const word = model.getWordUntilPosition(position);

          const range = {
            startLineNumber: position.lineNumber,
            endLineNumber: position.lineNumber,
            startColumn: word.startColumn,
            endColumn: word.endColumn,
          };

          let lineText = model.getValueInRange({
            startLineNumber: position.lineNumber,
            endLineNumber: position.lineNumber,
            startColumn: 0,
            endColumn: position.column,
          });

          let lastIndexModel = lineText.lastIndexOf('[');
          let startColumnModel = lastIndexModel !== -1 ? lastIndexModel : 0;
          let modelText = lineText.substring(startColumnModel, position.column);

          let lastIndexProp = lineText.lastIndexOf('.');
          let prevIndexProp = lineText.lastIndexOf('.', lastIndexProp - 1);
          let startColumnProp = prevIndexProp !== -1 ? prevIndexProp : 0;
          let propText = lineText.substring(startColumnProp, position.column);

          const prevCharRange = {
            startLineNumber: position.lineNumber,
            startColumn: position.column - 1,
            endLineNumber: position.lineNumber,
            endColumn: position.column - 0
          };

          let prevChar = model.getValueInRange(prevCharRange);

          if (prevChar === ".") {
            propMode = true;
          } else {
            propMode = false;
          }

          if (propMode) {
            let modelid = "";
            let match = modelText.match(/\[([^\]]+)\]/); // /\[([^\]]+)\]\.\w*$/
            if (match) {
              modelid = match[1];
            }

            let propName = "";
            let match2 = propText.match(/\.(.*?)\./);
            if (match2) {
              propName = match2[1];
            }

            let currModel = data?.models?.find(model => model.id === modelid);
            let propType = currModel?.properties?.find(prop => prop.propertyName === propName)?.propertyType;
            
            let nestedProps = data?.models?.find(model => model.id === propType);

            let inheritedModels = data?.models?.filter(model => currModel.inheritedModelIds.includes(model.id));
            let inheritedProps = inheritedModels.map(model => model.properties).flat();

            if (nestedProps) {
              return {
                suggestions: [...nestedProps.properties.map(item => ({
                  label: item.propertyName ?? "",
                  kind: monaco.languages.CompletionItemKind.Property,
                  insertText: item.propertyName ?? "", 
                  sortText: 'a',
                  insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                  range, 
                }))?? [], ...data?.models?.map(item => ({
                  label: item.id!,
                  kind: monaco.languages.CompletionItemKind.Class,
                  insertText: '[' + item.id! + ']',
                  sortText: 'b',
                  insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                  range,
                })) ?? []]
              };
            }
            if ((!propName.includes("[")) && (propName !== "")) {
              return {
                suggestions: [...data?.models?.map(item => ({
                  label: item.id!,
                  kind: monaco.languages.CompletionItemKind.Class,
                  insertText: '[' + item.id! + ']',
                  insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                  range,
                })) ?? []]
              };
            }
            return {
              suggestions: [...currModel?.properties?.map(item => ({
                label: item.propertyName ?? "",
                kind: monaco.languages.CompletionItemKind.Property,
                insertText: item.propertyName ?? "",
                sortText: 'a',
                insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                range,
              })) ?? [], ...inheritedProps.map(item => ({
                label: item.propertyName ?? "",
                kind: monaco.languages.CompletionItemKind.Property,
                insertText: item.propertyName ?? "",
                sortText: 'b',
                insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                range,
              })) ?? [], ...data?.models?.map(item => ({
                label: item.id!,
                kind: monaco.languages.CompletionItemKind.Class,
                insertText: '[' + item.id! + ']',
                sortText: 'c',
                insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                range,
              })) ?? []]
            };
          }
          
          return {
            suggestions: [...data?.models?.map(item => ({
              label: item.id!,
              kind: monaco.languages.CompletionItemKind.Class,
              insertText: '[' + item.id! + ']',
              insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
              range,
            })) ?? [], ...data?.globals?.items?.map(item => ({
              label: item.name!,
              kind: monaco.languages.CompletionItemKind.Value,
              insertText: item.parameters!.length > 0 ? `${item.name!}(${item.parameters!.map(v => v.name).join(", ")})` : item.name!,
              insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
              range,
            })) ?? [], ...data?.mlModels?.items?.map(item => ({
              label: item.fullName!,
              kind: monaco.languages.CompletionItemKind.Keyword,
              insertText: item.inputParams!.length > 0 ? `${item.fullName!}(${item.inputParams!.map(v => v.name).join(", ")})` : item.fullName!,
              insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
              range,
            })) ?? [], ...defaultFunctions.map(item => ({
              label: item.label ?? "",
              kind: monaco.languages.CompletionItemKind.Function,
              insertText: item.label + (item.defaults.length > 0 ? item.defaults : '(${0})') ?? "",
              insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
              range,
            })) ?? [], ...defaultVariables.map(item => ({
              label: item ?? "",
              kind: monaco.languages.CompletionItemKind.Variable,
              insertText: item ?? "",
              insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
              range,
            })) ?? [], ...parameters?.map(item => ({
              label: item.fieldId!,
              kind: monaco.languages.CompletionItemKind.Variable,
              insertText: item.fieldId!,
              insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
              range,
            })) ?? []]
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

          const matchingOption = defaultFunctions.find(opt => opt.label === word.word);
          if (matchingOption) {
            return {
              range,
              contents: [
                {
                  value: matchingOption?.returnType && matchingOption?.returnType.length > 0 ? matchingOption?.returnType + ' ' + matchingOption?.syntax : matchingOption?.syntax
                },
                {
                  value: matchingOption?.description
                }
              ]
            };
          } else {

            const globalOption = data?.globals?.items?.find(opt => opt.name === word.word);
            if (globalOption) {
              let syntax = globalOption.parameters!.map(v => `${v.name}`).join(", ");
              let paramDescriptions = globalOption.parameters!.map(v => `***${v.name}***${v.units!?.length > 0 ? " (" + v.units + ")" : ""}: *${(v.description!?.length > 0 ? v.description : "No description")}*`).join('\n\n');

              syntax = globalOption?.parameters!.length == 0 ? globalOption.name! : `${globalOption.name}(${syntax})`;

              syntax = `<a style="text-decoration:none" href = "${location.origin}${baseurl}global/${globalOption.id}" >${syntax}</a>`;

              return {
                range,
                contents: [
                  {
                    supportHtml: true,
                    isTrusted: true,
                    value: globalOption?.units && globalOption?.units.length > 0 ? globalOption?.units + ' ' + syntax : syntax
                  },
                  {
                    value: `${globalOption?.description}\n\n${paramDescriptions}`
                  },
                  {
                    value: `***${globalOption.variableType == 0 ? "Macro" : "Function"}***: *${globalOption.expression!.length > 0 ? globalOption.expression![globalOption.expression!.length - 1].pointExpression : 'REDACTED'}*`
                  }
                ]
              };
            }

            const mlModelOption = data?.mlModels?.items?.find(opt => opt.fullName === word.word);
            if (mlModelOption) {
              let syntax = mlModelOption.inputParams!.map(v => `${v.name}`).join(", ");

              let paramDescriptions = mlModelOption.inputParams!.map(v => `***${v.name}***${v.unit!?.length > 0 ? " (" + v.unit + ")" : ""}: *No description*`).join('\n\n');

              syntax = mlModelOption?.inputParams!.length == 0 ? mlModelOption.fullName! : `${mlModelOption.fullName}(${syntax})`;

              syntax = `<a style="text-decoration:none" href = "${location.origin}${baseurl}mlmodel/${mlModelOption.id}" >${syntax}</a>`;

              return {
                range,
                contents: [
                  {
                    supportHtml: true,
                    isTrusted: true,
                    value: syntax
                  },
                  {
                    value: `${mlModelOption?.description ?? ""}\n\n${paramDescriptions}`
                  }
                ]
              };
            }

            return null;
          }
        }
      })
      completionRegistered = true;

      return () => {
        d.dispose();
        d2.dispose();
        completionRegistered = false;
      }
    }
  }, [monaco, isFetched, editorType, parameters]);

  return monaco;
}
