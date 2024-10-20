import { Icon } from '../../misc/Icon'
import { Loader } from '../../feedback/Loader'
import { useTheme } from '../../theme'
import {
  BaseIconButton,
  BaseInputCheckbox,
  BaseTooltip,
  DataGridIcon,
  DataGridOverlay,
} from './components'

export const useSxStyles = () => {
  const theme = useTheme()

  const sx = {
    // default styles for DataGrid
    ...theme.font.body.md.regular,
    color: theme.color.neutral.fg.default,
    borderColor: theme.color.neutral.border.default,

    a: {
      color: theme.color.intent.primary.fg.default,
      textDecoration: 'none',
    },

    '& .MuiDataGrid-columnSeparator': {
      color: theme.color.neutral.border.default,

      '&:hover, &.MuiDataGrid-columnSeparator--resizing': {
        color: theme.color.neutral.fg.default,
      },
    },

    '& .MuiDataGrid-columnHeader, .MuiDataGrid-cell': {
      paddingTop: theme.spacing.s12,
      paddingBottom: theme.spacing.s12,
      paddingLeft: theme.spacing.s8,
      '&:focus, &:focus-within': {
        outlineOffset: '-1px',
        outline: `1px solid ${theme.color.state.focus.border}`,
      },
    },

    '& .MuiDataGrid-cellCheckbox': {
      position: 'relative',
    },
    // override the paddings for row handler cells
    '& .MuiDataGrid-cellCheckbox, .MuiDataGrid-columnHeaderCheckbox, .MuiDataGrid-rowReorderCellContainer':
      {
        // padding value is not useful because it will be overridden
        // by the minimum width and height from the cell style.
        // But it helps to centralize the icon.
        padding: theme.spacing.s8,
      },

    '& .MuiDataGrid-cell, .MuiDataGrid-columnHeader': {
      padding: theme.spacing.s12,
    },

    '& .MuiDataGrid-columnHeaders': {
      ...theme.font.heading.xs,
      backgroundColor: theme.color.neutral.bg.panel.default,
    },

    '& .MuiDataGrid-withBorderColor': {
      // color for all borders
      borderColor: theme.color.neutral.border.default,
    },

    '& .MuiDataGrid-row': {
      '&.Mui-selected, &.Mui-selected:hover': {
        backgroundColor: theme.color.neutral.bg.accent.default,
      },
      '&:hover, &.Mui-hovered': {
        backgroundColor: theme.color.neutral.bg.panel.hovered,
      },
    },

    // overlay
    '& .MuiDataGrid-overlay': {
      background: 'none', // remove default overlay background
    },

    // drag and drop
    '& .MuiDataGrid-rowReorderCell--draggable': {
      cursor: 'grab',
    },
    '& .MuiDataGrid-rowReorderCellContainer': {
      padding: 0, // remove padding so that the dragging row will take full height same as normal row
    },
    // dragging row and column
    '& .MuiDataGrid-row--dragging, .MuiDataGrid-columnHeader--dragging': {
      height: 'auto', // required for column header
      backgroundColor: theme.color.neutral.bg.panel.default,
      border: `1px solid ${theme.color.neutral.border.default}`,
      borderRadius: theme.radius.r4,
      boxShadow: theme.shadow.s3,
      opacity: 0.99, // opacity cannot be higher than the encapsulating element which is 1

      gap: theme.spacing.s24,
    },

    // restyle the pinned columns
    '& .MuiDataGrid-pinnedColumnHeaders, & .MuiDataGrid-pinnedColumns': {
      backgroundColor: theme.color.neutral.bg.panel.default,
    },

    '& .MuiDataGrid-pinnedColumnHeaders--left, & .MuiDataGrid-pinnedColumns--left':
      {
        boxShadow: `1px 0 0 0 ${theme.color.neutral.border.default}`,
      },

    '& .MuiDataGrid-pinnedColumnHeaders--right, & .MuiDataGrid-pinnedColumns--right':
      {
        boxShadow: `-1px 0 0 0 ${theme.color.neutral.border.default}`,
      },
  }
  return sx
}

export const slots = {
  baseIconButton: BaseIconButton,
  baseCheckbox: BaseInputCheckbox,
  baseTooltip: BaseTooltip,

  columnUnsortedIcon: () => <Icon icon="arrow_upward" />,
  columnSortedAscendingIcon: () => <DataGridIcon icon="arrow_upward" />,
  columnSortedDescendingIcon: () => <DataGridIcon icon="arrow_downward" />,
  columnMenuIcon: () => <DataGridIcon icon="more_vert" />,
  treeDataExpandIcon: () => <DataGridIcon icon="chevron_right" />,
  treeDataCollapseIcon: () => <DataGridIcon icon="keyboard_arrow_down" />,
  rowReorderIcon: () => <DataGridIcon icon="drag_indicator" />,
  loadingOverlay: () => (
    <DataGridOverlay>
      <Loader size="md" />
    </DataGridOverlay>
  ),
}
