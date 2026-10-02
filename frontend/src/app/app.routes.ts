import { Routes } from '@angular/router';
import { ListaComponent } from './pages/lista.component';
import { CadastroComponent } from './pages/cadastro.component';
import { DetalheComponent } from './pages/detalhe.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'candidatos' },
  { path: 'candidatos', component: ListaComponent },
  { path: 'candidatos/novo', component: CadastroComponent },   // antes de :id, senão "novo" seria lido como id
  { path: 'candidatos/:id', component: DetalheComponent },
  { path: '**', redirectTo: 'candidatos' }
];
