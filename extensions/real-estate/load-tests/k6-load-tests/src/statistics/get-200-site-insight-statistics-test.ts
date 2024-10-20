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
  getSiteInsightStatistics(data);
};

export function getSiteInsightStatistics(data:any, sleepTime: number = SLEEP_DURATION) {
  group('get site insight statistics', () => {

    // Get site insight statistics
    const get_site_insight_statistics_response = http.get(
      __ENV.API_BASEURL + '/statistics/insights/site/' + SITE_ID,
      data.params
    );

    check(get_site_insight_statistics_response, {
      'is status 200': (r) => r.status === 200
    });

    sleep(sleepTime);
  });
}