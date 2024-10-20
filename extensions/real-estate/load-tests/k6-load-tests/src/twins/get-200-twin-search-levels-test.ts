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
  getCustomerLevels(data);
};


export function getCustomerLevels(data:any, sleepTime: number = SLEEP_DURATION) {
  group('get all customer levels', () => {

    // Get all customer levels
    const get_customer_levels_response = http.get(
      __ENV.API_BASEURL + '/twins/search?modelId=dtmi%3Acom%3Awillowinc%3ALevel%3B1',
      data.params
    );
    check(get_customer_levels_response, {
      'is status 200': (r) => r.status === 200
    });

    sleep(sleepTime);
  });
}