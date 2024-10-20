import { WillowContext } from "./types/ConfigModel";

export const WillowAppContext = {
  EnvName: '',
  RegionName: '',
  StampName: '',
  CustomerInstanceName: '',
  AppVersion: '',

  FullName: ''
};

export const UpdateWillowContext = (cont: WillowContext) => {

  WillowAppContext.EnvName = cont.environmentConfiguration?.shortName ?? '';
  WillowAppContext.RegionName = cont.regionConfiguration?.shortName ?? '';
  WillowAppContext.CustomerInstanceName = cont.customerInstanceConfiguration?.customerInstanceName ?? '';
  WillowAppContext.StampName = cont.stampConfiguration.name ?? '';
  WillowAppContext.AppVersion = cont.appVersion ?? '';

  WillowAppContext.FullName = checkNullOrUndefinedOrEmpty(WillowAppContext.EnvName,'??') + ' : ' +
    checkNullOrUndefinedOrEmpty(WillowAppContext.RegionName, '??') + ' : ' +
    checkNullOrUndefinedOrEmpty(WillowAppContext.StampName, '??') + ' : ' +
    checkNullOrUndefinedOrEmpty(WillowAppContext.CustomerInstanceName, '??');
};

// Todo:: Move this function to an Util Module
function checkNullOrUndefinedOrEmpty(value: any,defaultValue:string) {
  if (value === null || value === undefined || value === '') {
    return defaultValue;
  }

  return value;
}
