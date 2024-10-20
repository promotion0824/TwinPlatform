import { sleep, check, group } from 'k6';
import { Options } from 'k6/options';
import http from 'k6/http';

import { login, SLEEP_DURATION  } from "../";

export let options:Options = {
  vus: 10,
  duration: '10s'
};
export function setup() {
  return login();
}

export default (data:any) => {
  getSitesConnectivity(data);
};


export function getSitesConnectivity(data:any, sleepTime: number = SLEEP_DURATION) {
  group('get sites connectivity', () => {

    // Get sites connectivity
    const get_connectivity_response = http.get(
      __ENV.API_BASEURL + '/connectivity',
      data.params
    );
    
    check(get_connectivity_response, {
      'is status 200': (r) => r.status === 200
    });

    sleep(sleepTime);
  });
}