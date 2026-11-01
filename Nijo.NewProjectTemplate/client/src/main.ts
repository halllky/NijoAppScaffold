import './style.css'
import myAppLogo from '/my-app.svg'
import example from './example/example'

const app = document.querySelector<HTMLDivElement>('#app')!
app.innerHTML = `
  <img src="${myAppLogo}" class="logo" alt="MyApp logo" />
  <h1>Nijo App Scaffold Minimum Template (Vite + TypeScript)</h1>
`
app.append(example())
