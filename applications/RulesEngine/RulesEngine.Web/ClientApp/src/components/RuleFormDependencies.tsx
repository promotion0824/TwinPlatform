import { useEffect, useState } from 'react';
import { RuleDependencyDto, RuleDto } from '../Rules';
import RuleDependenciesTable from './grids/RuleDependenciesTable';

const RuleFormDependencies = (params: { rule: RuleDto, formContext: any, revision: number }) => {

  const [rule, setRule] = useState(params.rule);

  const updateDependencies = (dependencies: RuleDependencyDto[]) => {
    rule.init({ ...rule, dependencies: dependencies });
    setRule(rule);
  };

  // Whenever params.rule is invalidated, we need to refresh our copy.
  useEffect(() => {
    setRule(params.rule);
  }, [params.rule]);

  return (
    <RuleDependenciesTable rule={rule} updateDependencies={updateDependencies} revision={params.revision} />
  );
}

export default RuleFormDependencies;
