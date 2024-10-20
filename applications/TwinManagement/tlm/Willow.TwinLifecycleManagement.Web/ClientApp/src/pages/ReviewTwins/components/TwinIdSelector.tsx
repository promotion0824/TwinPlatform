import { useState, useEffect, useLayoutEffect, useRef, useCallback } from 'react';
import { Combobox, useCombobox, ScrollArea } from '@mantine/core';
import { TextInput, Loader } from '@willowinc/ui';
import { GridRenderEditCellParams } from '@mui/x-data-grid-pro';
import styled from '@emotion/styled';
import useGetTwinsByModelIds from '../hooks/useGetTwinsByModelIds';
import { BasicDigitalTwin } from '../../../services/Clients';

import { useMappings } from '../MappingsProvider';

export default function TwinIdSelector(props: GridRenderEditCellParams) {
  const { tableApiRef } = useMappings();
  const { id, value: twinIdValue, field, hasFocus, row } = props;

  const { willowModelId, parentWillowId } = row;
  const { query, pageState, totalRecordsCount, pageSize } = useGetTwinsByModelIds([willowModelId], parentWillowId);

  const { data, isFetching, isError, isSuccess } = query;

  const [items, setItems] = useState<Set<string>>(new Set());

  useEffect(() => {
    if (data) {
      setItems((prev) => new Set([...prev, ...data?.content!.map((item) => JSON.stringify(item?.twin!))]));
    }
  }, [data]);

  const ref = useRef<HTMLInputElement>(null);

  const combobox = useCombobox({
    onDropdownClose: () => combobox.resetSelectedOption(),
  });

  useLayoutEffect(() => {
    if (hasFocus) {
      // select entire text when focused
      ref?.current?.select();
    } else {
      combobox.closeDropdown();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [hasFocus]);

  const handleValueChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = event.target.value;
    setValue(newValue);
    await tableApiRef.current.setEditCellValue({ id, field, value: newValue });

    combobox.openDropdown();
  };

  const [value, setValue] = useState(twinIdValue);

  const options = [...items].map((item) => {
    let twin = JSON.parse(item) as BasicDigitalTwin;
    return (
      <StyledOptions value={twin.$dtId!} key={twin.$dtId}>
        {`${twin.$dtId} ${twin.name && `(${twin.name})`}`}
      </StyledOptions>
    );
  });

  const fetchMoreData = useCallback(
    (flag: boolean) => {
      // No more fetching when we have all the records or if we're already fetching
      if (flag || isFetching) {
        return;
      }

      pageState[1]((prev) => prev + 1);
    },
    [pageState, isFetching]
  );

  const observerTarget = useRef(null);

  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting) {
          fetchMoreData(items.size >= totalRecordsCount || pageSize * pageState[0] > totalRecordsCount);
        }
      },
      { threshold: 0.1 }
    );

    if (observerTarget.current) {
      observer.observe(observerTarget.current);
    }

    return () => {
      if (observerTarget.current) {
        // eslint-disable-next-line react-hooks/exhaustive-deps
        observer.unobserve(observerTarget.current);
      }
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [observerTarget, items, totalRecordsCount, fetchMoreData, pageSize, pageState[0]]);

  return (
    <Combobox
      onOptionSubmit={async (optionValue) => {
        await tableApiRef.current.setEditCellValue({ id, field, value: optionValue });
        setValue(optionValue);
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
          placeholder="Type or select from list"
          value={value}
          onChange={handleValueChange}
          onClick={() => combobox.openDropdown()}
          onFocus={() => combobox.openDropdown()}
        />
      </Combobox.Target>

      <Combobox.Dropdown>
        <ScrollArea.Autosize type="auto" mah={176}>
          {options}

          {/* Whenever we scroll to the ObserverDiv, this will trigger another data fetch*/}
          {!isError && items.size !== 0 && !isFetching && items.size !== totalRecordsCount && (
            <ObserverDiv ref={observerTarget} />
          )}

          {/*Loading state*/}
          {isFetching && (
            <Flex>
              <NoHeightLoader size="md" variant="dots" />
            </Flex>
          )}

          {/*No records found state*/}
          {isSuccess && !isFetching && items.size === 0 && <Flex>No records found</Flex>}

          {/*Error state*/}
          {isError && <Flex>Error fetching data</Flex>}
        </ScrollArea.Autosize>
      </Combobox.Dropdown>
    </Combobox>
  );
}

const Flex = styled('div')({ display: 'flex', justifyContent: 'center', padding: '0.5rem !important' });
const ObserverDiv = styled('div')({ padding: 5 });

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
