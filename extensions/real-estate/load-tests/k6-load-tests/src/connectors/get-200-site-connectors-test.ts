import { sleep, check, group } from 'k6';
import { Options } from 'k6/options';
import http from 'k6/http';

import { login, SLEEP_DURATION,SITE_ID  } from "../";

export let options:Options = {
  vus: 5,
  duration: '10s'
};
export function setup() {
  return login();
}

export default (data:any) => {
  getSiteConnectors(data);
};


export function getSiteConnectors(data: any, sleepTime: number = SLEEP_DURATION) {
  group('get connectors for site', () => {

    // Get connectors for site
    const get_site_connectors = http.get(
      __ENV.API_BASEURL + '/sites/' + SITE_ID + '/connectors',
      data.params
    );

    check(get_site_connectors, {
      'is status 200': (r) => r.status === 200
    });

    sleep(sleepTime);
  })
}
