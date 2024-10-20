import { sleep, check, group } from 'k6';
import { Options } from 'k6/options';
import http from 'k6/http';

import { login, SLEEP_DURATION, SITE_ID, TICKET_ID  } from "../";

export let options:Options = {
  vus: 1,
  duration: '5s'
};
export function setup() {
  return login();
}

export default (data:any) => {
  getManagedPortfolios(data);
};


export function getManagedPortfolios(data:any, sleepTime: number = SLEEP_DURATION) {
  group('get managed portfolios', () => {

    // Get managed portfolios
    const get_managed_portfolios_response = http.get(
      __ENV.API_BASEURL + '/management/managedPortfolios',
      data.params
    );

    check(get_managed_portfolios_response, {
      'is status 200': (r) => r.status === 200
    });

    sleep(sleepTime);
  });
}