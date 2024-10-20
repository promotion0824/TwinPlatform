import { CustomerInstanceState, OverallState } from "../generated";

/*
* Gets the apps for one customer instance filtered to just the primary ones
*/

const usePrimaryApps = (overallState: OverallState, customerInstanceState: CustomerInstanceState) => {

  const domain = customerInstanceState.customerInstance?.domain!;

  const apps = overallState.applicationInstances?.filter(app => app.isPrimary
    && app.domain === domain
    && app.customerInstanceCode === customerInstanceState.customerInstance?.customerInstanceCode) ?? [];

  return apps.map(app => ({ name: app.applicationName, url: app.url, state: app.health?.status! }));
}

export default usePrimaryApps;
