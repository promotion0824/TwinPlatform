import { useState } from 'react';

/** Use the hook for creating dropdown filter type. It will show all distinct options from rows. */
export const useSelectFilterGrid = () => {
  const [filterOptions, setFilterOptions] = useState<Array<string>>([]);

  /** Pass each value from specific cell during iteration. */
  const enableDropDownFilter = (option: string) => {
    if (!filterOptions.includes(option)) setFilterOptions(filterOptions.concat(option));
    return option;
  };
  return { enableDropDownFilter, filterOptions };
};

export default useSelectFilterGrid;
