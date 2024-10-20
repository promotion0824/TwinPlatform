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
  getCustomerBuildings(data);
};


export function getCustomerBuildings(data:any, sleepTime: number = SLEEP_DURATION) {
  group('get all customer buildings', () => {

    // Get all customer buildings
    const get_customer_buildings_response = http.get(
      __ENV.API_BASEURL + '/twins/search?modelId=dtmi%3Acom%3Awillowinc%3ABuilding%3B1',
      data.params
    );
    check(get_customer_buildings_response, {
      'is status 200': (r) => r.status === 200
    });

    sleep(sleepTime);
  });
}