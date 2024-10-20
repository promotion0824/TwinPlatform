import { FilterSpecificationDto } from "../services/Clients";

export function getFilterSpecification(
  field: string,
  logicalOperator: string,
  operator: string,
  value: any
) {
  const filter = new FilterSpecificationDto();
  filter.field = field;
  filter.logicalOperator = logicalOperator;
  filter.operator = operator;
  filter.value = value;
  return filter;
}
