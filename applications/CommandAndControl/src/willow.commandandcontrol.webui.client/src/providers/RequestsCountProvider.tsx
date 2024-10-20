import {  createContext,  useContext,  useState,  useEffect,  PropsWithChildren } from "react";
import { useGetRequestedCommandsCount } from "../pages/RequestsPage/hooks/useGetRequestedCommandsCount";
import { UseState } from "../../types/UseState";

type RequestsCountContextType = {
  requestsCountState: UseState<number | string>;
};

const RequestsCountContext = createContext<RequestsCountContextType | undefined>(undefined);

export function useRequestsCountContext() {
  const context = useContext(RequestsCountContext);
  if (context == null) {
    throw new Error("useAppContext must be used within a AppContextProvider");
  }

  return context;
}

export const RequestsCountContextProvider: React.FC<PropsWithChildren<{}>> = ({ children }) => {

  const {
    data = 0,
    isSuccess,
    isError,
    isLoading,
  } = useGetRequestedCommandsCount({
    refetchInterval: 30000, // refetch every 30 seconds
  });

  // Used to inject requests count to navmenu requests tab
  const requestsCountState = useState<number | string>(0);

  useEffect(() => {
    if (isError || isLoading) requestsCountState[1]("?");
    else {
      requestsCountState[1](data);
    }
  }, [data, isSuccess, isError, isLoading]);

  return (
    <RequestsCountContext.Provider value={{ requestsCountState }}>
      {children}
    </RequestsCountContext.Provider>
  );
}
