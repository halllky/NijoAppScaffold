import React from 'react'
import * as ReactRouter from 'react-router-dom'
import ReactDOM from 'react-dom/client'

import './main.css'
import 'allotment/dist/style.css'
import ReflectionDebugger from './reflection-debugger'

function App() {
  return (
    <div className="h-full w-full p-4">

      {/* ReflectionDebugger の使い方のサンプル */}
      <ReflectionDebugger className="h-full w-full" />
    </div>
  )
}

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
)
