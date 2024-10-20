import { FaBookOpen, FaCar, FaCreativeCommonsNd, FaFire, FaGripfire, FaQuestionCircle, FaRegObjectGroup, FaStreetView, FaUserFriends } from 'react-icons/fa';
import { BiRuler, BiWallet, BiWater } from 'react-icons/bi';
import { FcElectricalSensor } from 'react-icons/fc';
import { BsCloudRain, BsCloudRainFill, BsFillPersonFill, BsHddRack, BsPeopleFill, BsPercent, BsSun } from 'react-icons/bs';
import { IoColorFilterOutline, IoDiscOutline, IoSpeedometerOutline, IoWaterOutline, IoWaterSharp } from 'react-icons/io5';
import { Gi3DStairs, GiArmoredBoomerang, GiBelt, GiBoatPropeller, GiCheckboxTree, GiChemicalTank, GiElectric, GiIBeam, GiPipes, GiPollenDust, GiPowerLightning, GiRialtoBridge, GiTowel, GiValve, GiWaterDrop, GiWaterTank } from 'react-icons/gi';
import { IconPropsShort } from './IconForModel'
import { MdOutlineMotionPhotosOn, MdOutlineSensors, MdSpaceBar } from 'react-icons/md';
import { RiLightbulbLine } from 'react-icons/ri';
import { VscGroupByRefType } from 'react-icons/vsc';

const IconForModelP = ({ shortModelId, size }: IconPropsShort): JSX.Element => {

  const initial = shortModelId.charAt(1).toUpperCase();
  switch (initial) {
    case 'A':  // PA
      {
        switch (shortModelId) {
          case "PaperTowelDispenser": return (<GiTowel size={size} />);
          case "Parameter": return (<MdOutlineSensors size={size} />);
          case "ParkingEquipment": return (<FaCar size={size} />);
          case "ParkingTicketDispenser": return (<FaCar size={size} />);
          case "ParkingPaymentEquipment": return (<FaCar size={size} />);
          case "ParkingSpot": return (<MdSpaceBar size={size} />);
          case "PassiveChilledBeam": return (<GiIBeam size={size} />);
          case "PassengerCheckInBaggageConveyor": return (<GiBelt size={size} />);
          case "PassengerBoardingLift": return (<Gi3DStairs size={size} />);
          case "PassengerBoardingRamp": return (<Gi3DStairs size={size} />);
          case "PassengerBoardingBridge": return (<GiRialtoBridge size={size} />);
          case "PatchPanel": return (<BsHddRack size={size} />);
        }
        break;
      }
    case 'E':  // PE
      {
        switch (shortModelId) {
          case "PeopleCountSensor": return (<FaUserFriends size={size} />);
          case "PeopleCountSensorEquipment": return (<BsPeopleFill size={size} />);
          case "PercentSensor": return (<BsPercent size={size} />);
          case "PercentUnityActuator": return (<BsPercent size={size} />);
          case "PercentActuator": return (<BsPercent size={size} />);
          case "Person": return (<BsFillPersonFill size={size} />);
          case "PeopleMotionSensor": return (<MdOutlineMotionPhotosOn size={size} />);
          case "PeopleInferredOccupancySensor": return (<BsFillPersonFill size={size} />);
          case "PeopleOccupancySensor": return (<BsFillPersonFill size={size} />);
        }
        break;
      }
    case 'H':  // PH
      {
        switch (shortModelId) {
          case "PhaseAActiveElectricalPowerSensor": return (<FcElectricalSensor size={size} />);
          case "PhaseBActiveElectricalPowerSensor": return (<FcElectricalSensor size={size} />);
          case "PhaseCActiveElectricalPowerSensor": return (<FcElectricalSensor size={size} />);
          case "PhaseAReactiveElectricalEnergySensor": return (<GiElectric size={size} />);
          case "PhaseBReactiveElectricalPowerSensor": return (<GiElectric size={size} />);
          case "PhaseCReactiveElectricalPowerSensor": return (<GiElectric size={size} />);
          case "PhotovoltaicArray": return (<BsSun size={size} />);
          case "PhotovoltaicPanel": return (<BsSun size={size} />);
        }
        break;
      }
    case 'I':  // PI
      {
        switch (shortModelId) {
          case "PipeFitting":
          case "PipeFittingAdaptor":
          case "PipeFittingElbow":
          case "PipeFittingFlange":
          case "PipeFittingCoupling":
          case "PipeFittingCross":
          case "PipeFittingPlug":
          case "PipeFittingTrap":
          case "PipeFittingTee":
          case "PipeFittingWye": return (<GiPipes size={size} />);
        }
        break;
      }
    case 'L':  // PL
      {
        switch (shortModelId) {
          case "PlumbingBackflowPreventer": return (<GiValve size={size} />);
          case "PlumbingBalancingValve": return (<GiValve size={size} />);
          case "PlumbingCheckValve": return (<GiValve size={size} />);
          case "PlumbingGlobeValve": return (<GiValve size={size} />);
          case "PlumbingShutOffValve": return (<GiValve size={size} />);
          case "PlumbingEquipmentGroup": return (<VscGroupByRefType size={size} />);
          case "PlumbingExpansionTank": return (<GiWaterTank size={size} />);
          case "PlumbingEquipment": return (<BiWater size={size} />);
          case "PlumbingFixture": return (<BiWater size={size} />);
          case "PlumbingGroup": return (<VscGroupByRefType size={size} />);
          case "PlumbingSystem": return (<IoWaterSharp size={size} />);
          case "PlumbingManifold": return (<GiCheckboxTree size={size} />);
          case "PlumbingPressureReducingStation": return (<IoWaterOutline size={size} />);
          case "PlumbingPump": return (<GiBoatPropeller size={size} />);
          case "PlumbingPumpGroup": return (<VscGroupByRefType size={size} />);
          case "PlumbingStorageTank": return (<GiWaterTank size={size} />);
          case "PlumbingShutoffValve": return (<GiValve size={size} />);
          case "PlumbingValve": return (<GiValve size={size} />);
          case "PlumbingSolenoidValve": return (<GiValve size={size} />);
          case "PlumbingWaterFiltration": return (<IoColorFilterOutline size={size} />);
          case "PlumbingWaterSystem": return (<GiWaterDrop size={size} />);
          case "PlumbingWaterTreatment": return (<IoDiscOutline size={size} />);
          case "PlumbingTank": return (<GiChemicalTank size={size} />);
          case "PlumbingPressureReducingValve": return (<GiValve size={size} />);
        }
        break;
      }
    case 'R':  // PR
      {
        switch (shortModelId) {
          case "PreActionCabinet": return (<FaGripfire size={size} />);
          case "PreActionSprinklerSystem": return (<FaFire size={size} />);
          case "PrecipitationAccumulationSensor;1": return (<BsCloudRainFill size={size} />);
          case "PrecipitationRateSensor;1": return (<BsCloudRain size={size} />);
          case "PrecisionApproachPathIndicatorLightFixture": return (<RiLightbulbLine size={size} />);
          case "PresenceSensor": return (<FaStreetView size={size} />);
          case "PresenceAbsenceSensor": return (<FaStreetView size={size} />);
          case "PressureSensor": return (<FaCreativeCommonsNd size={size} />);
          case "PressureSetpoint": return (<FaCreativeCommonsNd size={size} />);
          case "ProductData": return (<FaBookOpen size={size} />);
          case "Product_IOM_Manual": return (<FaBookOpen size={size} />);
          case "PropaneSystem": return (<FaRegObjectGroup size={size} />);
        }
        break;
      }
    default:
      {
        break;
      }
  }

  switch (shortModelId) {
    case "PM001AirQualitySensor": return (<GiPollenDust size={size} />);
    case "PM025AirQualitySensor": return (<GiPollenDust size={size} />);
    case "Portfolio": return (<BiWallet size={size} />);
    case "PositionActuator": return (<GiArmoredBoomerang size={size} />);
    case "PowerFactorSensor": return (<GiPowerLightning size={size} />);
    case "PowerSensor": return (<GiElectric size={size} />);
    case "Pump": return (<GiBoatPropeller size={size} />);
    case "PositionSensor": return (<BiRuler size={size} />);
    case "PumpActuator": return (<GiBoatPropeller size={size} />);
    case "PumpRunSensor": return (<IoSpeedometerOutline size={size} />);
    case "PumpRunActuator": return (<GiBoatPropeller size={size} />);

    default: return (<FaQuestionCircle size={size} />);
  }
}

export default IconForModelP;
