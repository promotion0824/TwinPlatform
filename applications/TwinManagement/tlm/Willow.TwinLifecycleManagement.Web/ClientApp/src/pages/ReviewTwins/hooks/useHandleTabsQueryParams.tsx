/* eslint-disable react-hooks/exhaustive-deps */
import { TabsName } from '../MappingsProvider';
import { useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';

export default function useHandleTabsQueryParams(
  tabState: [TabsName | undefined, React.Dispatch<React.SetStateAction<TabsName | undefined>>]
) {
  const [searchParams, setSearchParams] = useSearchParams();

  useEffect(() => {
    if (tabState[0]) setSearchParams({ ...Object.fromEntries(searchParams.entries()), tab: tabState[0] });
  }, [tabState[0]]);

  const isTabsName = (tab: string = 'default'): tab is TabsName => {
    return ['things', 'points', 'spaces', 'miscellaneous', 'conflicts'].includes(tab);
  };
  const tab = searchParams.get('tab');

  useEffect(() => {
    tabState[1](isTabsName(tab?.toLowerCase()) ? (tab?.toLowerCase() as TabsName) : ('things' as TabsName));
  }, [tab]);
}
