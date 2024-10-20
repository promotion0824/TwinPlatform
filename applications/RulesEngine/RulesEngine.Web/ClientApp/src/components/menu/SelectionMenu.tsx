import { Menu, MenuItem, Tooltip } from '@mui/material';


export default function SimpleListMenu(props: any) {
  const { items, selectedIndex, open, anchorEl, onSelect, onClose } = props;

  const handleMenuItemClick = (selectedItemIndex: number) => {
    if (onSelect) {
      onSelect(selectedItemIndex);
    }
  };

  return (
    <div>
      <Menu
        anchorEl={anchorEl}
        open={open}
        onClose={onClose}>
        {items.map((item: any, itemIndex: number) => (
          <Tooltip key={item.key} title={item.description} placement={'right'}>
            <MenuItem key={item.key} selected={itemIndex === selectedIndex} onClick={() => handleMenuItemClick(itemIndex)}>
              {item.name}
            </MenuItem>
          </Tooltip>
        ))}
      </Menu>
    </div>
  );
}
