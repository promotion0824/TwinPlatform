/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ApplicationSpecification } from './ApplicationSpecification';
import type { DeploymentPhase } from './DeploymentPhase';
import type { LifeCycleState } from './LifeCycleState';
export type CustomerInstance = {
    name?: string | null;
    domain?: string | null;
    deploymentPhase?: DeploymentPhase;
    resourceGroup?: string | null;
    readonly resourceGroupLink?: string | null;
    subscription?: string | null;
    isHybrid?: boolean;
    isDevelopment?: boolean;
    region?: string | null;
    customerInstanceCode?: string | null;
    fullCustomerInstanceName?: string | null;
    lifeCycleState?: LifeCycleState;
    applications?: Array<ApplicationSpecification> | null;
    isNewBuild?: boolean;
    readonly logUrl?: string | null;
};

