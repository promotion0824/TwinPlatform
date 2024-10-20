import { Routes, Route } from 'react-router-dom'
import BaseLayout from './layout/BaseLayout'
import SidenavLayout from './layout/SidenavLayout'
import ThemeObject from './pages/ThemeObject'
import ThemeColors from './pages/ThemeColors'
import ThemeTypography from './pages/ThemeTypography'
import ComponentButton from './pages/ComponentButton'
import ComponentSelect from './pages/ComponentSelect'
import ComponentRadio from './pages/ComponentRadio'
import ComponentCheckbox from './pages/ComponentCheckbox'
import ComponentTextField from './pages/ComponentTextField'
import ComponentSwitch from './pages/ComponentSwitch'
import TableDemo from './pages/TableDemo'
import NoMatch from './pages/NoMatch'

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<SidenavLayout />}>
        <Route index element={<ThemeObject />} />
        <Route path="colors" element={<ThemeColors />} />
        <Route path="typography" element={<ThemeTypography />} />

        <Route path="button" element={<ComponentButton />} />
        <Route path="checkbox" element={<ComponentCheckbox />} />
        <Route path="radio" element={<ComponentRadio />} />
        <Route path="select" element={<ComponentSelect />} />
        <Route path="switch" element={<ComponentSwitch />} />
        <Route path="text-field" element={<ComponentTextField />} />
      </Route>

      <Route element={<BaseLayout />}>
        <Route path="table-demo" element={<TableDemo />} />
        <Route path="*" element={<NoMatch />} />
      </Route>
    </Routes>
  )
}
