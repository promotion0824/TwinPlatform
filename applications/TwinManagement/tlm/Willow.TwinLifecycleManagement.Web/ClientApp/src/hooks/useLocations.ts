import useGetLocations from './useGetLocations';
import { INestedTwin } from '../services/Clients';

export default function useLocations() {
  const { data: locations = [], isLoading, isSuccess } = useGetLocations();
  return { data: new Location(locations), isLoading, isSuccess };
}

type LocationLookup = { [key: string]: INestedTwin };

export class Location {
  locationLookup: LocationLookup;

  constructor(locations: INestedTwin[]) {
    this.locationLookup = getLocationLookup(locations);
  }

  getLocationById(siteId: string): INestedTwin {
    return this.locationLookup[siteId];
  }
}

/**
 * Take a list of sites (nested twin) and return as a mapping indexed by the siteID.
 */
export function getLocationLookup(locationsResponse: INestedTwin[] = []): LocationLookup {
  const locations = flattenArray(locationsResponse);

  return Object.fromEntries(
    locations.map((locationItem) => {
      let siteID = locationItem?.twin?.['siteID'] || null;
      let uniqueID = locationItem?.twin?.['uniqueID'] || null;

      var id = siteID !== '' && siteID !== null ? siteID : uniqueID;
      return [id, locationItem];
    })
  );
}

// this function is used to flatten the nested twin object into a single array
function flattenArray(arr: INestedTwin[]) {
  return arr.reduce((result: INestedTwin[], nestedTwin) => {
    if (nestedTwin.hasOwnProperty('twin') && nestedTwin.twin) {
      result.push(nestedTwin);
    }

    if (nestedTwin.hasOwnProperty('children') && Array.isArray(nestedTwin.children)) {
      const childrenTwins = flattenArray(nestedTwin.children);
      result = result.concat(childrenTwins);
    }

    return result;
  }, []);
}
