import { AbstractControl, ValidationErrors } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';

// Mesmas regras do backend (CandidatoRequest.cs)
export const EMAIL_REGEX = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
export const TELEFONE_REGEX = /^[0-9()+\-.\s]{8,30}$/;
export const TAMANHO_MAX_PDF = 5 * 1024 * 1024;

/** Como Validators.required, mas também recusa texto só com espaços. */
export function obrigatorio(control: AbstractControl): ValidationErrors | null {
  return String(control.value ?? '').trim() ? null : { obrigatorio: true };
}

/** Traduz uma resposta de erro HTTP em uma mensagem amigável. */
export function mensagemDeErro(err: unknown): string {
  if (!(err instanceof HttpErrorResponse)) return 'Ocorreu um erro inesperado.';
  if (err.status === 0) return 'Não foi possível conectar ao servidor. Verifique se o backend está em execução.';
  if (typeof err.error?.detail === 'string') return err.error.detail;
  if (err.error?.errors) return Object.values<string[]>(err.error.errors).flat().join(' ');
  return 'Ocorreu um erro ao processar a solicitação. Tente novamente.';
}
