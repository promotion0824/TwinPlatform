import { RuleTriggerDto, RuleTriggerType } from '../../Rules';

const command = { text: "Command", value: RuleTriggerType._1 };

const InlineItem = (params: { type: any, value: RuleTriggerType }) => {
  const type = params.type;
  const value = params.value;
  return ((value == type.value) ? <span>{type.text}</span> : <></>);
}

export function GetTriggerTypeText(type: RuleTriggerType) {
  return GetTriggerTypeFilter().find(v => v.value == type)?.label;
}

export function GetTriggerTypeFilter() {
  return [
    { label: command.text, value: command.value }
  ];
}

export const TriggerTypeFormatter = (data: RuleTriggerDto) => {
  const type = data.triggerType!;

  return (
    <>
      <InlineItem type={command} value={type} />
    </>
  );
}
