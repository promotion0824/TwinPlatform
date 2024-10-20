/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ApplicationInstance } from './ApplicationInstance';
import type { CustomerInstanceState } from './CustomerInstanceState';
import type { HealthStatus } from './HealthStatus';
export type OverallState = {
    status?: HealthStatus;
    readonly customerInstances?: Array<CustomerInstanceState> | null;
    readonly applicationInstances?: Array<ApplicationInstance> | null;
};

