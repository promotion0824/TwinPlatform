import { sleep, check, group } from 'k6';
import { Options } from 'k6/options';
import http from 'k6/http';

import { login, SITE_ID, SLEEP_DURATION  } from "../";

export let options:Options = {
  vus: 1,
  duration: '5s'
};
export function setup() {
  return login();
}

export default (data:any) => {
  getSiteInsights(data);
};

export function getSiteInsights(data:any, sleepTime: number = SLEEP_DURATION) {
  group('get site insights', () => {

    // Get site insights
    const get_site_insights_response = http.get(
      __ENV.API_BASEURL + '/sites/' + SITE_ID + '/insights?tab=open',
      data.params
    );
    check(get_site_insights_response, {
      'is status 200': (r) => r.status === 200
    });

    sleep(sleepTime);
  });
}