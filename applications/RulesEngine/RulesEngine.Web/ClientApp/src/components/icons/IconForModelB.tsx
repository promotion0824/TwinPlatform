import { FaQuestionCircle } from 'react-icons/fa';
import { VscCircuitBoard } from 'react-icons/vsc';
import { CgSmartHomeBoiler } from 'react-icons/cg';
import { HiOutlineOfficeBuilding } from 'react-icons/hi';
import { FiBatteryCharging, FiHexagon } from 'react-icons/fi';
import { BsBatteryHalf, BsDownload, BsFileBinary, BsSlashCircle } from 'react-icons/bs';
import { GiBattery100, GiBelt, GiBrickPile, GiCableStayedBridge, GiElectric, GiHammerBreak, GiValve } from 'react-icons/gi';
import { RiBuilding2Line } from 'react-icons/ri';
import { IconPropsShort } from './IconForModel'
import { BiAlignMiddle, BiBath, BiBattery, BiShoppingBag } from 'react-icons/bi';
import { MdOutlineLocalDrink } from 'react-icons/md';
import { AiOutlineBorder } from 'react-icons/ai';

const IconForModelB = ({ shortModelId, size }: IconPropsShort): JSX.Element => {

  const initial = shortModelId.charAt(1).toUpperCase();
  switch (initial) {
    case 'A':  // BA
      {
        switch (shortModelId) {
          case "BACnetController": return (<FiHexagon size={size} />);
          case "BackdraftDamper": return (<BsSlashCircle size={size} />);
          case "BaggageHandlingEquipment": return (<BiShoppingBag size={size} />);
          case "BaggageScale": return (<BiShoppingBag size={size} />);
          case "BaggageConveyor": return (<GiBelt size={size} />);
          case "BaggageScreeningEquipment": return (<BiShoppingBag size={size} />);
          case "BalancingValve": return (<GiValve size={size} />);
          case "BarrierAsset": return (<AiOutlineBorder size={size} />);
          case "Bathtub": return (<BiBath size={size} />);
          case "BatteryRack": return (<BiBattery size={size} />);
          case "BatteryCabinet": return (<BiBattery size={size} />);
          case "BatteryCharger": return (<BsBatteryHalf size={size} />);
          case "BatterySystem": return (<FiBatteryCharging size={size} />);
          case "BatteryEquipment": return (<GiBattery100 size={size} />);
          case "BasementLevel": return (<BiAlignMiddle size={size} />);
        }
        break;
      }

    case 'E': // BE
      {
        switch (shortModelId) {
          case "BeverageEquipment": return (<MdOutlineLocalDrink size={size} />);
        }
        break;
      }

    case 'I': // BI
      {
        switch (shortModelId) {
          case "BilledActiveElectricalEnergy": return (<GiElectric size={size} />);
          case "BinarySensor": return (<BsFileBinary size={size} />);
          case "BinaryState": return (<BsFileBinary size={size} />);
        }
        break;
      }

    case 'O': // BO
      {
        switch (shortModelId) {
          case "Boiler": return (<CgSmartHomeBoiler size={size} />);
        }
        break;
      }

    case 'R': // BR
      {
        switch (shortModelId) {
          case "BreakGlassSensor": return (<GiHammerBreak size={size} />);
          case "Bridge": return (<GiCableStayedBridge size={size} />);
        }
        break;
      }

    case 'U': // BU
      {
        switch (shortModelId) {
          case "Building": return (<RiBuilding2Line size={size} />);
          case "BuildingPodium": return (<HiOutlineOfficeBuilding size={size} />);
          case "BuildingTower": return (<HiOutlineOfficeBuilding size={size} />);
          case "BuildingCommonArea": return (<HiOutlineOfficeBuilding size={size} />);
          case "BuildingComponent": return (<HiOutlineOfficeBuilding size={size} />);
          case "BuildingWing": return (<HiOutlineOfficeBuilding size={size} />);
          case "BuildingManagementSystem": return (<HiOutlineOfficeBuilding size={size} />);
          case "BuildingOperationsArea": return (<HiOutlineOfficeBuilding size={size} />);
          case "BuildingAirStaticPressureSensor": return (<BsDownload size={size} />);
          case "BuildingAirStaticPressureSetpoint": return (<BsDownload size={size} />);
          case "BulkMaterialHandlingSystem": return (<GiBrickPile size={size} />);
          case "Busway": return (<GiElectric size={size} />);
          case "BusWay": return (<VscCircuitBoard size={size} />);
        }
        break;
      }
  }
  return (<FaQuestionCircle size={size} />);
}

export default IconForModelB;
