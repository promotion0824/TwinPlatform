import './App.css'
import { Grid } from '@mui/material';
import StatusIcon from './components/StatusIcon';
import PageWrapper from './components/PageWrapper';
import { PageTitle, PageTitleItem } from '@willowinc/ui';
import { LinkWithState } from './components/ApplicationContext';
import { useFilteredState } from './hooks/useFilteredState';
import CustomerCard from './components/CustomerCard';

const CustomersPage = () => {

  const filteredState = useFilteredState();

  if (!filteredState.isFetched) return <>Loading...</>;

  let customerInstances = filteredState.filteredCustomersStates.filter(x => !(x?.customerInstance?.isHybrid));

  if (filteredState.app.sortAlpha) {
    customerInstances.sort(function (x, y) {
      if (x.customerInstance!.name!.toLowerCase() < y.customerInstance!.name!.toLowerCase()) return -1;
      if (x.customerInstance!.name!.toLowerCase() > y.customerInstance!.name!.toLowerCase()) return 1;
      return 0;
    })
  }
  else {
    customerInstances.sort(function (x, y) {
      if (x.customerInstance!.deploymentPhase! < y.customerInstance!.deploymentPhase!) return -1;
      if (x.customerInstance!.deploymentPhase! > y.customerInstance!.deploymentPhase!) return 1;
      return 0;
    })
  }

  const filterProduct = (products: string[]) => {
    if (products.length == 1)
      switch (products[0]) {
        case "willow": return customerInstances.filter(x => !(x?.customerInstance?.isNewBuild));
        case "newbuild": return customerInstances.filter(x => x?.customerInstance?.isNewBuild);
        default: return customerInstances;
      }
    else return customerInstances
  }

  customerInstances = filterProduct(filteredState.app.products);

  customerInstances = filteredState.app.regions.length > 0 ? customerInstances.filter(x => filteredState.app.regions.indexOf(x.customerInstance?.region!) > -1) : customerInstances;

  const ciStatus = customerInstances.length > 0 ?
    customerInstances.reduce((previous, current) => current.status! < previous! ? current.status! : previous, customerInstances[0].status) : null;

  return (
    <PageWrapper>

      <PageTitle>
        <PageTitleItem>
          <LinkWithState to={"/"}>WSUP</LinkWithState>
        </PageTitleItem>
        <PageTitleItem>
          Customers
        </PageTitleItem>
      </PageTitle>

      <>
        <h1>Customer Instances <StatusIcon health={ciStatus!} size={22} /></h1>
        <div>
          <Grid container spacing={1}>
            {
              customerInstances.map(x => CustomerCard(filteredState.data!, x))
            }
          </Grid>
        </div>
      </>

    </PageWrapper >
  )
}

export default CustomersPage;
