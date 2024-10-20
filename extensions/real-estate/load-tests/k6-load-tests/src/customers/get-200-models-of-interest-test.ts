import { sleep, check, group } from 'k6';
import { Options } from 'k6/options';
import http from 'k6/http';

import { login, SLEEP_DURATION, CUSTOMER_ID  } from "../";

export let options:Options = {
  vus: 1,
  duration: '5s'
};
export function setup() {
  return login();
}

export default (data:any) => {
  getModelsOfInterest(data);
};

export function getModelsOfInterest(data:any, sleepTime: number = SLEEP_DURATION) {
  group('get models of interest', () => {

    // Get customer models of interest
    const get_models_of_interest_response = http.get(
      __ENV.API_BASEURL + '/customers/' + CUSTOMER_ID + '/modelsOfInterest',
      data.params
    );

    check(get_models_of_interest_response, {
      'is status 200': (r) => r.status === 200
    });

    sleep(sleepTime);
  });
}