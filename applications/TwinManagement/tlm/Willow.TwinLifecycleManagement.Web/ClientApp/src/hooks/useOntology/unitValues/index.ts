import valsFromJson from './unitValues.json';

type Option = { name: string; displayName: string };

/**
 * The keys of this dictionary are names of schemas that are inbuilt in DTDLv3.
 * The values are the values a property using this schema may take.
 */
const unitValues: { [key: string]: Option[] } = valsFromJson;

export default unitValues;
