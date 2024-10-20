export type Application = {
  name?: string | null;
  countHealthy?: number;
  countDegraded?: number;
  countUnhealthy?: number;
  versions?: Array<string> | null;
};
