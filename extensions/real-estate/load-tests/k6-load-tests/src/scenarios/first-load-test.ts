import { Options } from "k6/options";
import { sleep, group } from 'k6';
import { jUnit, textSummary } from "../util/k6-summary-0.0.1";
import { getAllUserLogin  } from "../authentication";
import { getAllSites, getSitesConnectivity, getInsights, getMe, getMetrics, SLEEP_DURATION} from "../";
// @ts-ignore
import { scenario } from "k6/execution";

export const options: Options = {
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<5000"],
  },
  scenarios: {
    initial_dashboard_load_and_navigation: {
      executor: "constant-vus",
      exec: "firstLoad",
      vus: 5,
      duration: __ENV.DURATION || '30s',
      tags: {
        test_type: "api",
        test_target: "portalxl"
      },
    }
  },
};

export function setup() {
  return getAllUserLogin();
}


export function firstLoad(data: any[]) {
  const loginData = data[scenario.iterationInTest % data.length];

  const testData = {
    ...data,
    ...loginData
};

  group('load dashboard and initial navigation', function () {
    group('first tab', function () {
      getMe(testData, 0);
      getAllSites(testData, 0);
      getMetrics(testData, 0);
      getSitesConnectivity(testData, 0);
      sleep(SLEEP_DURATION);
    });
    group('second tab', function () {
      getSitesConnectivity(testData, 0);
      sleep(SLEEP_DURATION);
    });
    group('third tab', function () {
      getInsights(testData, 0);
      sleep(SLEEP_DURATION);
    });
  });
}


export function handleSummary(data: any) {
  console.log("Preparing the end-of-test summary...");
  return {
    stdout: textSummary(data, { indent: "", enableColors: true }),
    "./loadtest-result.xml": jUnit(data),
  };
}
