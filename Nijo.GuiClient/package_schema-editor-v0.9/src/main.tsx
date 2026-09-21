import { StrictMode } from "react"
import { createRoot } from "react-dom/client"
import { BackendDataContextProvider } from "./features/backend"
import ProjectPage from "./page/ProjectPage"

import "./index.css"

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <BackendDataContextProvider>
      <ProjectPage />
    </BackendDataContextProvider>
  </StrictMode>,
)
