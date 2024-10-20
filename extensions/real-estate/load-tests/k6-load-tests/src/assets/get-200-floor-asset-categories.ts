import { sleep, check, group } from 'k6';
import { Options } from 'k6/options';
import http from 'k6/http';

import { login, SLEEP_DURATION, SITE_ID, FLOOR_ID  } from "../";

export let options:Options = {
  vus: 1,
  duration: '5s'
};
export function setup() {
  return login();
}

export default (data:any) => {
  getFloorAssetCategories(data);
};

export function getFloorAssetCategories(data:any, sleepTime: number = SLEEP_DURATION) {
  group('get floor asset categories', () => {

    // Get floor asset categories
    const get_floor_asset_categories_response = http.get(
      __ENV.API_BASEURL + '/sites/' + SITE_ID + '/assets/categories?floorId=' + FLOOR_ID,
      data.params
    );
    //console.log(get_floor_asset_categories_response);
    check(get_floor_asset_categories_response, {
      'is status 200': (r) => r.status === 200
    });

    sleep(sleepTime);
  });
}