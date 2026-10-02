import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <header style="background:#0f172a; color:#fff; padding:12px 16px">
      <nav class="acoes" style="max-width:900px; margin:0 auto">
        <a href="/" style="margin-right:16px;color:#fff;font-weight:bold;font-size:1.80rem">Portal de Currículos</a>
      </nav>
    </header>
    <main><router-outlet /></main>
  `
})
export class AppComponent {}
