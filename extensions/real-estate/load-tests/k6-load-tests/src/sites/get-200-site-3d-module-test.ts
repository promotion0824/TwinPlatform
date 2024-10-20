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
  getSite3dModule(data);
};

export function getSite3dModule(data:any, sleepTime: number = SLEEP_DURATION) {
  group('get site 3d module', () => {

    // Get site 3d module
    const get_site_3d_module_response = http.get(
      __ENV.API_BASEURL + '/sites/' + SITE_ID + '/module',
      data.params
    );

    check(get_site_3d_module_response, {
      'is status 200': (r) => r.status === 200
    });

    sleep(sleepTime);
  });
}