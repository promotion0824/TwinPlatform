import { createTheme } from "@mui/material";
import type {} from "@mui/x-data-grid-pro/themeAugmentation";
import { deepmerge } from "@mui/utils";
import getTheme from "@willowinc/mui-theme";

const willowTheme = getTheme();

const customTheme = {
  components: {},
};

const theme = createTheme(deepmerge(willowTheme, customTheme));

export default theme;
