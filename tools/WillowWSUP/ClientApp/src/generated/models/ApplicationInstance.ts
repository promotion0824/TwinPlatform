/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DeploymentPhase } from './DeploymentPhase';
import type { HealthCheckDto } from './HealthCheckDto';
export type ApplicationInstance = {
    isPrimary?: boolean;
    region?: string | null;
    devOrPrd?: string | null;
    customerInstanceCode?: string | null;
    applicationName?: string | null;
    deploymentPhase?: DeploymentPhase;
    domain?: string | null;
    isSingleTenant?: boolean;
    url?: string | null;
    healthUrl?: string | null;
    cloudRoleName?: string | null;
    applicationInsightsLink?: string | null;
    applicationInsightsExceptionsLink?: string | null;
    health?: HealthCheckDto;
    last?: string;
};

