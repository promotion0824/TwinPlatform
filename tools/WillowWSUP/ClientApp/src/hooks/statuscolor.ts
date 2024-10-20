import { HealthStatus } from "../generated";

const getStatusColor = (health: HealthStatus | undefined) => {
  if (health === HealthStatus._0) {
    return 'red';
  } else if (health === HealthStatus._1) {
    return 'orange';
  } else if (health === HealthStatus._2) {
    return 'green';
  } else {
    return 'grey';
  }
};

export const getStatusTextColor = (health: HealthStatus | undefined) => {
  if (health === HealthStatus._0) {
    return 'white';
  } else if (health === HealthStatus._1) {
    return 'black';
  } else if (health === HealthStatus._2) {
    return 'white';
  } else {
    return 'white';
  }
};

export default getStatusColor;
