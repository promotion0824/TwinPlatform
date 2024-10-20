import { createContext, useContext, useState } from 'react';

const initialState: any = {} ;

const StateContext = createContext(initialState);

export const PageStateProvider = ({ children }) => {
  const [pageState, setPageState] = useState(initialState);

  return (
    <StateContext.Provider value={{ pageState, setPageState }}>
      {children}
    </StateContext.Provider>
  );
};

export const useStateContext = () => useContext(StateContext);
