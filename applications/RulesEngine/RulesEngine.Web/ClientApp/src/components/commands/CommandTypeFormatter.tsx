import { CommandDto, CommandType } from '../../Rules';

const set = { text: "Set", value: CommandType._1 };
const atMost = { text: "AtMost", value: CommandType._2 };
const atLeast = { text: "AtLeast", value: CommandType._3 };

const InlineItem = (params: { type: any, value: CommandType }) => {
  const type = params.type;
  const value = params.value;
  return ((value == type.value) ? <span>{type.text}</span> : <></>);
}

export function GetCommandTypeText(type: CommandType) {
  return GetCommandTypeFilter().find(v => v.value == type)?.label;
}

export function GetCommandTypeFilter() {
  return [
    { label: set.text, value: set.value },
    { label: atMost.text, value: atMost.value },
    { label: atLeast.text, value: atLeast.value },
  ];
}

export const CommandTypeFormatter = (command: CommandDto) => {
  const type = command.commandType!;

  return (
    <>
      <InlineItem type={set} value={type} />
      <InlineItem type={atMost} value={type} />
      <InlineItem type={atLeast} value={type} />
    </>
  );
}
