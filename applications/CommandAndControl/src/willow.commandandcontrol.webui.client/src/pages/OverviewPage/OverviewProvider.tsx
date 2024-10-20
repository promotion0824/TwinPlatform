import { createContext, useContext, useEffect } from "react";
import useGetStatistics, { IGetStatistic } from "./hooks/useGetStatistics";
import { DateRangePickerPortal } from "../../components/DateRangePicker/DateRangePicker";
import { useAppContext } from "../../providers/AppContextProvider";
import { AppName } from "../../utils/appName";

type OverviewContextType = { getStatisticsQuery: IGetStatistic["query"] };

const OverviewContext = createContext<OverviewContextType | undefined>(
  undefined
);

export function useOverviewContext() {
  const { breadCrumbsState } = useAppContext();

  useEffect(() => {
    breadCrumbsState[1]([{ text: AppName, to: "/" }])
  }, []);

  const context = useContext(OverviewContext);
  if (context == null) {
    throw new Error(
      "useOverviewContext must be used within a OverviewProvider"
    );
  }

  return context;
}

export default function OverviewProvider({
  children,
}: {
  children: any;
}): React.ReactNode {
  const { query, dateRangeState } = useGetStatistics({
    refetchOnMount: true,
    refetchInterval: 30000,
  });

  return (
    <OverviewContext.Provider value={{ getStatisticsQuery: query }}>
      <>{children}</>
      <DateRangePickerPortal dateRangeState={dateRangeState} />
    </OverviewContext.Provider>
  );
}
