import { getGridStringOperators, getGridNumericOperators, getGridSingleSelectOperators, GridFilterModel, GridSortModel, GridFilterOperator, GridFilterItem, GridColDef, GridCellParams } from '@mui/x-data-grid';

export interface ISortSpecificationDto {
  field: string;
  sort: string;
}

export interface IFilterSpecificationDto {
  field: string;
  operator: string;
  logicalOperator: string | undefined;
  value: any | undefined;
}


/** A batch request */
export interface IBatchRequestDto {
  /** Specifications on how to sort the batch */
  sortSpecifications?: ISortSpecificationDto[] | undefined;
  /** Specification on how to filter the batch */
  filterSpecifications?: IFilterSpecificationDto[] | undefined;
  /** The page number to return for the batch (one-based) */
  page?: number | undefined;
  /** The amount of items in the batch */
  pageSize?: number | undefined;
}

export function stringOperators() {

  const operators = getGridStringOperators().filter(({ value }) =>
    ['equals', 'contains', 'startsWith', 'endsWith', 'isEmpty', 'isNotEmpty'].includes(value));

  const containsOperator = operators.find(v => v.value == "contains");
  const notContainsOperator: GridFilterOperator<any, number | string | null, any> = {
    label: 'not contains',
    value: 'notcontains',
    getApplyFilterFn: (filterItem: GridFilterItem, column: GridColDef) => {
      const innerFilterFn = equalsOperator?.getApplyFilterFn(filterItem, column);
      if (!innerFilterFn) {
        return null;
      }
      return (params: GridCellParams<any, number | string | null, any>): boolean => {
        return !innerFilterFn(params);
      };
    },
    InputComponent: containsOperator?.InputComponent,
    InputComponentProps: containsOperator?.InputComponentProps
  };
  operators.push(notContainsOperator);

  const equalsOperator = operators.find(v => v.value == "equals");
  const notEqualsOperator: GridFilterOperator<any, number | string | null, any> = {
    label: 'not equals',
    value: 'notequals',
    getApplyFilterFn: (filterItem: GridFilterItem, column: GridColDef) => {
      const innerFilterFn = equalsOperator?.getApplyFilterFn(filterItem, column);
      if (!innerFilterFn) {
        return null;
      }
      return (params: GridCellParams<any, number | string | null, any>): boolean => {
        return !innerFilterFn(params);
      };
    },
    InputComponent: equalsOperator?.InputComponent,
    InputComponentProps: equalsOperator?.InputComponentProps
  };
  operators.push(notEqualsOperator);

  return operators;
}

export function guidOperators() {
  const operators = getGridStringOperators().filter(({ value }) =>
    ['equals'].includes(value));

  return operators;
}

/**
 * Numeric operators allowed for querying
 * @returns
 */
export function numberOperators() {
  const operators = getGridNumericOperators().filter(({ value }) =>
    ['=', '!=', '>', '>=', '<', '<='].includes(value));

  return operators;
}

/**
 * Numeric operators allowed for querying double values
 * @returns
 */
export function doubleOperators() {
  const operators = getGridNumericOperators().filter(({ value }) =>
    ['>', '<'].includes(value));

  return operators;
}

/**
 * Current operators allowed for querying single select values. 'isNot', 'isAnyOf' must be implemented on our filter Dto model
 * @returns
 */
export function singleSelectOperators() {
  const operators = getGridSingleSelectOperators().filter(({ value }) =>
    ['is', 'not'].includes(value));

  return operators;
}

/**
 * Boolean operators
 * @returns
 */
export function boolOperators() {
  //Cannot filter getGridBooleanOperators because it overrides the valueOptions
  const operators = getGridSingleSelectOperators().filter(({ value }) =>
    ['is'].includes(value));

  return operators;
}

/**
 * Maps MUI grid sort model to rules engine dto
 * @param model
 * @returns
 */
export function mapSortSpecifications(model: GridSortModel): ISortSpecificationDto[] {
  const sort: ISortSpecificationDto[] = model.map(x => {
    const result: ISortSpecificationDto = { field: x.field, sort: x.sort?.toString()! };
    return result;
  });

  return sort;
}

/**
 * Converts MUI grid filter model to our filter Dto model
 * @param model The MUI filter model
 * @returns Rules engine DTO
 */
export function mapFilterSpecifications(model: GridFilterModel): IFilterSpecificationDto[] {
  const logicalOperator = (model.logicOperator ?? "AND").toUpperCase();
  const filter: IFilterSpecificationDto[] = model.items.filter(x =>
    (x.value === 'any' && x.operator === 'is') //other values for 'is' operator is essentially 'no filtering'
    || (x.value !== '' && x.value !== undefined) //ignore entries with no value
    || x.operator === 'isEmpty'//no value required
    || x.operator === 'isNotEmpty'//no value required
  ).map(x => {
    const result: IFilterSpecificationDto = {
      field: x.field,
      logicalOperator: logicalOperator,
      operator: x.operator ?? '=',
      value: x.value
    };
    return result;
  });

  return filter;
}

/**
 * Stores gridFilterModel, gridSortModel state per user/browser
 */
export function setUserGridPreferences(gridId: string, gridFilterModel?: GridFilterModel, gridSortModel?: GridSortModel) {
  // console.log('gridName: ', gridId)
  // console.log('filterName: ', gridFilterModel)

  const gridStateModel = { ...gridFilterModel, ...gridSortModel }

  localStorage.setItem(gridId, JSON.stringify(gridStateModel));

}
