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
  getSiteSigmaEmbedUrl(data);
};

export function getSiteSigmaEmbedUrl(data:any, sleepTime: number = SLEEP_DURATION) {
  group('get site sigma embedurl', () => {

    // get site sigma embedurl
    const get_site_sigma_embedurl_response = http.get(
      __ENV.API_BASEURL + '/sigma' + '/sites/' + SITE_ID + '/embedurl?reportId=70d5a6dc-a99f-4e52-04be-08daa0325e04&reportName=BUILDING%20SUMMARY&start=2022-09-01T00:00&end=2022-09-30T23:59',
      data.params
    );

    check(get_site_sigma_embedurl_response, {
      'is status 200': (r) => r.status === 200
    });

    sleep(sleepTime);
  });
}