import { ReactNode, createContext, useContext } from "react";
import { useSearchParams } from "react-router-dom";
import { Link } from 'react-router-dom';
import { CustomerInstanceState, LifeCycleState } from "../generated";


export enum HybridType {
  All = "All",
  Hybrid = "Hybrid",
  NonHybrid = "Non-hybrid"
}

/*
* Application state tracked by query parameters
*/
interface IApplicationState {
  prod: boolean,
  dev: boolean,
  sortAlpha: boolean
  products: string[]
  regions: string[]
}

const ApplicationContext = createContext<IApplicationState>({ prod: true, dev: true, sortAlpha: false, products: [], regions: [] });

export const ApplicationContextProvider = (props: { children: any }) => {

  const [searchParams] = useSearchParams();
  const app: IApplicationState =
  {
    dev: !!searchParams.get('dev'),
    prod: !!searchParams.get('prod'),
    sortAlpha: !!searchParams.get('sortalpha'),
    products: searchParams.getAll('product') ?? [],
    regions: searchParams.getAll('region') ?? []
  };

  // Force allowed states to not be empty
  if (!app.dev && !app.prod) app.prod = true;

  return (
    <ApplicationContext.Provider value={app}>
      {props.children}
    </ApplicationContext.Provider>
  );
};

/*
* Gets the application state as defined by the query parameters
*/
export const useApplicationContext: () => IApplicationState = () => useContext(ApplicationContext);

/*
* Method to generate the full query string for the current application state
*/
export const generateQueryString = (state: IApplicationState) => {

  const params = new URLSearchParams();
  state.prod ? params.append("prod", state.prod.toString()) : null;
  state.dev ? params.append("dev", state.dev.toString()) : null;
  state.sortAlpha ? params.append("sortalpha", state.sortAlpha.toString()) : null;
  state.products.map(p => params.append("product", p));
  state.regions.map(r => params.append("region", r));
  return params.toString();
}

/*
* Link component that preserves the current application state query parameter
*/
export const LinkWithState = (props: { to: string, children: ReactNode }) => {
  const state = useApplicationContext();
  const toWithQuery = props.to + "?" + generateQueryString(state);
  return (<Link to={toWithQuery}>{props.children}</Link>);
}

/*
* Filter customer instances according to the app state
*/
export const filterLifecycleState = (app: IApplicationState) => (x: CustomerInstanceState) =>

  x === null ? false :

    ((x.customerInstance?.lifeCycleState === LifeCycleState.LIVE && app.prod) ||
      (x.customerInstance?.lifeCycleState != LifeCycleState.LIVE && app.dev));
