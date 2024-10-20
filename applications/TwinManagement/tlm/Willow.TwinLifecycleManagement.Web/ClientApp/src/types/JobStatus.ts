import { BaseAsyncJobDetails } from './AsyncJobDetails';

export type JobStatus = {
  jobId: string;

  details: BaseAsyncJobDetails;

  target: string[];

  entitiesError: any;

  processedEntities: number;

  totalEntities: number;

  createTime: Date;

  userEmail: string;

  twinsByModel: { [key: string]: number };

  entitiesId: string[];
};
