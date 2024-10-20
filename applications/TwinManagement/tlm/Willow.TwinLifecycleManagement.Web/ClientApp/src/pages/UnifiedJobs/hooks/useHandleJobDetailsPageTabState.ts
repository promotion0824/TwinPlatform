/* eslint-disable react-hooks/exhaustive-deps */
import { useSearchParams } from 'react-router-dom';
import { useState, useEffect } from 'react';

type TabsName = 'inputs' | 'output' | 'errors' | 'customdata';

export default function useHandleJobDetailsTabsQueryParams() {
  const tabState = useState<TabsName>();
  const [searchParams, setSearchParams] = useSearchParams();

  const isTabsName = (tab: string = 'default'): tab is TabsName => {
    return ['inputs', 'output', 'errors', 'customdata'].includes(tab);
  };
  const tab = searchParams.get('tab');

  useEffect(() => {
    if (tabState[0]) setSearchParams({ ...Object.fromEntries(searchParams.entries()), tab: tabState[0] });
  }, [tabState[0]]);

  useEffect(() => {
    tabState[1](isTabsName(tab?.toLowerCase()) ? (tab?.toLowerCase() as TabsName) : ('details' as TabsName));
  }, [tab]);

  return tabState;
}
