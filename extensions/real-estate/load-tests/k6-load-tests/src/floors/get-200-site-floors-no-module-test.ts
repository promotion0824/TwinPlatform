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
  getSiteFloorsNoModule(data);
};

export function getSiteFloorsNoModule(data:any, sleepTime: number = SLEEP_DURATION) {
  group('get site floors no module', () => {

    // Get site floors with NO base module
    const get_site_floors_no_module_response = http.get(
      __ENV.API_BASEURL + '/sites/' + SITE_ID + '/floors?hasBaseModule=false',
      data.params
    );

    check(get_site_floors_no_module_response, {
      'is status 200': (r) => r.status === 200
    });

    sleep(sleepTime);
  });
}