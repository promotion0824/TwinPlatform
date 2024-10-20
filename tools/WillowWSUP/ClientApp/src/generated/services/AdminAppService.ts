/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { HealthCheckDto } from '../models/HealthCheckDto';
import type { OverallState } from '../models/OverallState';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class AdminAppService {
    /**
     * @returns OverallState OK
     * @throws ApiError
     */
    public static state(): CancelablePromise<OverallState> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/state',
        });
    }
    /**
     * @returns HealthCheckDto OK
     * @throws ApiError
     */
    public static healthchecks(): CancelablePromise<HealthCheckDto> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/healthz',
        });
    }
    /**
     * @returns string OK
     * @throws ApiError
     */
    public static livenessProbe(): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/livez',
        });
    }
    /**
     * @returns string OK
     * @throws ApiError
     */
    public static readinessProbe(): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/readyz',
        });
    }
}
