import { GlobalVariableDto, GlobalVariableType } from '../Rules';

const _macro = { text: "Macro", value: GlobalVariableType._0 };
const _function = { text: "Function", value: GlobalVariableType._1 };

const InlineItem = (params: { variableType: any, value: GlobalVariableType }) => {
  const variableType = params.variableType;
  const value = params.value;
  return ((value == variableType.value) ? <span>{variableType.text}</span> : <></>);
}

export function GetGlobalVariableTypeFilter() {
  return [
    { label: _macro.text, value: _macro.value },
    { label: _function.text, value: _function.value }
  ];
}


export function GetGlobalVariableTypeText(globalVariable: GlobalVariableDto) {
  return GetGlobalVariableTypeFilter().find(v => v.value == globalVariable.variableType)?.label;
}

export const GlobalVariableTypeFormatter = (globalVariable: GlobalVariableDto) => {
  const variableType = globalVariable.variableType!;

  return (
    <>
      <InlineItem variableType={_macro} value={variableType} />
      <InlineItem variableType={_function} value={variableType} />
    </>
  );
}
