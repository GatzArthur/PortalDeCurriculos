import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CandidatoResumo } from '../models/candidato';
import { CandidatoService } from '../services/candidato.service';
import { mensagemDeErro } from '../shared/validacao';

@Component({
  selector: 'app-lista',
  standalone: true,
  imports: [RouterLink, FormsModule, DatePipe],
  template: `
    <h1>Candidatos</h1>
    <div class="acoes" style="margin-bottom:16px">
      <input placeholder="Buscar por nome, e-mail ou área" [(ngModel)]="busca" (keyup.enter)="carregar()" style="max-width:340px">
      <button class="btn secundario" type="button" (click)="carregar()">Buscar</button>
      <a class="btn" routerLink="/candidatos/novo" style="margin-left:auto">Novo cadastro</a>
    </div>

    @if (erro()) { <div class="aviso erro">{{ erro() }}</div> }

    <div class="card">
      @if (carregando()) {
        <p class="dica">Carregando...</p>
      } @else if (candidatos().length === 0 && !erro()) {
        <p class="dica">Nenhum candidato encontrado.</p>
      } @else {
        <table>
          <thead><tr><th>Nome</th><th>E-mail</th><th>Área/cargo</th><th>Cadastro</th><th></th></tr></thead>
          <tbody>
            @for (c of candidatos(); track c.id) {
              <tr>
                <td>{{ c.nomeCompleto }}</td>
                <td>{{ c.email }}</td>
                <td>{{ c.areaInteresse || '—' }}</td>
                <td>{{ c.criadoEm | date: 'dd/MM/yyyy HH:mm' }}</td>
                <td><a [routerLink]="['/candidatos', c.id]">Detalhes</a></td>
              </tr>
            }
          </tbody>
        </table>
      }
    </div>
  `
})
export class ListaComponent {
  private api = inject(CandidatoService);

  busca = '';
  candidatos = signal<CandidatoResumo[]>([]);
  carregando = signal(false);
  erro = signal('');

  constructor() { this.carregar(); }

  carregar(): void {
    this.carregando.set(true);
    this.erro.set('');
    this.api.listar(this.busca).subscribe({
      next: lista => { this.candidatos.set(lista); this.carregando.set(false); },
      error: err => { this.erro.set(mensagemDeErro(err)); this.candidatos.set([]); this.carregando.set(false); }
    });
  }
}
