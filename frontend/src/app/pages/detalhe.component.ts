import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Candidato } from '../models/candidato';
import { CandidatoService } from '../services/candidato.service';
import { mensagemDeErro } from '../shared/validacao';

@Component({
  selector: 'app-detalhe',
  standalone: true,
  imports: [RouterLink, DatePipe],
  template: `
    <h1>Detalhes do candidato</h1>

    @if (salvo) { <div class="aviso ok" role="status">Cadastro salvo com sucesso.</div> }
    @if (erro()) { <div class="aviso erro">{{ erro() }}</div> }

    @if (candidato(); as c) {
      <div class="card">
        <p><strong>Nome completo</strong><br>{{ c.nomeCompleto }}</p>
        <p><strong>E-mail</strong><br>{{ c.email }}</p>
        <p><strong>Telefone</strong><br>{{ c.telefone || '—' }}</p>
        <p><strong>Área ou cargo de interesse</strong><br>{{ c.areaInteresse || '—' }}</p>
        <p><strong>Resumo profissional</strong><br><span style="white-space:pre-wrap">{{ c.resumoProfissional || '—' }}</span></p>
        <p class="dica">Cadastrado em {{ c.criadoEm | date: 'dd/MM/yyyy HH:mm' }}</p>
      </div>
    }

    <p><a routerLink="/candidatos">← Voltar para a listagem</a></p>
  `
})
export class DetalheComponent {
  private api = inject(CandidatoService);
  private route = inject(ActivatedRoute);

  // Definido pela tela de cadastro ao navegar após salvar.
  salvo = inject(Router).getCurrentNavigation()?.extras.state?.['salvo'] === true;

  candidato = signal<Candidato | null>(null);
  erro = signal('');

  constructor() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.api.obter(id).subscribe({
      next: c => this.candidato.set(c),
      error: err => this.erro.set(mensagemDeErro(err))
    });
  }
}
