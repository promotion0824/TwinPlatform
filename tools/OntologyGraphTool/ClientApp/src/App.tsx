import './App.css'
import NavBar from './components/navbar'
import { Container, CssBaseline, ThemeProvider } from '@mui/material'
import { Route, Routes } from 'react-router-dom'
import SideBySide from './pages/side-by-side'
import Assignment from './pages/assignments'
import RemoteFetchProvider from './providers/reactfetchprovider'
import AssignmentSingle from './pages/assignment-single'
import themes from './muiTheme'

function App() {

  return (
    <ThemeProvider theme={themes['dark']}>
      <RemoteFetchProvider>
        <CssBaseline />
        <Container maxWidth={false} sx={{ mt: 2 }}>
          <NavBar />
          <Routes>
            <Route path="/side-by-side" element={<SideBySide />} />
            <Route path="/assignment" element={<Assignment />} />
            <Route path="/assignment/:id" element={<AssignmentSingle />} />
          </Routes>
        </Container>
      </RemoteFetchProvider>
    </ThemeProvider>
  )
}

export default App
