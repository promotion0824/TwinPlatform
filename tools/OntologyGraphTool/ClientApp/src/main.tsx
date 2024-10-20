import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.tsx'
import './index.css'
import { RouterProvider, createBrowserRouter } from 'react-router-dom';
// import SideBySide from './pages/side-by-side.tsx';
// import Assignment from './pages/assignments.tsx';

const router = createBrowserRouter([
  {
    path: "*",
    element: <App />,
  }//,
  // {
  //   path: "side-by-side",
  //   element: <SideBySide />,
  // },
  // {
  //   path: "assignment",
  //   element: <Assignment />,
  // },
]);

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <RouterProvider router={router}></RouterProvider>
  </React.StrictMode>,
)
