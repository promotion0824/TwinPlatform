import { sleep, check, group } from 'k6';
import { Options } from 'k6/options';
import http from 'k6/http';

import { login, SLEEP_DURATION, FLOOR_ID, SITE_ID } from "../";

export let options:Options = {
  vus: 1,
  duration: '5s'
};
export function setup() {
  return login();
}

export default (data:any) => {
  getFloorLayerGroups(data);
};

export function getFloorLayerGroups(data:any, sleepTime: number = SLEEP_DURATION) {
  group('get floor layergroups', () => {

    // Get a list of layergroups for a floor
    const get_floor_layergroups_response = http.get(
      __ENV.API_BASEURL + '/sites/' + SITE_ID + '/floors/' + FLOOR_ID + '/layerGroups',
      data.params
    );

    check(get_floor_layergroups_response, {
      'is status 200': (r) => r.status === 200
    });

    sleep(sleepTime);
  });
}