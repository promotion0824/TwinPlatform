import getStatusColor from '../hooks/statuscolor';
import { HealthStatus } from "../generated";
import { FaCircle } from "react-icons/fa";

const StatusIcon: React.FC<{ health: HealthStatus | undefined, size: number }> = ({ health, size }) => (
  <FaCircle color={getStatusColor(health)} size={size} />);

export default StatusIcon;
