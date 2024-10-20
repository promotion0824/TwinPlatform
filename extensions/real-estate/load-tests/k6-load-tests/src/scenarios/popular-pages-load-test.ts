import { Options } from "k6/options";
import { sleep, group } from 'k6';
import { jUnit, textSummary } from "../util/k6-summary-0.0.1";
import { getAllUserLogin  } from "../authentication";
import { getCustomerAssets, getCustomerBuildingComponents, getCustomerBuildings, getCustomerLevels, getCustomerRooms, getSiteTickets, getTicketDetails, getModelsOfInterest, getPortfolioDashboard, 
          getManagedPortfolios, getSiteFloorsWithModule, getSiteFloorsNoModule, getSiteInsights, getSiteTicketStatistics, getSiteInsightStatistics, getSite3dModule, getSiteDashboardData, 
          getSiteSigmaEmbedUrl, getInsights, getFloorLayerGroups, getModuleGroupsPreferences, getFloorAssetCategories, SLEEP_DURATION } from "../";
// @ts-ignore
import { scenario } from "k6/execution";

export const options: Options = {
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<5000"],
  },
  //Adding popular pages as scenarios gives us control over vuser numbers, iteration count, for each page//
  scenarios: {
    twin_search_and_explore_page: {
      executor: "constant-vus",
      exec: "twinSearchAndExplorePage",
      vus: 1,
      duration: __ENV.DURATION || '30s',
      tags: {
        test_type: "api",
        test_target: "portalxl"
      },
    },
    tickets_open_page: {
    executor: "constant-vus",
    exec: "ticketsOpenPage",
    vus: 2,
    duration: __ENV.DURATION || '30s',
    tags: {
      test_type: "api",
      test_target: "portalxl"
    },
  },
  ticket_details_page: {
    executor: "constant-vus",
    exec: "ticketDetailsPage",
    vus: 1,
    duration: __ENV.DURATION || '30s',
    tags: {
      test_type: "api",
      test_target: "portalxl"
    },
  },
  dashboard_all_buildings_page: {
    executor: "constant-vus",
    exec: "dashboardAllBuildingsPage",
    vus: 4,
    duration: __ENV.DURATION || '30s',
    tags: {
      test_type: "api",
      test_target: "portalxl"
    },
  },
  dashboard_floor_3d_page: {
    executor: "constant-vus",
    exec: "dashboardFloor3dPage",
    vus: 3,
    duration: __ENV.DURATION || '30s',
    tags: {
      test_type: "api",
      test_target: "portalxl"
    },
  },
  insights_open_page: {
    executor: "constant-vus",
    exec: "insightsOpenPage",
    vus: 1,
    duration: __ENV.DURATION || '30s',
    tags: {
      test_type: "api",
      test_target: "portalxl"
    },
  },
  dashboard_floor_2d_page: {
    executor: "constant-vus",
    exec: "dashboardFloor2dPage",
    vus: 1,
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

/////////////////twin search and explore page here////////////////
export function twinSearchAndExplorePage(data: any[]) {
  const loginData = data[scenario.iterationInTest % data.length];

  const testData = {
    ...data,
    ...loginData
};
    group('twin search and explore page', function () {
        getCustomerAssets(testData, 0);
        getCustomerBuildingComponents(testData, 0);
        getCustomerBuildings(testData, 0);
        getCustomerLevels(testData, 0);
        getCustomerRooms(testData, 0);
        sleep(SLEEP_DURATION);
    });
}

/////////////////tickets open page here////////////////////////
export function ticketsOpenPage(data: any[]) {
  const loginData = data[scenario.iterationInTest % data.length];

  const testData = {
    ...data,
    ...loginData
};
    group('tickets open page', function () {
        getSiteTickets(testData, 0);
        sleep(SLEEP_DURATION);
    });
}

/////////////////ticket details page here////////////////////////
export function ticketDetailsPage(data: any[]) {
  const loginData = data[scenario.iterationInTest % data.length];

  const testData = {
    ...data,
    ...loginData
};
    group('ticket details page', function () {
      getTicketDetails(testData, 0);
        sleep(SLEEP_DURATION);
    });
}

/////////////////dashboard all buildings page here////////////////////////
export function dashboardAllBuildingsPage(data: any[]) {
  const loginData = data[scenario.iterationInTest % data.length];

  const testData = {
    ...data,
    ...loginData
};
    group('dashboard all buildings page', function () {
        getModelsOfInterest(testData, 0);
        getPortfolioDashboard(testData, 0);
        getManagedPortfolios(testData, 0);
        sleep(SLEEP_DURATION);
    });
}

/////////////////dashboard floor 3d page here////////////////////////
export function dashboardFloor3dPage(data: any[]) {
  const loginData = data[scenario.iterationInTest % data.length];

  const testData = {
    ...data,
    ...loginData
};
    group('dashboard floor 3d page', function () {
      getSiteFloorsWithModule(testData, 0);
      getSiteFloorsNoModule(testData, 0);
      getSiteInsights(testData, 0);
      getSiteTickets(testData, 0);
      getSiteTicketStatistics(testData, 0);
      getSiteInsightStatistics(testData, 0);
      getSite3dModule(testData, 0);
      getSiteDashboardData(testData, 0);
      getSiteSigmaEmbedUrl(testData, 0);
      sleep(SLEEP_DURATION);
    });
}

/////////////////insights open page here////////////////////////
export function insightsOpenPage(data: any[]) {
  const loginData = data[scenario.iterationInTest % data.length];

  const testData = {
    ...data,
    ...loginData
};
    group('insights open page', function () {
      getSiteFloorsWithModule(testData, 0);
      getSiteInsights(testData, 0);
      getInsights(testData, 0);
      sleep(SLEEP_DURATION);
    });
}

/////////////////dashboard floor 2d page here////////////////////////
export function dashboardFloor2dPage(data: any[]) {
  const loginData = data[scenario.iterationInTest % data.length];

  const testData = {
    ...data,
    ...loginData
};
    group('dashboard floor 2d page', function () {
      getFloorLayerGroups(testData, 0);
      getModuleGroupsPreferences(testData, 0);
      getFloorAssetCategories(testData, 0);
      sleep(SLEEP_DURATION);
    });
}

export function handleSummary(data: any) {
  console.log("Preparing the end-of-test summary...");
  return {
    stdout: textSummary(data, { indent: "", enableColors: true }),
    "./loadtest-result.xml": jUnit(data),
  };
}
