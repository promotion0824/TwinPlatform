import { GridFilterModel, GridSortModel } from "@willowinc/ui";

export class BatchRequestDto {
  sortSpecifications: SortSpecificationDto[];
  filterSpecifications: FilterSpecificationDto[];
  page: number;
  pageSize: number;
  constructor(); // no arguments
  constructor(filterSpecifications?: FilterSpecificationDto[], sortSpecifications?: SortSpecificationDto[], page?: number, pageSize?: number);

  constructor(filterSpecifications?: FilterSpecificationDto[], sortSpecifications?: SortSpecificationDto[], page?: number, pageSize?: number) {
    this.sortSpecifications = sortSpecifications ?? [];
    this.filterSpecifications = filterSpecifications ?? [];
    this.page = page ?? 0;
    this.pageSize = pageSize ?? 100;
  }
}


export class SortSpecificationDto {
  field: string;
  sort?: string | undefined | null;

  constructor(field: string, sort?: string| undefined | null) {
    this.field = field;
    this.sort = sort;
  }

  static MapFrom(gridSortModel: GridSortModel) {
    return gridSortModel.map(m => new SortSpecificationDto(m.field, m.sort));
  }
}

export class FilterSpecificationDto {
  operator!: string;
  field: string;
  logicalOperator?: string | undefined;
  value?: any | undefined;
  isQuickFilter: boolean;

  constructor(field: string, operator: string, value: any, logicalOperator: string | undefined, isQuickFilter?:boolean) {
    this.field = field;
    this.operator = FilterSpecificationDto.ConvertToKnowOperator(operator);
    this.value = value ?? '';
    this.logicalOperator = logicalOperator;
    this.isQuickFilter = isQuickFilter ?? false;
  }

  static MapFrom(filterPropertyModel: GridFilterModel) {
    return filterPropertyModel.items.map(m => new FilterSpecificationDto(m.field, m.operator, m.value, filterPropertyModel.logicOperator));
  }

  static ConvertToKnowOperator(op: string) {
    if (op === 'isAnyOf')
      return 'in';
    else
      return op;
  }

}
