import { sleep, check, group } from 'k6';
import { Options } from 'k6/options';
import http from 'k6/http';

import { login, SLEEP_DURATION, SITE_ID  } from "../";

export let options:Options = {
  vus: 1,
  duration: '5s'
};
export function setup() {
  return login();
}

export default (data:any) => {
  getSiteFloorsWithModule(data);
};

export function getSiteFloorsWithModule(data:any, sleepTime: number = SLEEP_DURATION) {
  group('get site floors with module', () => {

    // Get site floors with base module
    const get_site_floors_with_module_response = http.get(
      __ENV.API_BASEURL + '/sites/' + SITE_ID + '/floors?hasBaseModule=true',
      data.params
    );

    check(get_site_floors_with_module_response, {
      'is status 200': (r) => r.status === 200
    });

    sleep(sleepTime);
  });
}