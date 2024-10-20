import { useState } from 'react';
import { Combobox, InputBase, useCombobox, ScrollArea } from '@mantine/core';
import { Icon, Radio, Loader, Badge, Tooltip, Checkbox } from '@willowinc/ui';
import styled from '@emotion/styled';
import { useMappings } from '../MappingsProvider';
import useGetFilterDropdown from '../hooks/useGetFilterDropdown';
import { IMappedEntriesGroupCount } from '../../../services/Clients';

export default function ApproveAcceptDropdownFilters() {
  const { data, isLoading } = useGetFilterDropdown();
  const { buildingIdsState, connectorIdState, disabledBuildingFilter, disabledConnectorFilter } = useMappings();

  const { buildingIdGroupedEntries = [], connectorIdGroupedEntries = [] } = data || {};

  const shouldDisabledBuildingDropdown = disabledBuildingFilter;

  const shouldDisabledConnectorDropdown = disabledConnectorFilter;

  return (
    <Flex>
      <MultiSelectInput
        label="Select Buildings"
        data={arrayToDictionary(buildingIdGroupedEntries)}
        isLoading={isLoading}
        value={buildingIdsState[0]}
        onChange={buildingIdsState[1]}
        disabled={shouldDisabledBuildingDropdown}
      />

      <Dropdown
        label="Select Connector"
        data={arrayToDictionary(connectorIdGroupedEntries)}
        isLoading={isLoading}
        value={connectorIdState[0]}
        onChange={connectorIdState[1]}
        disabled={shouldDisabledConnectorDropdown}
      />
    </Flex>
  );
}

function arrayToDictionary(array: IMappedEntriesGroupCount[]): Record<string, IMappedEntriesGroupCount> {
  return array.reduce((acc: Record<string, IMappedEntriesGroupCount>, item: IMappedEntriesGroupCount) => {
    if (item.key !== undefined) {
      acc[item.key] = item;
    }
    return acc;
  }, {});
}

const Flex = styled.div({
  display: 'flex',
  gap: 16,
});

function Dropdown({
  label,
  data,
  isLoading,
  onChange,
  value,
  disabled,
}: {
  label: string;
  data: Record<string, IMappedEntriesGroupCount>;
  isLoading: boolean;
  onChange: (value: string | null) => void;
  value: string | null;
  disabled?: boolean;
}) {
  const { twinsLookup } = useMappings();
  const { data: twinsLookupData } = twinsLookup;
  const valueState = useState<string | null>(value);

  const combobox = useCombobox({
    onDropdownClose: () => combobox.resetSelectedOption(),
  });

  const options = Object.keys(data).map((k) => {
    const { key, count } = data[k];
    return (
      <StyledOptions
        value={key!}
        key={key}
        title={`${(key && twinsLookupData.getTwinById(key)?.name) || key} (${count})`}
      >
        <OptionContainer>
          <OptionLeftSideContainer>
            <Radio checked={valueState[0] === key} readOnly />
            <OptionValue>{`${(key && twinsLookupData.getTwinById(key)?.name) || key}`}</OptionValue>
          </OptionLeftSideContainer>

          <StyledBadge mr={8}>{count}</StyledBadge>
        </OptionContainer>
      </StyledOptions>
    );
  });

  const handleChange = (val: string | null) => {
    valueState[1](val);
    onChange(val);
  };

  const noOptions = Object.keys(data).length === 0;

  const disabledDropdown = disabled || isLoading || noOptions;
  return (
    <Combobox
      store={combobox}
      onOptionSubmit={(val) => {
        if (val === value) {
          handleChange(null);
          return;
        }
        handleChange(val);
        combobox.closeDropdown();
      }}
      width={240}
      position="bottom-start"
    >
      <Combobox.Target>
        <Tooltip
          label={disabled ? 'Disabled' : noOptions ? 'No options' : undefined}
          disabled={(!disabled && !noOptions) || isLoading}
          position="bottom"
        >
          {/* @ts-expect-error*/}
          <StyledInputBase
            component="button"
            type="button"
            pointer
            label={label}
            rightSection={
              isLoading ? (
                <Loader />
              ) : valueState[0] && !disabled ? (
                <PointerIcon
                  icon="close"
                  onClick={() => {
                    handleChange(null);
                  }}
                />
              ) : (
                <StyledIcon
                  // @ts-expect-error
                  icon="expand_more"
                  disabled={disabled}
                  onClick={() => {
                    if (!disabledDropdown) combobox.toggleDropdown();
                  }}
                />
              )
            }
            onClick={() => combobox.toggleDropdown()}
            disabled={disabledDropdown}
          >
            <ValueContainer
              title={(valueState[0] && twinsLookupData.getTwinById(valueState[0])?.name) || valueState[0]}
            >
              {disabled ? (
                ''
              ) : (
                <>
                  <Value>{(valueState[0] && twinsLookupData.getTwinById(valueState[0])?.name) || valueState[0]}</Value>
                  {data[valueState[0]!]?.count && (
                    <StyledBadge color="purple">{data[valueState[0]!]?.count}</StyledBadge>
                  )}
                </>
              )}
            </ValueContainer>
          </StyledInputBase>
        </Tooltip>
      </Combobox.Target>

      <Combobox.Dropdown>
        <ScrollArea.Autosize type="auto" maw={240} mah={176}>
          {options}
        </ScrollArea.Autosize>
      </Combobox.Dropdown>
    </Combobox>
  );
}

function MultiSelectInput({
  label,
  data,
  isLoading,
  onChange,
  value,
  disabled,
}: {
  label: string;
  data: Record<string, IMappedEntriesGroupCount>;
  isLoading: boolean;
  onChange: React.Dispatch<React.SetStateAction<string[]>>;
  value: string[];
  disabled?: boolean;
}) {
  const { twinsLookup } = useMappings();
  const { data: twinsLookupData } = twinsLookup;

  const checkAllState = useState<boolean>(false);

  const handleValueSelect = (val: string) => {
    if (val === 'all') {
      if (checkAllState[0]) {
        checkAllState[1](false);
        onChange([]);
      } else {
        checkAllState[1](true);
        // Bandaid fix when all the options has been selected and user's select "all" option, table return empty.
        if (Object.keys(data).length === value.length) {
          return;
        }
        onChange(Object.keys(data).map((k) => data[k].key!));
      }
      return;
    }

    if (checkAllState[0]) {
      checkAllState[1](false);
    }
    onChange((current: string[]) =>
      current.includes(val) ? current.filter((v: string) => v !== val) : [...current, val]
    );
  };

  const combobox = useCombobox({
    onDropdownClose: () => combobox.resetSelectedOption(),
  });

  const options = Object.keys(data).map((k) => {
    const { key, count } = data[k];
    return (
      <StyledOptions
        value={key!}
        key={key}
        title={`${(key && twinsLookupData.getTwinById(key)?.name) || key} (${count})`}
      >
        <OptionContainer>
          <OptionLeftSideContainer>
            <Checkbox checked={value.includes(key!)} />
            <OptionValue>{`${(key && twinsLookupData.getTwinById(key)?.name) || key}`}</OptionValue>
          </OptionLeftSideContainer>
          <StyledBadge mr={8}>{count}</StyledBadge>
        </OptionContainer>
      </StyledOptions>
    );
  });

  const AllOption = (
    <StyledOptions value={'all'} key={'all'} title="All">
      <OptionContainer>
        <OptionLeftSideContainer>
          <Checkbox checked={checkAllState[0]} />
          <OptionValue>{'All'}</OptionValue>
        </OptionLeftSideContainer>
      </OptionContainer>
    </StyledOptions>
  );

  const inputValueCount = value.length > 0 ? `(${value.length}) ` : '';
  // convert id to twins' name and concatenate them into a string
  const inputValue = value.map((v) => twinsLookupData.getTwinById(v)?.name || v).join(', ');

  const noOptions = Object.keys(data).length === 0;

  const disabledDropdown = disabled || isLoading || noOptions;
  return (
    <Combobox store={combobox} onOptionSubmit={handleValueSelect} width={240} position="bottom-start">
      <Combobox.Target>
        <Tooltip
          label={disabled ? 'Disabled' : noOptions ? 'No options' : undefined}
          disabled={(!disabled && !noOptions) || isLoading}
          position="bottom"
        >
          {/* @ts-expect-error*/}
          <StyledInputBase
            component="button"
            pointer
            label={label}
            rightSection={
              isLoading ? (
                <Loader />
              ) : (
                <StyledIcon
                  // @ts-expect-error
                  icon="expand_more"
                  disabled={disabled}
                  onClick={() => {
                    if (!disabledDropdown) combobox.toggleDropdown();
                  }}
                />
              )
            }
            onClick={() => combobox.toggleDropdown()}
            disabled={disabledDropdown}
          >
            <ValueContainer1>{`${inputValueCount + inputValue}`}</ValueContainer1>
          </StyledInputBase>
        </Tooltip>
      </Combobox.Target>

      <Combobox.Dropdown>
        <ScrollArea.Autosize type="auto" maw={240} mah={176}>
          {AllOption}
          {options}
        </ScrollArea.Autosize>
      </Combobox.Dropdown>
    </Combobox>
  );
}

const ValueContainer1 = styled('div')({
  whiteSpace: 'nowrap',
  textOverflow: 'ellipsis',
  overflow: 'hidden',
  boxSizing: 'border-box',
  width: '90%',
});

const StyledOptions = styled(Combobox.Option)(({ theme }) => ({
  color: '#c6c6c6 !important',
  backgroundColor: 'unset',
  padding: '0.25rem !important',
  borderRadius: '2px !important',
  opacity: 1,
  font: '400 0.75rem/1.25rem Poppins, Arial, sans-serif !important',

  '&:hover': { color: '#e2e2e2 !important', backgroundColor: '#272727 !important' },

  maxWidth: 235,
}));

const OptionContainer = styled('div')({
  display: 'flex',
  gap: 8,
  alignItems: 'center',
  justifyContent: 'space-between',

  width: '100%',
});
const OverflowStyle = {
  overflow: 'hidden',
  textOverflow: 'ellipsis',
  whiteSpace: 'nowrap',
};

const OptionLeftSideContainer = styled('div')({
  display: 'flex',
  flexDirection: 'row',
  alignItems: 'center',
  gap: 8,
  maxWidth: '86%',
  ...OverflowStyle,
});

const OptionValue = styled('div')({
  ...OverflowStyle,
  flexShrink: 1,
});

const StyledBadge = styled(Badge)({
  flexShrink: 0, // Prevents the badge from shrinking
  whiteSpace: 'nowrap', // Ensures the content doesn't wrap
});

const ValueContainer = styled('div')({
  display: 'flex',
  gap: 4,
  alignItems: 'center',
  justifyContent: 'space-between',
});

const Value = styled('div')({
  maxWidth: 160,
  ...OverflowStyle,
});

interface IconProps {
  disabled?: boolean;
  icon: string;
  onClick: () => void;
}
const StyledIcon = styled(Icon)<IconProps>(({ disabled }) => ({
  color: disabled ? '#474747 !important' : '#c6c6c6',
  '&:hover': {
    cursor: disabled ? 'not-allowed' : 'pointer',
  },
}));

const PointerIcon = styled(Icon)({
  '&:hover': {
    cursor: 'pointer',
  },
});

const StyledInputBase = styled(InputBase)({
  width: 240,
  ' > div > button': {
    fontFamily: 'Poppins, Arial, sans-serif',
    fontWeight: 400,
    fontSize: '0.75rem',
    lineHeight: '1.25rem',
    borderRadius: '2px',

    border: '1px #3b3b3b solid',
    boxShadow: '0 1px 2px 0 rgba(0, 0, 0, 16%)',
    color: '#c6c6c6',
    backgroundColor: '#242424',
    padding: '0.25rem calc(var(--input-right-section-size) + 0.25rem) 0.25rem 0.5rem',
    height: '1.75rem',
    minHeight: 'unset',
    '&:hover': {
      backgroundColor: '#272727',
    },
    '&:focus-within': {
      borderColor: '#ababab',
    },
    '&:disabled': {
      cursor: 'not-allowed',
      opacity: 0.6,
      outline: '1px solid #3b3b3b',
      outlineOffset: '-1px',
      color: '#474747 !important',
      fill: '#474747 !important',
      backgroundColor: '#242424 !important',
    },
  },
});
