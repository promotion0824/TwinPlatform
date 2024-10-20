import { useState, useLayoutEffect, useRef, useMemo } from 'react';
import { Combobox, useCombobox, ScrollArea } from '@mantine/core';
import { TextInput, Loader } from '@willowinc/ui';
import { GridRenderEditCellParams } from '@mui/x-data-grid-pro';
import styled from '@emotion/styled';
import { useMappings } from '../MappingsProvider';
import { IInterfaceTwinsInfo } from '../../../services/Clients';
import useOntology from '../../../hooks/useOntology/useOntology';

export default function ModelIdSelector(props: GridRenderEditCellParams) {
  const { tableApiRef } = useMappings();
  const { id, value: modelIdValue, field, hasFocus, models, error } = props;

  const { data: ontology, isFetching, isError } = useOntology();

  const combobox = useCombobox({
    onDropdownClose: () => {
      combobox.resetSelectedOption();

      setSearch('');
    },
  });

  const ref = useRef<HTMLInputElement>(null);

  useLayoutEffect(
    () => {
      if (hasFocus) {
        // select entire text when focused
        ref?.current?.select();
      } else {
        combobox.closeDropdown();
      }
    }, // eslint-disable-next-line react-hooks/exhaustive-deps
    [hasFocus]
  );

  const handleValueChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = event.target.value;
    setValue(newValue);
    combobox.updateSelectedOptionIndex();
    setSearch(event.currentTarget.value);
    await tableApiRef.current.setEditCellValue({ id, field, value: newValue });

    combobox.openDropdown();
  };

  const [value, setValue] = useState(ontology.getModelById(modelIdValue)?.name);
  const [search, setSearch] = useState('');

  const sortedModels = useMemo(
    () => ontology.getModels().sort((a: IInterfaceTwinsInfo, b: IInterfaceTwinsInfo) => a.name!.localeCompare(b.name!)),
    [ontology]
  );

  const shouldFilterOptions = useMemo(
    () => sortedModels.every(({ name }: IInterfaceTwinsInfo) => name !== search),
    [sortedModels, search]
  );

  const filteredOptions = useMemo(
    () =>
      shouldFilterOptions
        ? sortedModels.filter(({ name }: IInterfaceTwinsInfo) =>
            name?.toLowerCase().includes(search.toLowerCase().trim())
          )
        : models,
    [models, search, shouldFilterOptions, sortedModels]
  );

  const options = useMemo(
    () =>
      filteredOptions.map(({ id, name }: IInterfaceTwinsInfo) => {
        return (
          <StyledOptions value={id!} key={id}>
            <span>{name}</span>
            <br />
            <span>{`(${id})`}</span>
          </StyledOptions>
        );
      }),
    [filteredOptions]
  );

  return (
    <Combobox
      onOptionSubmit={async (optionValue) => {
        await tableApiRef.current.setEditCellValue({ id, field, value: optionValue });

        setValue(ontology.getModelById(optionValue)?.name);

        combobox.closeDropdown();
      }}
      store={combobox}
      width={373}
      position="bottom-start"
    >
      <Combobox.Target>
        <TextInput
          // @ts-ignore
          ref={ref}
          placeholder="Search or select model"
          value={value}
          onChange={handleValueChange}
          onClick={() => combobox.openDropdown()}
          onFocus={() => combobox.openDropdown()}
          error={error}
        />
      </Combobox.Target>

      <Combobox.Dropdown>
        <ScrollArea.Autosize type="auto" mah={'35vh'}>
          {options}

          {/*Loading state*/}
          {isFetching && (
            <Flex>
              <NoHeightLoader size="md" variant="dots" />
            </Flex>
          )}

          {/*No records found state*/}
          {options.length === 0 && <Flex>No records found</Flex>}

          {/*Error state*/}
          {isError && <Flex>Error fetching data</Flex>}
        </ScrollArea.Autosize>
      </Combobox.Dropdown>
    </Combobox>
  );
}

const Flex = styled('div')({ display: 'flex', justifyContent: 'center', padding: '0.5rem !important' });

const NoHeightLoader = styled(Loader)({ height: 0 });
const StyledOptions = styled(Combobox.Option)(({ theme }) => ({
  color: '#c6c6c6 !important',
  backgroundColor: 'unset',
  padding: '0.25rem 0.5rem !important',
  borderRadius: '2px !important',
  opacity: 1,
  font: '400 0.75rem/1.25rem Poppins, Arial, sans-serif !important',

  '&:hover': { color: '#e2e2e2', backgroundColor: '#272727 !important' },
}));
