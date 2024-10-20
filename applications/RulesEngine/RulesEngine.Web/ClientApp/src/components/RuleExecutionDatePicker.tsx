import { DateInput } from '@willowinc/ui';
import { useState } from 'react';

const RuleExecutionDatePicker = (props: { maxDays: number, days: number, onChange: (value: number) => void, showLabel?: boolean }) => {
  const defaultStartDate = new Date();

  defaultStartDate.setDate(defaultStartDate.getDate() - props.days);

  const [updatedValue, setValue] = useState<Date | null>(defaultStartDate);

  const totalDays = function (date: Date) {
    return Math.abs(Math.round((new Date(date).getTime() - new Date().getTime()) / (1000 * 60 * 60 * 24)));
  }

  let minDate = new Date();

  minDate.setDate(minDate.getDate() - props.maxDays);

  return (
    <DateInput
      label={(props.showLabel ?? true) ? "Start date" : ""}
      value={updatedValue}
      minDate={minDate}
      maxDate={new Date()}
      onChange={(newValue: any) => {
        if (newValue !== null) {
          props.onChange(totalDays(newValue));
        }
        setValue(newValue);
      }}
    />
  );
};


export default RuleExecutionDatePicker
