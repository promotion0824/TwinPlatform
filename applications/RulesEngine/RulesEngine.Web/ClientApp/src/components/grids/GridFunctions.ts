import { getGridStringOperators, getGridNumericOperators, getGridSingleSelectOperators, GridFilterModel, GridSortModel, GridFilterOperator, GridFilterItem, GridColDef, GridCellParams, getGridDateOperators } from '@mui/x-data-grid-pro';
import { FileResponse, FilterSpecificationDto, SortSpecificationDto } from '../../Rules';
import env from '../../services/EnvService';

export function gridPageSizes() {
  return [10, 20, 50, 100, 500, 1000];
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
 * date operators allowed for querying dates
 * @returns
 */
export function dateTimeOperators() {
  const operators = getGridDateOperators(true).filter(({ value }) =>
    ['after', 'onOrAfter', 'before', 'onOrBefore'].includes(value));

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
 * Operators allowed for querying single select values from a collection
 * @returns 
 */
export function singleSelectCollectionOperators() {
  const customOperators: GridFilterOperator<any, any, any>[] = [];
  const isOperator = getGridSingleSelectOperators().find(v => v.value === "is");

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
    InputComponentProps: isOperator?.InputComponentProps
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
    InputComponentProps: isOperator?.InputComponentProps
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
  const operators = getGridSingleSelectOperators().filter(({ value }) =>
    ['is'].includes(value));

  return operators;
}

/**
 * Maps MUI grid sort model to rules engine dto
 * @param model
 * @returns
 */
export function mapSortSpecifications(model: GridSortModel): SortSpecificationDto[] {
  const sort: SortSpecificationDto[] = model.map(x => {
    const result = new SortSpecificationDto();
    result.field = x.field;
    result.sort = x.sort?.toString();
    return result;
  });

  return sort;
}

/**
 * Converts MUI grid filter model to our filter Dto model
 * @param model The MUI filter model
 * @returns Rules engine DTO
 */
export function mapFilterSpecifications(model: GridFilterModel): FilterSpecificationDto[] {
  const logicalOperator = (model.logicOperator ?? "AND").toUpperCase();
  const filter: FilterSpecificationDto[] = model.items.filter(x =>
    (x.value === 'any' && x.operator === 'is') //other values for 'is' operator is essentially 'no filtering'
    || (x.value !== '' && x.value !== undefined) //ignore entries with no value
    || x.operator === 'isEmpty'//no value required
    || x.operator === 'isNotEmpty'//no value required
  ).map(x => {
    const result = new FilterSpecificationDto();
    result.field = x.field;
    result.logicalOperator = logicalOperator;
    result.operator = x.operator ?? '=';
    result.value = x.value;
    return result;
  });

  return filter;
}

export function createCsvFileResponse(data: any, fileName: string): Promise<FileResponse> {
  var arrData = data;
  var CSV = '';

  var row = "";

  //This loop will extract the label from 1st index of on array
  for (var index in arrData[0]) {
    //Now convert each value to string and comma-seprated
    row += index + ',';
  }
  row = row.slice(0, -1);
  //append Label row with line break
  CSV += row + '\r\n';

  //1st loop is to extract each row
  for (var i = 0; i < arrData.length; i++) {
    var row = "";
    //2nd loop will extract each column and convert it in string comma-seprated
    for (var index in arrData[i]) {
      row += '"' + arrData[i][index] + '",';
    }
    row.slice(0, row.length - 1);
    //add a line break after each row
    CSV += row + '\r\n';
  }

  var csv = CSV;
  const blob = new Blob([csv], { type: 'text/csv' });

  fileName = fileName.replace(';', '');

  const fileResponse: FileResponse = {
    data: blob,
    status: 200,
    fileName: fileName
  };

  Promise.resolve<FileResponse>(fileResponse);

  return Promise.resolve<FileResponse>(fileResponse);
}

/**
 * Generates/adds to cache key the customer id
 */
export function buildCacheKey(cacheKey: string): string {
  const customerId = env.customerId();

  return `${customerId}_${cacheKey}`;
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
