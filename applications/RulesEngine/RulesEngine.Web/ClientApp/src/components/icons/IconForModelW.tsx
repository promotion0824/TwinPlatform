import { FaQuestionCircle, FaTemperatureHigh, FaTemperatureLow, FaWifi } from 'react-icons/fa';
import { BiWindow } from 'react-icons/bi';
import { SiHackerrank } from 'react-icons/si';
import { IoCompassOutline, IoDiscOutline, IoSnowSharp } from 'react-icons/io5';
import { GiDesk, GiStoneWall, GiWaterDrop, GiWindsock } from 'react-icons/gi';
import { TiDocument, TiWeatherPartlySunny } from 'react-icons/ti';
import { IconPropsShort } from './IconForModel'
import { BsSpeedometer2 } from 'react-icons/bs';

const IconForModelW = ({ shortModelId, size }: IconPropsShort): JSX.Element => {
  switch (shortModelId) {
    case "Wall": return (<GiStoneWall size={size} />);
    case "Warranty": return (<TiDocument size={size} />);
    case "WasteVentDrainageSystem": return (<IoDiscOutline size={size} />);
    case "WaterCooledChiller": return (<IoSnowSharp size={size} />);
    case "WaterHeater": return (<SiHackerrank size={size} />);
    case "WaterMeter": return (<GiWaterDrop size={size} />);
    case "WaterFlowSensor": return (<GiWaterDrop size={size} />);
    case "WaterFlowSetpoint": return (<GiWaterDrop size={size} />);
    case "WaterPressureSensor": return (<GiWaterDrop size={size} />);
    case "WaterSoftener": return (<IoDiscOutline size={size} />);
    case "WaterTemperatureSensor": return (<FaTemperatureHigh size={size} />);
    case "WaterTemperatureSetpoint": return (<FaTemperatureHigh size={size} />);
    case "WaterVolumeSensor": return (<GiWaterDrop size={size} />);
    case "WaterVolumeSetpoint": return (<GiWaterDrop size={size} />);

    case "WeatherConditionState": return (<TiWeatherPartlySunny size={size} />);
    case "WeatherStationEquipment": return (<TiWeatherPartlySunny size={size} />);
    case "WetBulbTemperatureSensor": return (<FaTemperatureLow size={size} />);
    case "Window": return (<BiWindow size={size} />);
    case "WindCone": return (<GiWindsock size={size} />);
    case "WindDirectionSensor": return (<IoCompassOutline size={size} />);
    case "WindSpeedSensor": return (<BsSpeedometer2 size={size} />);
    case "WirelessAccessPoint": return (<FaWifi size={size} />);
    case "Workstation": return (<GiDesk size={size} />);

    default: return (<FaQuestionCircle size={size} />);
  }
}

export default IconForModelW;
