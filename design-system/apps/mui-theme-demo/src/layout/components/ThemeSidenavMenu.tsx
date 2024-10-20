import * as React from 'react'
import { Stack } from '@mui/material'

import {
  ThemeSidenavItem,
  ThemeSidenavList,
  ThemeSidenavButton,
  ThemeSidenavText,
  ThemeSidenavSubheader,
} from './ThemeSidenavMenuComponents'

export default function ThemeSidenavMenu() {
  return (
    <Stack spacing={2}>
      <ThemeSidenavList>
        <ThemeSidenavSubheader>Styles</ThemeSidenavSubheader>
        <ThemeSidenavItem>
          <ThemeSidenavButton to="/">
            <ThemeSidenavText primary={'Theme Object'} />
          </ThemeSidenavButton>
        </ThemeSidenavItem>
        <ThemeSidenavItem>
          <ThemeSidenavButton to="colors">
            <ThemeSidenavText primary={'Colors'} />
          </ThemeSidenavButton>
        </ThemeSidenavItem>
        <ThemeSidenavItem key={'Typography'}>
          <ThemeSidenavButton to="typography">
            <ThemeSidenavText primary={'Typography'} />
          </ThemeSidenavButton>
        </ThemeSidenavItem>
      </ThemeSidenavList>

      <ThemeSidenavList>
        <ThemeSidenavSubheader>Components</ThemeSidenavSubheader>
        <ThemeSidenavItem key={'Button'}>
          <ThemeSidenavButton to="button">
            <ThemeSidenavText primary={'Button'} />
          </ThemeSidenavButton>
        </ThemeSidenavItem>
        <ThemeSidenavItem key={'Checkbox'}>
          <ThemeSidenavButton to="checkbox">
            <ThemeSidenavText primary={'Checkbox'} />
          </ThemeSidenavButton>
        </ThemeSidenavItem>
        <ThemeSidenavItem key={'Radio'}>
          <ThemeSidenavButton to="radio">
            <ThemeSidenavText primary={'Radio'} />
          </ThemeSidenavButton>
        </ThemeSidenavItem>
        <ThemeSidenavItem key={'TextField'}>
          <ThemeSidenavButton to="text-field">
            <ThemeSidenavText primary={'TextField'} />
          </ThemeSidenavButton>
        </ThemeSidenavItem>
        <ThemeSidenavItem key={'Select'}>
          <ThemeSidenavButton to="select">
            <ThemeSidenavText primary={'Select'} />
          </ThemeSidenavButton>
        </ThemeSidenavItem>
        <ThemeSidenavItem key={'Switch'}>
          <ThemeSidenavButton to="switch">
            <ThemeSidenavText primary={'Switch'} />
          </ThemeSidenavButton>
        </ThemeSidenavItem>
      </ThemeSidenavList>
    </Stack>
  )
}
