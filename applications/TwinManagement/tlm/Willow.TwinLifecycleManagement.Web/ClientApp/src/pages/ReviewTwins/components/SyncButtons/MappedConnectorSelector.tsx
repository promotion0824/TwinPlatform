import { Select, Loader, Tooltip } from '@willowinc/ui';
import useGetAllTwins from '../../hooks/useGetAllTwins';
import { Dispatch, SetStateAction } from 'react';
import useGetTwinById from '../../../Twins/hooks/useGetTwinById';
import { SourceType } from '../../../../services/Clients';

/**
 * Dropdown input component for selecting mapped connectors
 */
export default function MappedConnectorSelector({
  className,
  selectedConnectorState,
}: {
  className?: string;
  selectedConnectorState: [string | null, Dispatch<SetStateAction<string | null>>];
}) {
  const {
    data = [],
    isLoading,
    isSuccess,
  } = useGetAllTwins(['dtmi:com:willowinc:ConnectorApplication;1'], {
    select: (data) =>
      data
        .sort((a, b) => a?.twin?.name.localeCompare(b?.twin?.name))
        .map(({ twin }) => ({ label: twin?.name, value: twin?.$dtId })),
  });

  // const { data: buildingTwin, isSuccess: isSuccess1 } = useGetTwinById(
  //   selectedBuildingId!,
  //   { enabled: !!selectedBuildingId },
  //   true,
  //   SourceType.AdtQuery
  // );

  // let buildingConnectors =
  //   buildingTwin?.outgoingRelationships
  //     ?.filter(({ $relationshipName, $targetId }) => $relationshipName === 'servedBy' && $targetId?.startsWith('CON'))
  //     .map(({ $targetId }) => $targetId) || [];

  // // @ts-expect-error
  // const filteredData = data.filter(({ value }) => buildingConnectors.includes(value));
  const isDependencyLoaded = isSuccess; //&& isSuccess1;

  return (
    <>
      <Tooltip label="No connectors found" disabled={data.length > 0 || !isDependencyLoaded} position="bottom">
        <Select
          className={className}
          label="Select Connector"
          suffix={isLoading && <Loader />}
          data={data as any}
          onChange={(val) => selectedConnectorState[1](val)}
          searchable
          nothingFound="No results found"
        />
      </Tooltip>
    </>
  );
}
