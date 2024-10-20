import { createContext, useContext, useState, PropsWithChildren, useMemo } from "react";
import useBreadcrumbs from "../components/Breadcrumbs/useBreadcrumbs";
import { Breadcrumb } from "../components/Breadcrumbs/Breadcrumbs";
import { UseState } from "../../types/UseState";
import { FilterSpecificationDto, SiteDto } from "../services/Clients";
import useLocalStorage from "../hooks/useLocalStorage";
import { SiteSelection } from "../../types/SiteSelection";

type AppContextType = {
  breadCrumbsState: UseState<Breadcrumb[]>;
  selectedSiteState: UseState<string>;
  selectedSite: string;
  requestsFilters: UseState<Record<string, FilterSpecificationDto[] | undefined>>;
  requestsSelectedDate: UseState<string | undefined>;
  commandsFilters: UseState<Record<string, FilterSpecificationDto[] | undefined>>;
  commandsSelectedDate: UseState<string | undefined>;
  activityLogsFilters: UseState<Record<string, FilterSpecificationDto[] | undefined>>;
  activityLogsSelectedDate: UseState<string | undefined>;
  activityLogsSelectedTypes: UseState<string[]>;
};

const AppContext = createContext<AppContextType | undefined>(undefined);

export function useAppContext() {
  const context = useContext(AppContext);
  if (context == null) {
    throw new Error("useAppContext must be used within a AppContextProvider");
  }

  return context;
}

export const AppContextProvider: React.FC<PropsWithChildren<{}>> = ({ children }) => {
  const { breadCrumbsState } = useBreadcrumbs();

  const requestsFilters = useState<Record<string, FilterSpecificationDto[] | undefined>>({});
  const commandsFilters = useState<Record<string, FilterSpecificationDto[] | undefined>>({});
  const activityLogsFilters = useState<Record<string, FilterSpecificationDto[] | undefined>>({});

  const requestsSelectedDate = useState<string | undefined>(undefined);
  const commandsSelectedDate = useState<string | undefined>(undefined);
  const activityLogsSelectedDate = useState<string | undefined>(undefined);

  const activityLogsSelectedTypes = useState<string[]>([]);

  const selectedSiteState = useLocalStorage<string>("activecontrol:site-selector", "All sites|allSites");// useState<string>("allSites");
  const selectedSite = useMemo(() => selectedSiteState[0].split("|")[1] ?? "allSites", [selectedSiteState]);

  return (
    <AppContext.Provider
      value={{
        breadCrumbsState,
        selectedSiteState,
        selectedSite,
        requestsFilters,
        requestsSelectedDate,
        commandsFilters,
        commandsSelectedDate,
        activityLogsFilters,
        activityLogsSelectedDate,
        activityLogsSelectedTypes,
      }}
    >
      <>{children}</>
    </AppContext.Provider>
  );
}
