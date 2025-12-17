import './style.css'
import myAppLogo from '/my-app.svg'

document.querySelector<HTMLDivElement>('#app')!.innerHTML = `
  <div>
    <img src="${myAppLogo}" class="logo" alt="MyApp logo" />
    <h1>Nijo App Scaffold Minimum Template (Vite + TypeScript)</h1>
  </div>
`
