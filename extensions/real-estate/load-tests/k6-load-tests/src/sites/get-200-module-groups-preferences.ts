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
  getModuleGroupsPreferences(data);
};

export function getModuleGroupsPreferences(data:any, sleepTime: number = SLEEP_DURATION) {
  group('get module groups preferences', () => {

    // Get module groups preferences
    const get_module_groups_preferences_response = http.get(
      __ENV.API_BASEURL + '/sites/' + SITE_ID + '/preferences/moduleGroups',
      data.params
    );

    check(get_module_groups_preferences_response, {
      'is status 200': (r) => r.status === 200
    });

    sleep(sleepTime);
  });
}