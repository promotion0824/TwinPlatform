type EnvironmentConfiguration = {
  shortName: string;
}

type RegionConfiguration = {
  shortName: string;
}

type StampConfiguration = {
  name: string;
}

type CustomerInstanceConfiguration = {
  customerSalesId: string,
  customerInstanceName: string,
  name: string,
  dnsSubDomain: string
}

export type WillowContext = {
  environmentConfiguration: EnvironmentConfiguration;
  regionConfiguration: RegionConfiguration;
  stampConfiguration: StampConfiguration;
  customerInstanceConfiguration: CustomerInstanceConfiguration;
}
