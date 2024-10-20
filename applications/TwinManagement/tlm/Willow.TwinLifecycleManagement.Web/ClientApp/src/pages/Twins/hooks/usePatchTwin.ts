import { BasicDigitalTwin } from '../../../services/Clients';
import useApi from '../../../hooks/useApi';

export default function usePatchTwin() {
  const api = useApi();

  async function saveTwin({
    existingTwin,
    newTwin,
    ignoreFields,
  }: {
    existingTwin: BasicDigitalTwin;
    newTwin: BasicDigitalTwin;
    ignoreFields: string[];
  }) {
    const patchOperations = createJsonPatchOps(existingTwin, newTwin, ignoreFields);
    await api.patchTwin(existingTwin.$dtId!, true, patchOperations);
  }

  function createJsonPatchOps(existingTwin: BasicDigitalTwin, newTwin: BasicDigitalTwin, ignoreFields: string[]) {
    function compareObjects(obj1: BasicDigitalTwin, obj2: BasicDigitalTwin, path = '/customproperties/') {
      let patchOperations: any[] = [];
      for (let key in obj2) {
        if (ignoreFields.includes(key)) {
          continue; // Skip ignore fields
        }
        let newPath = path + key;
        let value1 = obj1[key];
        let value2 = obj2[key];

        if (value1 === undefined) {
          // Add operation
          patchOperations.push({ op: 'add', path: newPath, value: value2 });
        } else if (typeof value1 === 'object' && typeof value2 === 'object') {
          // Recursively compare nested objects
          let nestedOperations = compareObjects(value1, value2, newPath + '/');
          patchOperations = patchOperations.concat(nestedOperations);
        } else if (value1 !== value2) {
          if (value2 === '') {
            // Remove operation
            patchOperations.push({ op: 'remove', path: newPath });
          } else {
            // Replace operation
            patchOperations.push({ op: 'replace', path: newPath, value: value2 });
          }
        }
      }

      for (let key in obj1) {
        if (obj2[key] === undefined && !ignoreFields.includes(key)) {
          // Remove operation
          let newPath = path + key;
          patchOperations.push({ op: 'remove', path: newPath });
        }
      }

      return patchOperations;
    }

    return compareObjects(existingTwin, newTwin);
  }

  return { saveTwin };
}
