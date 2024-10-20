export type ValidateExpressionType = {
  expression: string
};

export class ValidateExpressionModel implements ValidateExpressionType {
  expression: string = '';

  constructor(inputExp: string) {
    this.expression = inputExp;
  }
}
