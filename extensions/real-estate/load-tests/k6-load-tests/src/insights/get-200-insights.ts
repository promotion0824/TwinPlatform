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
  getInsights(data);
};

export function getInsights(data:any, sleepTime: number = SLEEP_DURATION) {
  group('get insights for all sites', () => {

    // Get all site open insights
    const response = http.get(
      __ENV.API_BASEURL + '/insights?tab=open',
      data.params
    );
    check(response, {
      'is status 200': (r) => r.status === 200
    });

    sleep(sleepTime);
  });
}