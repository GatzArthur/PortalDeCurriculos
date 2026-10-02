import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CandidatoService } from '../services/candidato.service';
import { EMAIL_REGEX, TAMANHO_MAX_PDF, TELEFONE_REGEX, mensagemDeErro, obrigatorio } from '../shared/validacao';

interface Aviso { tipo: 'ok' | 'erro' | 'info'; texto: string; }

@Component({
  selector: 'app-cadastro',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <h1>Novo candidato</h1>

    <section class="card" style="margin-bottom:16px">
      <label for="pdf">Importar currículo em PDF (opcional)</label>
      <input id="pdf" type="file" accept="application/pdf,.pdf" (change)="aoEscolherArquivo($event)" [disabled]="lendoPdf()">
      <p class="dica">Até 5 MB. Os dados encontrados preenchem o formulário abaixo e podem ser corrigidos antes de salvar.</p>
      @if (lendoPdf()) { <p class="dica">Lendo o PDF...</p> }
    </section>

    @if (aviso(); as a) { <div [class]="'aviso ' + a.tipo" role="status">{{ a.texto }}</div> }

    <form class="card" [formGroup]="form" (ngSubmit)="salvar()" novalidate>
      <div class="campo">
        <label for="nomeCompleto">Nome completo *</label>
        <input id="nomeCompleto" formControlName="nomeCompleto" [class.invalido]="invalido('nomeCompleto')">
        @if (invalido('nomeCompleto')) { <div class="erro-campo">{{ erro('nomeCompleto') }}</div> }
      </div>
      <div class="campo">
        <label for="email">E-mail *</label>
        <input id="email" type="email" formControlName="email" [class.invalido]="invalido('email')">
        @if (invalido('email')) { <div class="erro-campo">{{ erro('email') }}</div> }
      </div>
      <div class="campo">
        <label for="telefone">Telefone</label>
        <input id="telefone" formControlName="telefone" [class.invalido]="invalido('telefone')">
        @if (invalido('telefone')) { <div class="erro-campo">{{ erro('telefone') }}</div> }
      </div>
      <div class="campo">
        <label for="areaInteresse">Área ou cargo de interesse</label>
        <input id="areaInteresse" formControlName="areaInteresse" [class.invalido]="invalido('areaInteresse')">
        @if (invalido('areaInteresse')) { <div class="erro-campo">{{ erro('areaInteresse') }}</div> }
      </div>
      <div class="campo">
        <label for="resumoProfissional">Resumo profissional</label>
        <textarea id="resumoProfissional" rows="6" formControlName="resumoProfissional" [class.invalido]="invalido('resumoProfissional')"></textarea>
        @if (invalido('resumoProfissional')) { <div class="erro-campo">{{ erro('resumoProfissional') }}</div> }
      </div>
      <div class="acoes">
        <button class="btn" type="submit" [disabled]="salvando()">{{ salvando() ? 'Salvando...' : 'Salvar' }}</button>
        <a class="btn secundario" routerLink="/candidatos">Cancelar</a>
      </div>
    </form>
  `
})
export class CadastroComponent {
  private fb = inject(FormBuilder);
  private api = inject(CandidatoService);
  private router = inject(Router);

  // Um único formulário para os dois caminhos (manual e PDF).
  form = this.fb.nonNullable.group({
    nomeCompleto: ['', [obrigatorio, Validators.maxLength(150)]],
    email: ['', [obrigatorio, Validators.pattern(EMAIL_REGEX), Validators.maxLength(254)]],
    telefone: ['', [Validators.pattern(TELEFONE_REGEX), Validators.maxLength(30)]],
    areaInteresse: ['', [Validators.maxLength(100)]],
    resumoProfissional: ['', [Validators.maxLength(4000)]]
  });

  aviso = signal<Aviso | null>(null);
  lendoPdf = signal(false);
  salvando = signal(false);

  aoEscolherArquivo(evento: Event): void {
    const input = evento.target as HTMLInputElement;
    const arquivo = input.files?.[0];
    if (!arquivo) return;

    const erroLocal = this.validarArquivo(arquivo);
    if (erroLocal) {
      this.aviso.set({ tipo: 'erro', texto: erroLocal });
      input.value = '';
      return;
    }

    this.lendoPdf.set(true);
    this.aviso.set(null);

    this.api.extrairDoPdf(arquivo).subscribe({
      next: dados => {
        this.lendoPdf.set(false);
        // Só preenche o que foi encontrado; nada é sobrescrito com vazio.
        const encontrados: Record<string, string> = {};
        if (dados.nomeCompleto) encontrados['nomeCompleto'] = dados.nomeCompleto;
        if (dados.email) encontrados['email'] = dados.email;
        if (dados.telefone) encontrados['telefone'] = dados.telefone;
        this.form.patchValue(encontrados);

        const qtd = Object.keys(encontrados).length;
        this.aviso.set(qtd
          ? { tipo: 'info', texto: `PDF lido. ${qtd} campo(s) preenchido(s). Revise as informações e complete o que faltar.` }
          : { tipo: 'info', texto: 'PDF lido, mas não foi possível identificar nome, e-mail ou telefone. Preencha o formulário manualmente.' });
      },
      error: err => {
        this.lendoPdf.set(false);
        // A falha na leitura nunca bloqueia o cadastro manual.
        this.aviso.set({ tipo: 'erro', texto: mensagemDeErro(err) });
      }
    });
    input.value = '';
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.aviso.set({ tipo: 'erro', texto: 'Corrija os campos destacados antes de salvar.' });
      return;
    }

    this.salvando.set(true);
    const v = this.form.getRawValue();
    this.api.criar({ ...v, nomeCompleto: v.nomeCompleto.trim(), email: v.email.trim() }).subscribe({
      next: candidato => {
        this.salvando.set(false);
        this.router.navigate(['/candidatos', candidato.id], { state: { salvo: true } });
      },
      error: err => {
        this.salvando.set(false);
        this.aplicarErrosDoServidor(err);
        this.aviso.set({ tipo: 'erro', texto: mensagemDeErro(err) });
      }
    });
  }

  invalido(campo: string): boolean {
    const c = this.form.get(campo);
    return !!c && c.invalid && (c.touched || c.dirty);
  }

  erro(campo: string): string {
    const e = this.form.get(campo)?.errors;
    if (!e) return '';
    if (e['obrigatorio']) return campo === 'email' ? 'Informe o e-mail.' : 'Informe o nome completo.';
    if (e['pattern']) return campo === 'email' ? 'Informe um e-mail válido.' : 'Informe um telefone válido (números, espaços, +, -, ( e )).';
    if (e['maxlength']) return `Máximo de ${e['maxlength'].requiredLength} caracteres.`;
    return e['servidor'] ?? 'Valor inválido.';
  }

  private validarArquivo(arquivo: File): string | null {
    if (!arquivo.name.toLowerCase().endsWith('.pdf')) return 'Formato inválido. Envie um arquivo PDF.';
    if (arquivo.size > TAMANHO_MAX_PDF) return 'O arquivo excede o tamanho máximo de 5 MB.';
    return null;
  }

  private aplicarErrosDoServidor(err: unknown): void {
    if (!(err instanceof HttpErrorResponse) || err.status !== 400 || !err.error?.errors) return;
    for (const [chave, mensagens] of Object.entries<string[]>(err.error.errors)) {
      const nome = chave.charAt(0).toLowerCase() + chave.slice(1);
      this.form.get(nome)?.setErrors({ servidor: mensagens[0] });
      this.form.get(nome)?.markAsTouched();
    }
  }
}
