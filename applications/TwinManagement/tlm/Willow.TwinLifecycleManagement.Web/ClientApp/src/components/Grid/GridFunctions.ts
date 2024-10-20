import {
  getGridStringOperators,
  getGridNumericOperators,
  getGridSingleSelectOperators,
  GridFilterModel,
  GridFilterOperator,
} from '@mui/x-data-grid-pro';
import { FilterSpecificationDto } from '../../services/Clients';

export function stringOperators() {
  const operators = getGridStringOperators().filter(({ value }) =>
    ['equals', 'contains', 'startsWith', 'endsWith', 'isEmpty', 'isNotEmpty'].includes(value)
  );
  return operators;
}

export function guidOperators() {
  const operators = getGridStringOperators().filter(({ value }) => ['equals'].includes(value));

  return operators;
}

/**
 * Numeric operators allowed for querying
 * @returns
 */
export function numberOperators() {
  const operators = getGridNumericOperators().filter(({ value }) => ['=', '!=', '>', '>=', '<', '<='].includes(value));

  return operators;
}

/**
 * Numeric operators allowed for querying double values
 * @returns
 */
export function doubleOperators() {
  const operators = getGridNumericOperators().filter(({ value }) => ['>', '<'].includes(value));

  return operators;
}

/**
 * Current operators allowed for querying single select values. 'isNot', 'isAnyOf' must be implemented on our filter Dto model
 * @returns
 */
export function singleSelectOperators() {
  const operators = getGridSingleSelectOperators().filter(({ value }) => ['is', 'not'].includes(value));

  return operators;
}

/**
 * Operators allowed for querying single select values from a collection
 * @returns
 */
export function singleSelectCollectionOperators() {
  const customOperators: GridFilterOperator<any, any, any>[] = [];
  const isOperator = getGridSingleSelectOperators().find((v) => v.value === 'is');

  const containsOperator: GridFilterOperator<any, any, any> = {
    label: 'contains',
    value: 'contains',
    getApplyFilterFn: (filterItem: any, column: any) => {
      const innerFilterFn = isOperator?.getApplyFilterFn(filterItem, column);
      if (!innerFilterFn) {
        return null;
      }
      return (params: any) => innerFilterFn(params);
    },
    InputComponent: isOperator?.InputComponent,
    InputComponentProps: isOperator?.InputComponentProps,
  };
  const notContainsOperator: GridFilterOperator<any, any, any> = {
    label: 'not contains',
    value: 'notcontains',
    getApplyFilterFn: (filterItem: any, column: any) => {
      const innerFilterFn = isOperator?.getApplyFilterFn(filterItem, column);
      if (!innerFilterFn) {
        return null;
      }
      return (params: any) => !innerFilterFn(params);
    },
    InputComponent: isOperator?.InputComponent,
    InputComponentProps: isOperator?.InputComponentProps,
  };
  customOperators.push(containsOperator);
  customOperators.push(notContainsOperator);

  return customOperators;
}

/**
 * Boolean operators
 * @returns
 */
export function boolOperators() {
  //Cannot filter getGridBooleanOperators because it overrides the valueOptions
  const operators = getGridSingleSelectOperators().filter(({ value }) => ['is'].includes(value));

  return operators;
}

// /**
//  * Maps MUI grid sort model to SortSpecificationDto
//  * @param GridSortModel
//  * @returns SortSpecificationDto[]
//  */
// export function mapSortSpecifications(model: GridSortModel): SortSpecificationDto[] {
//   const sort: SortSpecificationDto[] = model.map((x) => {
//     const result = new SortSpecificationDto();
//     result.field = x.field;
//     result.sort = x.sort?.toString();
//     return result;
//   });

//   return sort;
// }

/**
 * Converts MUI grid filter model to FilterSpecificationDto
 * @param model GridFilterModel
 * @returns FilterSpecificationDto[]
 */
export function mapFilterSpecifications(model: GridFilterModel): FilterSpecificationDto[] {
  const logicalOperator = (model.logicOperator ?? 'AND').toUpperCase();
  const filter: FilterSpecificationDto[] = model.items
    .filter(
      (x) =>
        (x.value === 'any' && x.operator === 'is') || //other values for 'is' operator is essentially 'no filtering'
        (x.value !== '' && x.value !== undefined) || //ignore entries with no value
        x.operator === 'isEmpty' || //no value required
        x.operator === 'isNotEmpty' //no value required
    )
    .map((x) => {
      const result = new FilterSpecificationDto();
      result.field = x.field;
      //result.logicalOperator = logicalOperator;
      result.operator = x.operator ?? '=';
      result.value = x.value;
      return result;
    });

  return filter;
}
