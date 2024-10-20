import { filterLifecycleState, useApplicationContext } from "../components/ApplicationContext";
import { GroupBy } from "../components/groupby";
import { ApplicationInstance } from "../generated";
import query from "./query";


/*
* Get the state filtered according to the query parameters set on the toolbar
*/
export const useFilteredState = () => {

  const { data, isFetched } = query();

  const app = useApplicationContext();

  if (isFetched) {
    console.log('data', data);
  }

  const filteredCustomerStates = data?.customerInstances?.filter(filterLifecycleState(app)) ?? [];
  const filteredCustomerInstances = filteredCustomerStates.map(x => x.customerInstance!);
  const filteredCustomerCodes = filteredCustomerInstances.map(x => x.customerInstanceCode!).filter(x => !!x) ?? [];

  const filteredApps = data?.applicationInstances?.filter(x => filteredCustomerCodes.indexOf(x.customerInstanceCode!) > -1) ?? [];

  const applications = GroupBy(filteredApps,
    (x: ApplicationInstance) => x.applicationName ?? "");

  if (isFetched) {
    console.log('customers', filteredCustomerStates);
    console.log('instances', filteredCustomerInstances);
    console.log('codes', filteredCustomerCodes);
  }


  return {
    data: data,
    isFetched: isFetched,
    app: app,
    filteredCustomersStates: filteredCustomerStates,
    filteredCustomerInstances: filteredCustomerInstances,
    filteredCustomerCodes: filteredCustomerCodes,
    filteredApps: filteredApps,
    applicationInstancesByName: applications
  };
}
