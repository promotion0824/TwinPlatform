
const nodeStyle = (x: { modelId: string, isSelected: boolean, isCollapsed: boolean, isExpanded: boolean, isHighlighted?: boolean }, theme: any): { border: string, boxShadow: any, padding: number, backgroundColor: string, color: string } => {

  const isZone = x.modelId.endsWith("Zone");
  const isSystem = x.modelId.endsWith("System");
  let transparency = '';

  if (x.isHighlighted !== undefined) {
    if (!x.isSelected && !x.isHighlighted) {
      //lowlight nodes that aren't part of highlight list
      transparency = '4D';
    }
  }

  let borderColor =
    x.modelId.indexOf("Room;1") > 0 ? '#6e00ff' :
      x.modelId.indexOf("Level;1") > 0 ? '#6e00ff' :
        x.modelId.indexOf("Building;1") > 0 ? '#6e00ff' :
          x.modelId.indexOf("Land;1") > 0 ? '#6e00ff' :
            x.modelId.indexOf("Portfolio;1") > 0 ? '#6e00ff' :
              isZone ? '#8E8E3F' :
                isSystem ? '#4E808F' :
                  '#decede';
  borderColor = `${borderColor}${transparency}`;

  const backgroundColor = `${theme.palette.background.default}${transparency}`;

  // hack for aggregate nodes
  const border = x.isCollapsed ? '8px double' :
    isZone ? '3px dotted' :
      '3px solid';

  const shadow: any = x.isSelected ?
    {
      boxShadow: `0 0 10px 8px #6E00FF${transparency}`
    } :

    x.isExpanded ?
      {
        boxShadow: `0 0 10px 8px #7c7c33${transparency}`
      } :

      {};


  return {
    color: `#b9b9ff${transparency}`,
    ...shadow,
    border: `${border} ${borderColor}`,
    padding: 10,
    backgroundColor: backgroundColor
  };
};

export default nodeStyle;
