/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { HealthStatus } from './HealthStatus';
export type HealthCheckDto = {
    key?: string | null;
    status?: HealthStatus;
    description?: string | null;
    version?: string | null;
    entries?: Record<string, HealthCheckDto> | null;
    readonly entriesWithPayload?: Record<string, any> | null;
};

