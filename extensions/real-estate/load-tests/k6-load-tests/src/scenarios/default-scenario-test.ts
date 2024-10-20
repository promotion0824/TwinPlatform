import { Options } from "k6/options";
import { jUnit, textSummary } from "../util/k6-summary-0.0.1";


import { login  } from "../";
export * from "../";


export const options: Options = {
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<15000"],
  },
  scenarios: {
    get_all_sites_for_user: {
      executor: "per-vu-iterations",
      exec: "getAllSites",
      vus: 5,
      iterations: 20,
      tags: {
        test_type: "api",
        test_target: "portalxl"
      },
    },
    get_sites_connectivity: {
      executor: "per-vu-iterations",
      exec: "getSitesConnectivity",
      vus: 5,
      iterations: 20,
      tags: {
        test_type: "api",
        test_target: "portalxl"
      },
    },
    get_site_connectors: {
      executor: "per-vu-iterations",
      exec: "getSiteConnectors",
      vus: 5,
      iterations: 20,
      tags: {
        test_type: "api",
        test_target: "portalxl"
      },
    },
  },
};
export function setup() {
  return login();
}

export function handleSummary(data: any) {
  console.log("Preparing the end-of-test summary...");
  return {
    stdout: textSummary(data, { indent: "", enableColors: true }),
    "./loadtest-result.xml": jUnit(data),
  };
}
