import { OverallState } from "../generated";
import compareStringsLexNumeric from "./AlphaNumericSorter";

const getVersionsForApp = (data: OverallState, isSingleTenant: boolean, appId: string): (string | null | undefined)[] => {
  if (!data) return [];
  const versions = [... new Set(data?.applicationInstances?.filter(x => x.applicationName === appId && x.isSingleTenant === isSingleTenant).map(x => x.health!.version))].filter(x => x).sort(compareStringsLexNumeric);
  return versions;
}

export const isLatestVersion = (data: OverallState, isSingleTenant: boolean, appId: string, version: string): boolean => {
  const versions = getVersionsForApp(data, isSingleTenant, appId);
  return versions.indexOf(version) === versions.length - 1;
};

export default getVersionsForApp;
