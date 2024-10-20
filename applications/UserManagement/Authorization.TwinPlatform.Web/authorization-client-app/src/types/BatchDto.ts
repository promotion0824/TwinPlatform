export class BatchDto<T> {
  items: T[];
  total: number;

  constructor() {
    this.items = [];
    this.total = 0;
  }
}
