import React, { useEffect, useState, useMemo } from 'react';
import { Autocomplete, TextField, SxProps, Theme, Box, styled, IconButton } from '@mui/material';
import { ApiException, ErrorResponse, INestedTwin } from '../../services/Clients';
import { PopUpExceptionTemplate } from '../PopUps/PopUpExceptionTemplate';
import useGetLocations from '../../hooks/useGetLocations';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';

/**
 * Dropdown input component for selecting site id.
 * @param getOptionLabel function to get the label for each option
 * @param width of the component
 * @param selectedLocation selected site
 * @param setSelectedLocation useState to set selected site
 * @param disabled flag to disable the input
 */
export default function LocationSelector({
  sx,
  selectedLocation,
  setSelectedLocation,
  disabled = false,
  modelIds = ['dtmi:com:willowinc:Building;1', 'dtmi:com:willowinc:SubStructure;1'],
  exactModelMatch = false,
  error = false,
  helperText = null,
}: {
  selectedLocation: any;
  setSelectedLocation: (setSelectedLocation: any) => void;
  sx?: SxProps<Theme>;
  disabled?: boolean;
  modelIds?: string[];
  exactModelMatch?: boolean;
  error?: boolean | undefined;
  helperText?: React.ReactNode | null;
}) {
  // state used for error handling
  const [openPopUp, setOpenPopUp] = useState(true);
  const [showPopUp, setShowPopUp] = useState(false);
  const [errorMessage, setErrorMessage] = useState<ErrorResponse | ApiException>();

  const { data = [], isLoading } = useGetLocations(modelIds, exactModelMatch, {
    onError: (error) => {
      setErrorMessage(error);
      setShowPopUp(true);
      setOpenPopUp(true);
    },
  });

  // state used to keep track of which nodes are expanded
  const [expandedStatus, setExpandedStatus] = useState<ExpandedStatus>({});

  // Get all nodes with children. Used to toggle expand/collapse options in the dropdown.
  useEffect(() => {
    if (data.length > 0) {
      setExpandedStatus(createObjectWithChildrenFlag(data));
    }
  }, [data]);

  const toggleExpand = (twinId: string) => {
    let newExpandedStatus = { ...expandedStatus };
    let currentStatus = newExpandedStatus[twinId];
    currentStatus.isExpanded = !currentStatus?.isExpanded;

    // Only expand one node at a time at the same level
    if (currentStatus.isExpanded) {
      for (let key of Object.keys(newExpandedStatus)) {
        if (key === twinId) continue;
        if (newExpandedStatus[key].level === currentStatus.level) {
          newExpandedStatus[key].isExpanded = false;
        }
      }
    }

    setExpandedStatus(newExpandedStatus);
  };

  let locations = useMemo(() => flattenNestedTwins(data), [data]);

  // if disabled, set selected location to null
  useEffect(() => {
    if (disabled) setSelectedLocation(null);
  }, [disabled]);

  // determine if options should be displayed based on expandedStatus
  const shouldDisplayOption = (option: NestedTwinWithLevel): boolean => {
    if (option.parentId) {
      return expandedStatus[option.parentId]?.isExpanded && shouldDisplayOption(parentIdMap[option.parentId!]);
    }
    // case for top level root nodes
    else {
      return true;
    }
  };

  const parentIdMap = useMemo(() => createParentIdMap(data), [data]);

  let getOptionLabel = (option: NestedTwinWithLevel) => {
    let cur = parentIdMap[option.twin?.$dtId!];

    let path = [];
    while (cur) {
      path.push(cur.twin?.name);
      cur = parentIdMap[cur.parentId!];
    }

    return path.reverse().join(' ⯈ ');
  };

  return (
    <>
      <Autocomplete
        options={locations}
        noOptionsText={isLoading ? 'Loading...' : 'No locations found'}
        getOptionLabel={getOptionLabel}
        autoComplete={false}
        filterSelectedOptions={false}
        id="location"
        value={selectedLocation}
        onChange={(_, newValue) => {
          setSelectedLocation(newValue);
        }}
        isOptionEqualToValue={(option, value) => option.twin?.$dtId === value.twin?.$dtId}
        sx={{
          ...sx,
        }}
        renderInput={({ inputProps, ...rest }) => (
          <TextField
            helperText={helperText}
            error={error}
            {...rest}
            title={inputProps.value as string}
            autoComplete="off"
            fullWidth
            variant="filled"
            label="Location"
            data-cy="ETLocation"
            inputProps={{ ...inputProps, readOnly: true }}
          />
        )}
        renderOption={(props, option) => {
          return shouldDisplayOption(option) ? (
            <Box
              component="span"
              sx={{
                flexDirection: 'column',
                alignItems: 'flex-start !important',
                justifyContent: 'center !important',
                padding: '0 5px 0 5px !important',
                minHeight: '33.98px !important',
              }}
              {...props}
              onClick={() => {
                /* override handling onClick*/
              }}
              key={option.twin?.$dtId}
            >
              <Flex>
                {
                  // case for top level root nodes with no children
                  option?.children?.length === 0 && option.level === 0 ? (
                    <PaddingLeft />
                  ) : // case for nodes with no children
                  option?.children?.length === 0 ? (
                    // @ts-ignore
                    <IconButton
                      disableRipple
                      size="small"
                      sx={{
                        backgroundColor: 'transparent !important', // disable onHover effect
                        paddingLeft: (option?.level || 0) * 2,
                      }}
                      {...props}
                      className={undefined} //override default styling
                    >
                      <ChevronRightIcon sx={{ visibility: 'hidden' }} />
                    </IconButton>
                  ) : (
                    // case for nodes with children
                    <IconButton
                      disableRipple
                      size="small"
                      sx={{
                        backgroundColor: 'transparent !important', // disable onHover effect
                        paddingLeft: (option?.level || 0) * 2,
                      }}
                      onClick={() => {
                        toggleExpand(option.twin?.$dtId!);
                      }}
                    >
                      {expandedStatus[option.twin?.$dtId!]?.isExpanded ? <ExpandMoreIcon /> : <ChevronRightIcon />}
                    </IconButton>
                  )
                }

                <Indent level={option.level!} {...props} className={undefined} key={option.twin?.$dtId}>
                  {option.twin?.name}
                </Indent>
              </Flex>
            </Box>
          ) : null;
        }}
        disabled={disabled}
      />

      {
        // todo: remove when global error handling is implemented
        showPopUp && (
          <PopUpExceptionTemplate isCurrentlyOpen={openPopUp} onOpenChanged={setOpenPopUp} errorObj={errorMessage} />
        )
      }
    </>
  );
}

const PaddingLeft = styled('span')({ paddingLeft: 16 });
const Flex = styled('div')({ display: 'flex', flexDirection: 'row', alignItems: 'center', width: '100%' });

const Indent = styled('li')(({ level }: { level: number }) => ({
  paddingLeft: level * 2,
  width: '100%',
}));

type NestedTwinWithLevel = INestedTwin & { level?: number; isExpanded?: boolean; parentId?: string };

/**
 *  Recursively convert nested twins and its children to a flat array of twins, to be consume by the LocationSelector component.
 *  Level is used to indent the twin in the LocationSelector dropdown.
 */
function flattenNestedTwins(
  nestedTwins: NestedTwinWithLevel[],
  level: number = 0,
  parentId: string | null = null
): NestedTwinWithLevel[] {
  return (
    nestedTwins
      .sort((a, b) => {
        // Sort root level based on if they have children, then by name
        if (level === 0) {
          // First, compare based on whether they have children
          if (a?.children?.length! > 0 && b?.children?.length! === 0) {
            return -1; // `a` comes before `b`
          } else if (a?.children?.length! === 0 && b?.children?.length! > 0) {
            return 1; // `b` comes before `a`
          }
        }
        // If both have children or both don't, compare by name
        if (a.twin?.$metadata?.$model === 'dtmi:com:willowinc:Level;1') {
          return a.twin?.code?.localeCompare(b.twin?.code);
        }
        return a.twin?.name?.localeCompare(b.twin?.name);
      })
      // flatten array
      .reduce((acc, curr) => {
        let twin = { ...curr, level, parentId } as NestedTwinWithLevel;

        if (curr.children && curr.children.length > 0) {
          let children = flattenNestedTwins(curr.children, level + 1, curr.twin?.$dtId);
          return [...acc, twin, ...children];
        } else {
          return [...acc, twin];
        }
      }, [] as NestedTwinWithLevel[])
  );
}

type ExpandedStatus = Record<string, { isExpanded: boolean; level: number }>;
/**
 * Create a map of twinId to boolean and the level, to keep track of which nodes are expanded.
 */
function createObjectWithChildrenFlag(nestedTwins: INestedTwin[]) {
  const result = {} as ExpandedStatus;

  function traverse(node: INestedTwin, level: number = 0) {
    if (node.children && node.children.length > 0) {
      result[node.twin?.$dtId!] = { isExpanded: false, level };
      node.children.forEach((child) => traverse(child, level + 1));
    } else {
      return (result[node.twin?.$dtId!] = { isExpanded: false, level });
    }
  }

  nestedTwins.forEach((root) => traverse(root));
  return result;
}

function createParentIdMap(nestedTwins: INestedTwin[]) {
  const parentIdMap = {} as Record<string, NestedTwinWithLevel>;

  function traverse(node: INestedTwin, parentId: string | undefined = undefined) {
    parentIdMap[node.twin?.$dtId!] = { ...node, parentId };

    if (node.children) {
      node.children.forEach((child) => traverse(child, node.twin?.$dtId!));
    }
  }

  nestedTwins.forEach((rootNode) => traverse(rootNode));
  return parentIdMap;
}
