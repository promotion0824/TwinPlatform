import { Loader, Tooltip, Checkbox } from '@willowinc/ui';
import { useState, useEffect } from 'react';
import { Combobox, InputBase, useCombobox, ScrollArea } from '@mantine/core';
import styled from '@emotion/styled';
import useGetAllTwins from '../../hooks/useGetAllTwins';

export type SelectedBuildingType = {
  label: string;
  value: string;
  id?: string;
};

/**
 * Dropdown input component for selecting mapped buildings
 */
export default function MappedBuildingSelector({
  selectedBuildingsState,
}: {
  selectedBuildingsState: [SelectedBuildingType[], React.Dispatch<React.SetStateAction<SelectedBuildingType[]>>];
}) {
  const { data = [], isLoading } = useGetAllTwins(['dtmi:com:willowinc:Building;1'], {
    select: (data) =>
      data
        ?.filter(({ twin }) => twin?.externalID)
        .sort((a, b) => a?.twin?.name.localeCompare(b?.twin?.name))
        .map(({ twin }) => ({ label: twin?.name, id: twin?.$dtId, value: twin?.externalID } || {})) || [],
  });

  return (
    <>
      <Tooltip label="No buildings found" position="bottom" disabled={data.length > 0 || isLoading}>
        <MultiSelectSearchInput
          isLoading={isLoading}
          label="Select Buildings"
          placeholder="Select Buildings"
          data={data as any}
          value={selectedBuildingsState[0]}
          onChange={selectedBuildingsState[1]}
        />
      </Tooltip>
    </>
  );
}

function MultiSelectSearchInput({
  label,
  data,
  isLoading,
  onChange,
  value = [],
  disabled,
  placeholder,
}: {
  label: string;
  data: SelectedBuildingType[];
  isLoading: boolean;
  onChange: React.Dispatch<React.SetStateAction<SelectedBuildingType[]>>;
  value: SelectedBuildingType[];
  disabled?: boolean;
  placeholder: string;
}) {
  const [search, setSearch] = useState('');
  const checkboxValuesState = useState<string[]>([]);
  const checkAllState = useState<boolean>(false);

  const handleValueSelect = (val: string) => {
    if (val === 'all') {
      if (checkAllState[0]) {
        checkAllState[1](false);
        checkboxValuesState[1]([]);
      } else {
        checkAllState[1](true);
        // Bandaid fix when all the options has been selected and user's select "all" option, table return empty.
        if (Object.keys(data).length === value.length) {
          return;
        }
        checkboxValuesState[1](data.map(({ value }) => value));
      }
      return;
    }

    if (checkAllState[0]) {
      checkAllState[1](false);
    }
    checkboxValuesState[1]((current: string[]) =>
      current.includes(val) ? current.filter((v: string) => v !== val) : [...current, val]
    );
  };

  useEffect(() => {
    const currentSelections = data.filter(({ value }) => checkboxValuesState[0].includes(value));
    onChange(currentSelections);
  }, [checkboxValuesState, data, onChange]);

  const combobox = useCombobox({
    onDropdownClose: () => {
      combobox.resetSelectedOption();
      combobox.focusTarget();
      setSearch('');
    },
    onDropdownOpen: () => {},
  });

  const options = data
    .filter(({ label }) => label.toLowerCase().includes(search.toLowerCase().trim()))
    .map(({ label, value }) => {
      return (
        <StyledOptions value={value!} key={label} title={label}>
          <OptionContainer>
            <OptionLeftSideContainer>
              <Checkbox checked={checkboxValuesState[0].includes(value)} readOnly />
              <OptionValue>{label}</OptionValue>
            </OptionLeftSideContainer>
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
  const inputValue = value.map(({ label }) => label).join(', ');

  const noOptions = data.length === 0;

  const disabledDropdown = disabled || isLoading || noOptions;

  return (
    <Combobox store={combobox} onOptionSubmit={handleValueSelect}>
      <Combobox.Target>
        {/* @ts-ignore*/}
        <StyledInputBase
          component="button"
          pointer
          label={label}
          rightSection={isLoading ? <Loader /> : <StyledChevron />}
          onClick={() => combobox.toggleDropdown()}
          disabled={disabledDropdown}
          placeholder={placeholder}
        >
          <ValueContainer>{`${inputValueCount + inputValue}`}</ValueContainer>
        </StyledInputBase>
      </Combobox.Target>

      <Combobox.Dropdown>
        <StyledComboboxSearch
          value={search}
          onChange={(event) => setSearch(event.currentTarget.value)}
          placeholder={'Search Buildings'}
        />
        <ScrollArea.Autosize type="auto" mah={176} scrollbars="y">
          <Combobox.Options>
            {search === '' && AllOption}
            {options.length > 0 ? options : <NotFoundOption>No results found</NotFoundOption>}
          </Combobox.Options>
        </ScrollArea.Autosize>
      </Combobox.Dropdown>
    </Combobox>
  );
}

const NotFoundOption = styled('div')({ display: 'flex', justifyContent: 'center', padding: '0.5rem !important' });

const StyledComboboxSearch = styled(Combobox.Search)({
  ' > input': {
    borderColor: '#3b3b3b !important',
    marginTop: -1,
    marginLeft: '0px !important',
    borderLeft: 'none !important',
    borderRight: 'none !important',
    borderRadius: '0px !important',
    width: '100%',
  },
});

const StyledChevron = styled(Combobox.Chevron)({
  width: ' var(--combobox-chevron-size-xs) !important',
  height: 'var(--combobox-chevron-size-xs) !important',
});
const StyledOptions = styled(Combobox.Option)(({ theme }) => ({
  color: '#c6c6c6 !important',
  backgroundColor: 'unset',
  padding: '0.25rem !important',
  borderRadius: '2px !important',
  opacity: 1,
  font: '400 0.75rem/1.25rem Poppins, Arial, sans-serif !important',

  '&:hover': { color: '#e2e2e2 !important', backgroundColor: '#272727 !important' },
}));

const OptionContainer = styled('div')({
  display: 'flex',
  gap: 8,
  alignItems: 'center',
  justifyContent: 'space-between',
  width: '325px',
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
  maxWidth: '93%',
  ...OverflowStyle,
});

const OptionValue = styled('div')({
  ...OverflowStyle,
  flexShrink: 1,
});

const ValueContainer = styled('div')({
  whiteSpace: 'nowrap',
  textOverflow: 'ellipsis',
  overflow: 'hidden',
  boxSizing: 'border-box',
  width: '90%',
});

const StyledInputBase = styled(InputBase)({
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
