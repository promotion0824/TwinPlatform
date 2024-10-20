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
  getMe(data);
};


export function getMe(data:any, sleepTime: number = SLEEP_DURATION) {
  group('get user details', () => {

    // Get user details
    const response = http.get(
      __ENV.API_BASEURL + '/me',
      data.params
    );
    check(response, {
      'is status 200': (r) => r.status === 200
    });

    sleep(sleepTime);
  });
}