import { sleep, check, group } from 'k6';
import { Options } from 'k6/options';
import http from 'k6/http';

import { login, SLEEP_DURATION  } from "../";

export let options:Options = {
  vus: 1,
  duration: '5s'
};
export function setup() {
  return login();
}

export default (data:any) => {
  getAllSites(data);
};


export function getAllSites(data:any, sleepTime: number = SLEEP_DURATION) {
  group('get all sites for user', () => {

    // Get all sites
    const get_sites_response = http.get(
      __ENV.API_BASEURL + '/me/sites',
      data.params
    );
    check(get_sites_response, {
      'is status 200': (r) => r.status === 200
    });

    sleep(sleepTime);
  });
}