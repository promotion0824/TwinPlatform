import {
  DateInput as WillowDateInput,
  DateInputProps,
  Combobox,
  useCombobox,
  DateTimeInput as WillowDateTimeInput,
  NumberInput,
  Button,
} from '@willowinc/ui';
import styled from '@emotion/styled';
import { forwardRef, useState, useMemo } from 'react';
import { InputBase } from '@mantine/core';

const FieldContainer = styled('div')({
  display: 'flex',
  flexDirection: 'row',
  height: '100%',
  flexWrap: 'wrap',
  paddingLeft: '1rem',
});

const GroupedPropertyContainer = styled('div')({
  border: '1px solid rgba(81, 81, 81, 1)',
  borderRadius: '4px',
  padding: '0.5rem 0',
  margin: '0.5rem',
});

const GroupPropertyName = styled('div')({
  paddingLeft: '1rem',
  color: '#d9d9d9',
  fontSize: 12,
  fontWeight: 700,
});

interface NestedContainerProps {
  isError: boolean;
}
const NestedGroupedPropertyContainer = styled('div')<NestedContainerProps>(({ isError = false }) => ({
  display: 'flex',
  minWidth: '230px',
  flexDirection: 'column',
  overflow: 'hidden',
  position: 'relative',
  backgroundColor: '#171717',
  border: isError ? '1px solid #c7504f' : '1px solid #3B3B3B',
  borderRadius: '4px',
  boxShadow: 'box-shadow: 0px 4px 8px 0px #00000040',
}));

const DateInput = forwardRef((props: DateInputProps, ref) => {
  return <StyledDateInput {...props} clearable={!props.disabled} valueFormat="MMM DD, YYYY" />;
});

const StyledDateInput = styled(WillowDateInput)({ '> input': { paddingLeft: 'unset !important' } });

const EnumInput = forwardRef((props: any, ref) => {
  const enumValues = useMemo(() => removeDuplicatesEnumValues(props.schema.enumValues), [props.schema.enumValues]);
  const enumLookupTable = useMemo(() => createEnumLookupTable(props.schema.enumValues), [props.schema.enumValues]);

  const combobox = useCombobox({
    onDropdownClose: () => combobox.resetSelectedOption(),
  });
  const [selectedValue, setSelectedValue] = useState<string | undefined>(props.value);

  const shouldDisable = props.disabled;

  return (
    <Combobox
      store={combobox}
      onOptionSubmit={(val) => {
        setSelectedValue(val);
        props.onChange(val);
        combobox.closeDropdown();
      }}
    >
      <Combobox.Target>
        <InputBase
          component="button"
          type="button"
          pointer
          disabled={shouldDisable}
          rightSection={
            !!selectedValue && !shouldDisable ? (
              <Combobox.ClearButton
                onClear={() => {
                  setSelectedValue(undefined);
                  props.onChange();
                }}
              />
            ) : !shouldDisable ? (
              <Combobox.Chevron />
            ) : undefined
          }
          onClick={() => combobox.toggleDropdown()}
        >
          {selectedValue && enumLookupTable[selectedValue]}
        </InputBase>
      </Combobox.Target>

      <Combobox.Dropdown>
        <Combobox.Options>
          {enumValues.map(({ name, displayName, enumValue }: any) => (
            <Combobox.Option value={enumValue} key={`${props.name} + ${enumValue}`} title={name}>
              {displayName?.en || displayName || name}
            </Combobox.Option>
          ))}
        </Combobox.Options>
      </Combobox.Dropdown>
    </Combobox>
  );
});

interface EnumValue {
  name: string;
  displayName: Record<string, string> | string;
  enumValue: string;
}

const removeDuplicatesEnumValues = (enumValues: EnumValue[]) => {
  return enumValues.filter((value, index, self) => self.findIndex((t) => t.enumValue === value.enumValue) === index);
};
const createEnumLookupTable = (enumValues: EnumValue[]) => {
  return enumValues.reduce((lookupTable, { enumValue, displayName, name }) => {
    //@ts-expect-error
    lookupTable[enumValue] = displayName?.['en'] || displayName || name;
    return lookupTable;
  }, {} as Record<string, string>);
};

const BooleanInput = forwardRef((props: any, ref) => {
  const combobox = useCombobox({
    onDropdownClose: () => combobox.resetSelectedOption(),
  });
  const [selectedValue, setSelectedValue] = useState<string | undefined>(props.value?.toString());

  const shouldDisable = props.disabled;
  return (
    <Combobox
      store={combobox}
      onOptionSubmit={(val) => {
        setSelectedValue(val);
        props.onChange(val.toLowerCase() === 'true');
        combobox.closeDropdown();
      }}
    >
      <Combobox.Target>
        <InputBase
          component="button"
          type="button"
          pointer
          disabled={shouldDisable}
          rightSection={
            !!selectedValue && !shouldDisable ? (
              <Combobox.ClearButton
                onClear={() => {
                  setSelectedValue(undefined);
                  props.onChange();
                }}
              />
            ) : !shouldDisable ? (
              <Combobox.Chevron />
            ) : undefined
          }
          onClick={() => combobox.toggleDropdown()}
        >
          {selectedValue}
        </InputBase>
      </Combobox.Target>

      <Combobox.Dropdown>
        <Combobox.Options>
          <Combobox.Option value={'true'} key={'true'} title={'true'}>
            {'true'}
          </Combobox.Option>
          <Combobox.Option value={'false'} key={'false'} title={'false'}>
            {'false'}
          </Combobox.Option>
        </Combobox.Options>
      </Combobox.Dropdown>
    </Combobox>
  );
});

const DateTimeInput = forwardRef((props: any, ref) => {
  return <WillowDateTimeInput {...props} type="date-time" placeholder="" />;
});

const DurationInput = forwardRef((props: any, ref) => {
  const combobox = useCombobox({
    onDropdownClose: () => combobox.resetSelectedOption(),
  });

  const shouldDisable = props.disabled;

  return (
    <Combobox
      store={combobox}
      onOptionSubmit={(val) => {
        combobox.closeDropdown();
      }}
    >
      <Combobox.Target>
        <InputBase
          component="button"
          type="button"
          pointer
          disabled={shouldDisable}
          rightSection={!shouldDisable && <Combobox.Chevron />}
          onClick={() => combobox.toggleDropdown()}
        >
          {formatDuration(props.value)}
        </InputBase>
      </Combobox.Target>

      <Combobox.Dropdown>
        <DurationContent value={props.value} onChange={props.onChange} />
      </Combobox.Dropdown>
    </Combobox>
  );
});

type RegularDuration<T> = {
  years: T;
  months: T;
  days: T;
  hours: T;
  minutes: T;
  seconds: T;
};

type WeeksDuration<T> = { weeks: T };

type Duration<T> = RegularDuration<T> | WeeksDuration<T>;

export type InputDuration = Duration<number>;

export type DurationState = Duration<number> | null;

function zeroRegularDuration(): RegularDuration<number> {
  return {
    years: 0,
    months: 0,
    days: 0,
    hours: 0,
    minutes: 0,
    seconds: 0,
  };
}

function DurationContent({ value, onChange }: { value: InputDuration; onChange: (val: DurationState) => void }) {
  const fields: (keyof RegularDuration<string>)[] = ['years', 'months', 'days', 'hours', 'minutes', 'seconds'];

  const [{ top }, setValues] = useState(() => {
    if (value == null) {
      return {
        top: zeroRegularDuration(),
      };
    } else if ('years' in value) {
      return {
        top: value,
      };
    } else {
      throw new Error(`Invalid value ${value}`);
    }
  });

  return (
    <div>
      {fields.map((f) => (
        <NumberInput
          layout="horizontal"
          label={f}
          mb={5}
          allowNegative={false}
          value={top?.[f]}
          onChange={(val) => {
            const newTop = { ...(top ?? zeroRegularDuration()), [f]: val as number };
            onChange(newTop);
            setValues({
              top: newTop,
            });
          }}
        />
      ))}
      <Button
        kind="secondary"
        background="transparent"
        onClick={() => {
          setValues({ top: zeroRegularDuration() });
          onChange(undefined!);
        }}
      >
        Clear
      </Button>
    </div>
  );
}
/**
 * Format a duration for display in the dropdown heading. Note this is not the
 * same as the ISO 8601 format (we could use tinyduration.serialize for that).
 */
export function formatDuration(duration: DurationState) {
  if (duration == null) {
    return '';
  }

  if ('years' in duration) {
    const { years, months, days, hours, minutes, seconds } = duration;
    const parts: string[] = [];
    if (years !== 0) {
      parts.push(`${years}Y`);
    }
    if (months !== 0) {
      parts.push(`${months}M`);
    }
    if (days !== 0) {
      parts.push(`${days}D`);
    }

    if (hours !== 0 || minutes !== 0 || seconds !== 0) {
      parts.push([hours, minutes, seconds].map((v) => v.toString().padStart(2, '0')).join(':'));
    }

    if (parts.length > 0) {
      return parts.join(' ');
    } else {
      return '0';
    }
  }
}
export {
  FieldContainer,
  GroupedPropertyContainer,
  GroupPropertyName,
  NestedGroupedPropertyContainer,
  DateInput,
  EnumInput,
  BooleanInput,
  DateTimeInput,
  DurationInput,
};
