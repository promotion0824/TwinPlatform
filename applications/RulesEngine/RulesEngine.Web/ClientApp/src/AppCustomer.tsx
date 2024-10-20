import { Route, Routes } from 'react-router-dom';
import Layout from './components/Layout';
import { lazy, Suspense } from 'react';
import env from './services/EnvService';
import { withErrorBoundary } from 'react-error-boundary'
import { ErrorFallback } from './components/error/errorBoundary';
// route-based code splitting to reduce bundle sizes and load faster

const AdminPage = lazy(() => import('./pages/AdminPage'));
const InsightsPage = lazy(() => import('./pages/InsightsPage'));
const InsightPage = lazy(() => import('./pages/InsightPage'));
const CommandsPage = lazy(() => import('./pages/CommandsPage'));
const CommandPage = lazy(() => import('./pages/CommandPage'));
const EquipmentPage = lazy(() => import('./pages/EquipmentPage'));
const ModelPage = lazy(() => import('./pages/ModelPage'));
const ModelsPage = lazy(() => import('./pages/ModelsPage'));
const GlobalVariablesPage = lazy(() => import('./pages/GlobalVariablesPage'));
const GlobalSingle = lazy(() => import('./pages/GlobalVariableSingle'));
const MLModelsPage = lazy(() => import('./pages/MLModelsPage'));
const MLModelSingle = lazy(() => import('./pages/MLModelSingle'));
const Rules = lazy(() => import('./pages/Rules'));
const RuleSingle = lazy(() => import('./pages/RuleSingle'));
const RuleCreatePage = lazy(() => import('./pages/RuleCreatePage'));
const SearchPage = lazy(() => import('./pages/SearchPage'));
const RuleInstancesPage = lazy(() => import('./pages/RuleInstancesPage'));
const RuleInstancePage = lazy(() => import('./pages/RuleInstancePage'));
const CalculatedPointPage = lazy(() => import('./pages/CalculatedPointPage'));
const CalculatedPointsPage = lazy(() => import('./pages/CalculatedPointsPage'));
const TimeSeriesPage = lazy(() => import('./pages/CapabilitySummariesPage'));
const Home = lazy(() => import('./pages/Home'));

export default withErrorBoundary((_props: any) => {
  return (
    <Layout>
      <Suspense fallback={<div>Loading...</div>}>
        <Routes>
          <Route path="/rules" element={<Rules />} />
          <Route path="/rule/:id" element={<RuleSingle />} />
          <Route path="/rulecreate" element={<RuleCreatePage />} />
          <Route path="/rulecreate/:id" element={<RuleCreatePage />} />
          <Route path="/globals" element={<GlobalVariablesPage />} />
          <Route path="/global/:id" element={<GlobalSingle />} />
          <Route path="/mlmodels" element={<MLModelsPage />} />
          <Route path="/mlmodel/:id" element={<MLModelSingle />} />
          <Route path="/calculatedPoints" element={<CalculatedPointsPage />} />
          <Route path="/calculatedPoint/:id" element={<CalculatedPointPage />} />
          <Route path="/search" element={<SearchPage />} />
          <Route path="/ruleInstances" element={<RuleInstancesPage />} />
          <Route path="/ruleInstance/:id" element={<RuleInstancePage />} />
          <Route path="/equipment" element={<EquipmentPage />}>
            <Route path=":id" element={<EquipmentPage />} />
            <Route path=":id/:previous1Id" element={<EquipmentPage />} />
            <Route path=":id/:previous1Id/:previous2Id" element={<EquipmentPage />} />
          </Route>
          <Route path="/equipment/:id/:previous1Id" element={<EquipmentPage />} />
          <Route path="/equipment/:id/:previous1Id/:previous2Id" element={<EquipmentPage />} />
          <Route path="/insight/:id" element={<InsightPage />} />
          <Route path="/insights/:id" element={<InsightsPage />} />
          <Route path="/command/:id" element={<CommandPage />} />
          <Route path="/commands" element={<CommandsPage />} />
          <Route path="/models" element={<ModelsPage />} />
          <Route path="/model/:id" element={<ModelPage />} />
          <Route path="/timeseries" element={<TimeSeriesPage />} />
          <Route path="/timeseries/:id" element={<TimeSeriesPage />} />
          <Route path="/admin" element={<AdminPage />} />
          <Route path="/" element={<Home />} />
          <Route path="*" element={<ErrorFallback />} />
        </Routes>
      </Suspense>
    </Layout>
  );
}, {
  FallbackComponent: ErrorFallback, //to test fallback on child component just throw an error inside it
  onError(error, info) {
    console.log('in router', error, info)
  },
}
);
