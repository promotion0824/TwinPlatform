import { FaHandHoldingWater, FaIntercom, FaQuestionCircle, FaRegLightbulb, FaStreetView, FaTemperatureHigh, FaTemperatureLow } from 'react-icons/fa';
import { BiAlignMiddle, BiCabinet, BiCylinder, BiNoEntry, BiWater } from 'react-icons/bi';
import { VscSymbolRuler } from 'react-icons/vsc';
import { BsBoxArrowDownRight, BsCloudDownload, BsSpeedometer2, BsSun } from 'react-icons/bs';
import { WiHumidity, WiWindBeaufort1 } from 'react-icons/wi';
import { GiBoatPropeller, GiLeak, GiServerRack, GiWaterDrop } from 'react-icons/gi';
import { RiSpeedMiniLine, RiTreasureMapLine } from 'react-icons/ri';
import { HiOutlineSwitchHorizontal } from 'react-icons/hi';
import { IconPropsShort } from './IconForModel'
import { TiDocument } from 'react-icons/ti';
import { FiTruck } from 'react-icons/fi';

const IconForModelIToL = ({ shortModelId, size }: IconPropsShort): JSX.Element => {
  switch (shortModelId) {
    case "IAQSensorEquipment": return (<GiServerRack size={size} />);
    case "ICTHardware": return (<GiServerRack size={size} />);
    case "ICTEquipment": return (<GiServerRack size={size} />);
    case "IlluminanceSensor": return (<BsSun size={size} />);
    case "IlluminanceZone": return (<BsSun size={size} />);
    case "Image": return (<TiDocument size={size} />);
    case "InletAirHumiditySensor": return (<WiHumidity size={size} />);
    case "InferredOccupancySensor": return (<FaStreetView size={size} />);
    case "InletAirFlowSensor": return (<WiWindBeaufort1 size={size} />);
    case "InletAirTemperatureSensor": return (<FaTemperatureHigh size={size} />);
    case "IntercomEntry": return (<FaIntercom size={size} />);
    case "InteriorAirTemperatureSensor": return (<FaTemperatureLow size={size} />);
    case "IntrusionDetectionEquipment": return (<BiNoEntry size={size} />);
    case "IrrigationPump": return (<GiBoatPropeller size={size} />);
    case "IrrigationSystem": return (<GiWaterDrop size={size} />);
    case "ITRack": return (<HiOutlineSwitchHorizontal size={size} />);
    case "JockeyPump": return (<GiBoatPropeller size={size} />);
    case "Land": return (<RiTreasureMapLine size={size} />);
    case "LeakDetectorEquipment": return (<GiLeak size={size} />);
    case "LeakSensor": return (<GiWaterDrop size={size} />);
    case "Lease": return (<TiDocument size={size} />);
    case "LengthSensor": return (<VscSymbolRuler size={size} />);
    case "LeaseContract": return (<TiDocument size={size} />);
    case "LeavingCondenserWaterTemperatureSensor": return (<FaHandHoldingWater size={size} />);
    case "LeavingCondenserWaterTemperatureSetpoint": return (<FaHandHoldingWater size={size} />);
    case "LeavingChilledWaterTemperatureSensor": return (<FaHandHoldingWater size={size} />);
    case "LeavingPeopleCountSensor": return (<BsBoxArrowDownRight size={size} />);
    case "LeavingHotWaterTemperatureSensor": return (<BiWater size={size} />);
    case "LeavingCondenserWaterTemperatureSensor": return (<BiWater size={size} />);
    case "Level": return (<BiAlignMiddle size={size} />);
    case "LevelSensor": return (<BiCylinder size={size} />);
    case "LevelState": return (<BiCylinder size={size} />);
    case "LevelActuator": return (<BiCylinder size={size} />);
    case "LightingController": return (<GiServerRack size={size} />);
    case "LightingEquipment": return (<FaRegLightbulb size={size} />);
    case "LightingZone": return (<FaRegLightbulb size={size} />);
    case "LinearAccelerationSensor": return (<RiSpeedMiniLine size={size} />);
    case "LinearSpeedSensor": return (<BsSpeedometer2 size={size} />);
    case "LoadLevelSensor": return (<BsCloudDownload size={size} />);
    case "LoadingDockEquipment": return (<FiTruck size={size} />);
    case "LoadingDockLeveler": return (<FiTruck size={size} />);
    case "LowTemperatureRefrigeratedFoodDisplayCase": return (<BiCabinet size={size} />);
    case "Luminaire": return (<FaRegLightbulb size={size} />);

    default: return (<FaQuestionCircle size={size} />);
  }
}

export default IconForModelIToL;
