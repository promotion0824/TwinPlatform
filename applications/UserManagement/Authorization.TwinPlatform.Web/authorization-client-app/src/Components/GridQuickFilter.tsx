import { Icon, TextInput } from "@willowinc/ui";
import { useDebounce } from '@uidotdev/usehooks';
import { Dispatch, SetStateAction, useEffect, useState } from "react";
import { FilterSpecificationDto } from "../types/BatchRequestDto";
import { CustomTooltip } from "./CustomTooltip";

export default function GridQuickFilter({ filterFieldNames, setQuickFilters }: { filterFieldNames: string[], setQuickFilters: Dispatch<SetStateAction<FilterSpecificationDto[]>> }) {
  const [quickFilterText, setQuickFilterText] = useState<string>('');
  const debouncedQuickFilterText = useDebounce(quickFilterText, 1000);

  useEffect(() => {

    const filters: FilterSpecificationDto[] = [];

    if (!!debouncedQuickFilterText && !!filterFieldNames && filterFieldNames.length > 0) {
      filters.push(new FilterSpecificationDto(filterFieldNames.join(','), "contains", debouncedQuickFilterText, "OR", true));
    }

    setQuickFilters(filters);

  }, [debouncedQuickFilterText]);

  return (
    <CustomTooltip title={`Search in column : ${filterFieldNames.join(", ")} `} >
      <TextInput placeholder="Search" value={quickFilterText} onChange={(e) => setQuickFilterText(e.currentTarget.value)} clearable suffix={<Icon icon="search" />}>
      </TextInput>
    </CustomTooltip>);
}
