import { IValidationResults, CheckType } from '../../../../services/Clients';
import { styled } from '@mui/material/styles';

/**
 * Parse the resultInfo object to an array of readable string.
 */
export function parseResultInfo(result: IValidationResults): JSX.Element[] {

  const checkType = result.checkType;
  const resultInfo = result.resultInfo || '';

  switch (checkType) {
    case CheckType.Properties:
      return parseErrorProperties(resultInfo);
    case CheckType.Relationships:
      return parseErrorRelationships(resultInfo);
    // todo: for the following result types (DataQualityRule, Telemetry), stringify the resultInfo object for now.
    //     - will make the output more readable in the future.
    default:
      return [<span> {JSON.stringify(resultInfo)} </span>];
  }
}

// PropertyValidationResultType is from Willow.DataQuality.Model.Validation,
// https://github.com/WillowInc/TwinPlatform/blob/main/libraries/Willow.DataQuality/Willow.DataQuality.Model/Validation/PropertyValidationResult.cs#L3
enum PropertyValidationResultType {
  RequiredPropertyMissing = 'RequiredPropertyMissing',
  InvalidValue = 'InvalidValue',
  InvalidFormat = 'InvalidFormat',
}

interface ErrorProperty {
  type: PropertyValidationResultType;
  propertyName: string;
  actualValue: string | number | null;
  expectedValue: string | number | null;
}

const parseErrorProperties = (errorProperties: ErrorProperty[]): JSX.Element[] => {
  let requiredMissing: JSX.Element[] = [];
  let invalidValue: JSX.Element[] = [];
  let invalidFormat: JSX.Element[] = [];

  for (let errorProperty of errorProperties) {
    const { type, propertyName, actualValue, expectedValue } = errorProperty;

    switch (type) {
      case PropertyValidationResultType.RequiredPropertyMissing:
        requiredMissing.push(<span><b>{propertyName}</b>: Required property missing</span>);
        break;
      case PropertyValidationResultType.InvalidValue:
        invalidValue.push(<span>hey<b>{propertyName}</b>: Invalid value - found: {boxedError(actualValue)}, expected {boxedOk(expectedValue)}</span>);
        break;
      case PropertyValidationResultType.InvalidFormat:
        invalidFormat.push(<span><b>{propertyName}</b>: Invalid format. Found: {boxedError(actualValue)}, should match pattern {boxedOk(expectedValue)}</span>);
        break;
    }
  }

  return [...requiredMissing, ...invalidValue, ...invalidFormat];
};

interface ErrorRelationship {
  isValid: boolean;
  path: string;
}

const parseErrorRelationships = (errorRelationships: ErrorRelationship[]): JSX.Element[] => {
  let invalidPath: JSX.Element[] = [];

  for (let errorRelationship of errorRelationships) {
    const { isValid, path } = errorRelationship;
    if (!isValid) {
      const shortPath = path.replace("dtmi:com:willowinc:", "");
      invalidPath.push(<span><b>Missing relationship</b>: {boxedError(shortPath)}</span>);
    }
  }

  return invalidPath;
};


const StyledSpan = styled('span')({ display: 'inline-block' });
function boxedOk(text: string | number | null | undefined): JSX.Element {
  return (
    <StyledSpan style={{ whiteSpace: 'nowrap', border: '1px solid aqua', padding: '0px', margin: '1px' }}>      {text || '??'}
    </StyledSpan>  );
}
function boxedError(text: string | number | null | undefined): JSX.Element {
  return (
    <StyledSpan style={{ whiteSpace: 'nowrap', border: '1px solid red', padding: '0px', margin: '1px' }}>      {text || '??'}
    </StyledSpan>  );
}
