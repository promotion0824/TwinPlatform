export type AsyncJobDetails = BaseAsyncJobDetails & {
  startTime: Date;

  endTime: Date;

  statusMessage: string;
};

export type BaseAsyncJobDetails = {
  status: string;
};
