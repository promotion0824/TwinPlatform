import { ButtonGroup, ClickAwayListener, Grow, MenuItem, MenuList, Paper, Popper } from "@mui/material";
import { ArrowDropDownIcon } from "@mui/x-date-pickers-pro";
import { Button } from "@willowinc/ui";
import { Fragment, useRef, useState } from "react";

const ButtonList = (params: { options: string[], kind?: any, disabled?: boolean, loading?: boolean, onClick: (option: string, index: number) => void }) => {
  const options = params.options;
  const disabled = params.disabled ?? false;
  const loading = params.loading ?? false;
  const kind = params.kind ?? "secondary";
  const onClick = params.onClick;

  const [open, setOpen] = useState(false);
  const anchorRef = useRef<HTMLDivElement>(null);
  const [selectedIndex, setSelectedIndex] = useState(0);

  const handleClick = () => {
    onClick(options[selectedIndex], 0);
  };

  const handleMenuItemClick = (
    _: React.MouseEvent<HTMLLIElement, MouseEvent>,
    index: number,
  ) => {
    setSelectedIndex(index);
    setOpen(false);
    onClick(options[selectedIndex], index);
  };

  const handleToggle = () => {
    setOpen((prevOpen) => !prevOpen);
  };

  const handleClose = (event: Event) => {
    if (
      anchorRef.current &&
      anchorRef.current.contains(event.target as HTMLElement)
    ) {
      return;
    }

    setOpen(false);
  };

  return (
    <Fragment>
      <ButtonGroup
        size="small"
        variant="contained"
        ref={anchorRef}
        aria-label="Button group with a nested menu"
      >
        <Button kind={kind} loading={loading} disabled={disabled} onClick={handleClick}>{options[0]}</Button>
        <Button
          kind={kind}
          disabled={disabled}
          onClick={handleToggle}
        >
          <ArrowDropDownIcon sx={{ fontSize: '20px' }} />
        </Button>
      </ButtonGroup>
      <Popper
        sx={{
          zIndex: 1,
        }}
        open={open}
        anchorEl={anchorRef.current}
        role={undefined}
        transition
        disablePortal
      >
        {({ TransitionProps, placement }) => (
          <Grow
            {...TransitionProps}
            style={{
              transformOrigin:
                placement === 'bottom' ? 'center top' : 'center bottom',
            }}
          >
            <Paper>
              <ClickAwayListener onClickAway={handleClose}>
                <MenuList id="split-button-menu" autoFocusItem>
                  {options.map((option, index) => (
                    <MenuItem
                      key={option}
                      disabled={index === 2}
                      selected={index === selectedIndex}
                      onClick={(event) => handleMenuItemClick(event, index)}
                    >
                      {option}
                    </MenuItem>
                  ))}
                </MenuList>
              </ClickAwayListener>
            </Paper>
          </Grow>
        )}
      </Popper>
    </Fragment>)
}

export default ButtonList
