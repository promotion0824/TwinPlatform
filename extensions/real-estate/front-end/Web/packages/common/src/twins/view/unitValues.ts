// See extensions/real-estate/front-end/Web/scripts/dtdl-unit-options/README.md
// for info on where this data comes from and how to update it.
import valsFromJson from './unitVals.json'

type Option = { name: string; displayName: string }

/**
 * The keys of this dictionary are names of schemas that are inbuilt in DTDLv3.
 * The values are the values a property using this schema may take.
 */
const unitValues: { [key: string]: Option[] } = valsFromJson

export default unitValues
