import { FilterSpecificationDto } from "../services/Clients";

export function getSearchInputFilterSpecification(
  searchInput: string,
  fields: string[]
): FilterSpecificationDto[] {

  let ret = fields.map((field) => {
    const result = new FilterSpecificationDto();
    result.field = field;
    result.logicalOperator = "OR";
    result.operator = "contains";
    result.value = searchInput;

    return result;
  });

  return ret;
}
