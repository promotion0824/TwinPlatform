import { RiShip2Line } from "react-icons/ri";
import { HealthStatus, LifeCycleState } from "../generated";
import { TbBackhoe } from "react-icons/tb";
import { GiAncientRuins } from "react-icons/gi";
import { Tooltip } from "@mui/material";
import { HiOutlineWrenchScrewdriver } from "react-icons/hi2";

const healthString = (health: HealthStatus) => {
  switch (health) {
    case HealthStatus._0: return 'Unhealthy';
    case HealthStatus._1: return 'Degraded';
    case HealthStatus._2: return 'Healthy';
  }
  return '';
}

const IndicatorTip = (props: { title: string, health: HealthStatus, children: React.ReactElement }) =>
  <Tooltip placement="right-start" arrow title={<><div>{props.title}</div><div>{healthString(props.health)}</div></>}>{props.children}</Tooltip>;

const LifeCycleStateIndicator = (props: { lifeCycleState: LifeCycleState, health: HealthStatus }) => {

  switch (props.lifeCycleState) {
    case LifeCycleState.UNKNOWN: return <IndicatorTip title="Unknown" health={props.health}><div><span>?</span></div></IndicatorTip>;
    case LifeCycleState.COMMISSIONING: return <IndicatorTip title="Commissioning" health={props.health}><div><HiOutlineWrenchScrewdriver /></div></IndicatorTip>;
    case LifeCycleState.LIVE: return <IndicatorTip title="Live" health={props.health}><div><RiShip2Line /></div></IndicatorTip>;
    case LifeCycleState.DECOMMISSIONING: return <IndicatorTip title="Decomissioning" health={props.health}><div><TbBackhoe /></div></IndicatorTip>;
    case LifeCycleState.DECOMMISSIONED: return <IndicatorTip title="Decomissioned" health={props.health}><div><GiAncientRuins /></div></IndicatorTip>;
  }
  return <span>?</span>;
}

export default LifeCycleStateIndicator;
