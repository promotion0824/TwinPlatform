import { ApplicationInsights } from '@microsoft/applicationinsights-web';
import { ReactPlugin } from '@microsoft/applicationinsights-react-js';
import { createBrowserHistory } from 'history';
import env from '../services/EnvService';

const browserHistory = createBrowserHistory();
const reactPlugin = new ReactPlugin();

const connectionString = env.appInsightsConnectionString();

const appInsights = new ApplicationInsights({
  config: {
    connectionString: connectionString,
    extensions: [reactPlugin],
    enableAutoRouteTracking: true,
    extensionConfig: {
      [reactPlugin.identifier]: { history: browserHistory },
    },
  },
});

if (connectionString) {
  // skipped if no app insights configured for local dev
  appInsights.loadAppInsights();
}

export { reactPlugin, appInsights };
