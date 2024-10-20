import { Box, Checkbox, Chip, FormControl, InputLabel, Select, TextField, Typography } from '@mui/material';
import { SimpleTreeView, TreeItem2 } from '@mui/x-tree-view';
import { ChangeEvent, useEffect, useMemo, useState } from 'react';
import { SelectTreeModel, FlattenSelectTree, GetTargetPaths, FormatValueAsExpression, UnFormatExpressionIntoValues, SelectTreeErrorModel } from '../../types/SelectTreeModel';
import { AppIcons } from '../../AppIcons';

const Enable_MultiSelect = false;
const ITEM_HEIGHT = 100;
const ITEM_PADDING_TOP = 8;
const MenuProps = {
  PaperProps: {
    style: {
      maxHeight: ITEM_HEIGHT * 4.5 + ITEM_PADDING_TOP,
      width: 300,
    },
  },
};

export default function FormSelectTree({ selectLabel, rawLabel, options, onChange, showRawEditor, defaultRawValue, onError }:
  {
    selectLabel: string,
    rawLabel: string,
    options: SelectTreeModel[],
    onChange: (val: string) => void,
    showRawEditor: boolean,
    defaultRawValue: string,
    onError?: (errorModel: SelectTreeErrorModel) => void,
  }) {
  const [locs, setLocs] = useState<SelectTreeModel[]>([]);
  const [rawText, setRawText] = useState<string>('');
  const [expandedItemIds, setExpandedItemIds] = useState<string[]>([]);
  const flattenedModel = useMemo(() => {
    return FlattenSelectTree(options,"");
  }, [options]);

  useEffect(() => {
    setRawText(defaultRawValue);
    updateSelectValues(defaultRawValue);
  }, []);

  const checkboxOnChange = (event: ChangeEvent<HTMLInputElement>, checked: boolean) => {
    // get the Id from the checkbox Id
    let Id = event.target.id.substring('chkbx-'.length);

    // get the Option
    let alteredLocs: SelectTreeModel[];
    let changedOptionFromSource = flattenedModel.find(f => f.id === Id);
    if (!changedOptionFromSource)
      return;

    if (Enable_MultiSelect) {
      let changedOptionFromSelected = locs.find(f => f.id === Id);
      if ((!changedOptionFromSelected && !checked) || (checked && !!changedOptionFromSelected)) {
        return;
      }

      alteredLocs = checked ? [...locs, changedOptionFromSource] : locs.filter(f => f.id !== Id);
    } else {
      alteredLocs = checked ? [changedOptionFromSource] : [];
    }
    setLocs([...alteredLocs]);

    const formattedVal = FormatValueAsExpression(alteredLocs);
    onChange(formattedVal);
    setRawText(formattedVal);
  };

  const checkboxLabel = (node: SelectTreeModel) => (
    <div style={{ display: 'flex', alignItems: 'center' }}>
      <Checkbox
        sx={{ padding: 0.5 }}
        id={`chkbx-${node.id}`}
        checked={locs.findIndex(f => f.id === node.id) > -1}
        onChange={(e, c) => checkboxOnChange(e, c)}
      />
      <Typography>{node.name}</Typography>
    </div>
  );

  const renderTreeNodes = (nodes: SelectTreeModel[]) =>
    nodes.map((node: SelectTreeModel) => (
      <TreeItem2 key={node.id} itemId={node.id} label={checkboxLabel(node)} onClick={(e) => e.stopPropagation()}>
        {Array.isArray(node.children) ? renderTreeNodes(node.children) : null}
      </TreeItem2>
    ));

  const onSelectOpen = () => {
    try {
      // Update the Location Select Items
      const matches = UnFormatExpressionIntoValues(rawText);
      // Arrange the expand/collapse of the tree based on the selection
      const selectedPathIds = GetTargetPaths(options, matches);
      setExpandedItemIds(Array.from(new Set(selectedPathIds)));
    } catch (e) {
      console.error(e);
    }
  };

  const onRawTextChange = (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    let currVal = event.target.value || '';
    setRawText(currVal);
    updateSelectValues(currVal);
    onChange(currVal);
  };

  const updateSelectValues = (rawExp: string) => {
    try {
      // Update the Location Select Items
      const matches = UnFormatExpressionIntoValues(rawExp);
      if (matches.length > 0) {
        setLocs(flattenedModel.filter(f => matches.findIndex(x => x === f.id) > -1));
      } else {
        setLocs([]);
        if (!!onError && !!rawExp && rawExp.length > 1) {
          const errorModel = new SelectTreeErrorModel();
          errorModel.expParseFailure = true;
          onError(errorModel);
        }
      }
    } catch (e) {
      console.error(e);
    }
  };

  return (
    <>
      {
        !showRawEditor ?
          <FormControl fullWidth>
            <InputLabel htmlFor={`${selectLabel}-multiple-chip`}>{selectLabel}</InputLabel>
            <Select
              multiple
              onOpen={onSelectOpen}
              label={selectLabel}
              id={`${selectLabel}-multiple-chip`}
              value={locs}
              renderValue={(selected) => (
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                  {selected.map((value: any) => (
                    <Chip key={value.id} icon={AppIcons.ApartmentIcon} label={value.displayName} variant="outlined" />
                  ))}
                </Box>
              )}
              MenuProps={MenuProps}
            >
              <SimpleTreeView disableSelection defaultExpandedItems={expandedItemIds}>
                {renderTreeNodes(options)}
              </SimpleTreeView>

            </Select>
          </FormControl>
          :
          <TextField
            margin="dense"
            id={rawLabel}
            label={rawLabel}
            type="text"
            fullWidth
            variant="outlined"
            multiline
            value={rawText}
            onChange={onRawTextChange}
          />
      }
    </>


  );
}
