import { sleep, check, group } from 'k6';
import { Options } from 'k6/options';
import http from 'k6/http';

import { login, SLEEP_DURATION, PORTFOLIO_ID  } from "../";

export let options:Options = {
  vus: 1,
  duration: '5s'
};
export function setup() {
  return login();
}

export default (data:any) => {
  getPortfolioDashboard(data);
};

export function getPortfolioDashboard(data:any, sleepTime: number = SLEEP_DURATION) {
  group('get portfolio dashboard', () => {

    // Get portfolio dashboard
    const get_portfolio_dashboard_response = http.get(
      __ENV.API_BASEURL + '/portfolios/' + PORTFOLIO_ID + '/dashboard',
      data.params
    );

    check(get_portfolio_dashboard_response, {
      'is status 200': (r) => r.status === 200
    });

    sleep(sleepTime);
  });
}