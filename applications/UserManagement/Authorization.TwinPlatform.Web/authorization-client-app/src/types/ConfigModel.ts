
export type SPAConfigModel = {
  clientId: string,
  authority: string,
  knownAuthorities: string[],
  hostName: string,
  baseName: string,
  redirectUri: string,
  apiB2CScopes: string[],
};

export type AppInsightConfigModel = {
  connectionString: string,
};

export type WillowContext = {
  appVersion:string,
  environmentConfiguration: EnvironmentConfiguration,
  regionConfiguration: RegionConfiguration,
  stampConfiguration: StampConfiguration,
  customerInstanceConfiguration: CustomerInstanceConfiguration
}

export type EnvironmentConfiguration = {
  shortName: string
}

export type RegionConfiguration =  {
  shortName: string
}

export type StampConfiguration =  {
  name: string
}

export type CustomerInstanceConfiguration =  {
  customerSalesId: string,
  customerInstanceName: string,
  name: string,
  dnsSubDomain: string
}

export class ConfigModel {

  _spaConfig: SPAConfigModel = null!;
  _appInsightSettings: AppInsightConfigModel = null!;
  _willowContext: WillowContext = null!;
  assemblyVersion: string = null!;

  static MapModel(apiRow: any): ConfigModel {
    return apiRow as ConfigModel;
  }
}
